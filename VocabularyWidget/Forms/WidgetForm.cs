using System.Drawing.Drawing2D;
using VocabularyWidget.Models;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms;

public class WidgetForm : Form
{
    private static readonly Size IdleSize = new(260, 110);
    private static readonly Size QuizSize = new(300, 252);
    private static readonly Color IdleBorder = Color.FromArgb(70, 72, 76);

    private readonly DataService _dataService;
    private readonly ReviewScheduler _scheduler = new();
    private readonly Random _random = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly System.Windows.Forms.Timer _feedbackTimer;

    private List<WordItem> _wordList;
    private AppSettings _settings;
    private WordItem? _currentWord;
    private MultipleChoiceQuestion? _question;
    private bool _isShowingWord;
    private bool _inQuiz;
    private bool _inFeedback;
    private bool _dragging;
    private bool _dragMoved;
    private Point _dragCursor;
    private Point _dragForm;
    private Color _borderColor = IdleBorder;

    private readonly Label _lblContent;
    private readonly Panel _quizPanel;
    private readonly Label _lblPrompt;
    private readonly Label[] _choiceLabels = new Label[4];
    private readonly Label _lblStatus;
    private readonly NotifyIcon _tray;
    private readonly ContextMenuStrip _contextMenu;
    private ManageForm? _manageForm;

    public WidgetForm()
    {
        _dataService = new DataService();
        _wordList = _dataService.LoadWords();
        _settings = _dataService.LoadSettings();

        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(32, 33, 36);
        StartPosition = FormStartPosition.Manual;
        Size = IdleSize;
        DoubleBuffered = true;
        KeyPreview = true;
        Text = "Vocabulary Widget";
        Padding = new Padding(2);
        Font = UiFont(9);

        Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        Location = new Point(workingArea.Right - Width - 20, workingArea.Bottom - Height - 40);

        _lblContent = new Label
        {
            ForeColor = Color.White,
            Font = UiFont(12, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent
        };
        _lblContent.MouseDown += OnPointerDown;
        _lblContent.MouseMove += OnPointerMove;
        _lblContent.MouseUp += OnPointerUp;

        _lblPrompt = new Label
        {
            Dock = DockStyle.Top,
            Height = 36,
            ForeColor = Color.White,
            Font = UiFont(11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _quizPanel = new Panel { Dock = DockStyle.Fill, Visible = false, Padding = new Padding(8, 0, 8, 4) };
        var choices = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0)
        };
        choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++)
        {
            choices.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            int index = i;
            var choice = new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Padding = new Padding(8, 2, 8, 2),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(48, 50, 54),
                Font = UiFont(8.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            choice.Click += (_, _) => OnChoiceClicked(index);
            _choiceLabels[i] = choice;
            choices.Controls.Add(choice, 0, i);
        }

        _quizPanel.Controls.Add(choices);
        _quizPanel.Controls.Add(_lblPrompt);

        _lblStatus = new Label
        {
            ForeColor = Color.FromArgb(160, 160, 160),
            Font = UiFont(8),
            Dock = DockStyle.Bottom,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 22,
            Text = "Click để trắc nghiệm",
            Cursor = Cursors.SizeAll
        };
        _lblStatus.MouseDown += OnPointerDown;
        _lblStatus.MouseMove += OnPointerMove;
        _lblStatus.MouseUp += OnPointerUp;

        MouseDown += OnPointerDown;
        MouseMove += OnPointerMove;
        MouseUp += OnPointerUp;
        KeyDown += OnWidgetKeyDown;
        Paint += (_, e) =>
        {
            using var pen = new Pen(_borderColor, 2);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
        };

        _contextMenu = BuildContextMenu();
        ContextMenuStrip = _contextMenu;
        _lblContent.ContextMenuStrip = _contextMenu;
        _lblStatus.ContextMenuStrip = _contextMenu;

        Controls.Add(_quizPanel);
        Controls.Add(_lblContent);
        Controls.Add(_lblStatus);

        _tray = new NotifyIcon
        {
            Text = "Vocabulary Widget",
            Visible = true,
            Icon = SystemIcons.Information,
            ContextMenuStrip = _contextMenu
        };
        _tray.DoubleClick += (_, _) =>
        {
            Visible = true;
            Activate();
        };

        _timer = new System.Windows.Forms.Timer { Interval = _settings.TimerMilliseconds };
        _timer.Tick += (_, _) => DisplayRandomWord();

        _feedbackTimer = new System.Windows.Forms.Timer();
        _feedbackTimer.Tick += OnFeedbackElapsed;

        FormClosed += (_, _) =>
        {
            _tray.Visible = false;
            _tray.Dispose();
        };

        DisplayRandomWord();
        _timer.Start();
    }

    private static Font UiFont(float size, FontStyle style = FontStyle.Regular)
    {
        foreach (string name in new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "Segoe UI" })
        {
            try
            {
                return new Font(name, size, style, GraphicsUnit.Point);
            }
            catch
            {
                // try next family
            }
        }

        return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Quản lý từ vựng", null, (_, _) => OpenManager());
        menu.Items.Add("Đổi từ khác ngay lập tức", null, (_, _) =>
        {
            CancelQuizUi();
            DisplayRandomWord();
            RestartRotationTimer();
        });

        var interval = new ToolStripMenuItem("Chỉnh thời gian");
        foreach (int minutes in AppSettings.AllowedMinutes)
        {
            int captured = minutes;
            var item = new ToolStripMenuItem($"{captured} phút")
            {
                Checked = captured == _settings.TimerMinutes,
                Tag = captured
            };
            item.Click += (_, _) => SetInterval(captured);
            interval.DropDownItems.Add(item);
        }

        menu.Items.Add(interval);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Thoát ứng dụng", null, (_, _) => Application.Exit());
        menu.Opening += (_, _) => RefreshIntervalChecks();
        return menu;
    }

    private void RefreshIntervalChecks()
    {
        if (_contextMenu.Items[2] is not ToolStripMenuItem interval)
        {
            return;
        }

        foreach (ToolStripItem raw in interval.DropDownItems)
        {
            if (raw is ToolStripMenuItem item && item.Tag is int minutes)
            {
                item.Checked = minutes == _settings.TimerMinutes;
            }
        }
    }

    private void SetInterval(int minutes)
    {
        _settings.SetTimerMinutes(minutes);
        _dataService.SaveSettings(_settings);
        _timer.Interval = _settings.TimerMilliseconds;
        RefreshIntervalChecks();
        if (!_inQuiz && !_inFeedback)
        {
            RestartRotationTimer();
        }
    }

    private void OpenManager()
    {
        if (_manageForm is { IsDisposed: false })
        {
            _manageForm.ReloadFromDisk();
            _manageForm.BringToFront();
            _manageForm.Activate();
            return;
        }

        _manageForm = new ManageForm(_dataService);
        _manageForm.WordsChanged += OnWordsChangedFromManager;
        _manageForm.FormClosed += (_, _) => _manageForm = null;
        _manageForm.Show(this);
    }

    private void OnWordsChangedFromManager(List<WordItem> words)
    {
        _wordList = words;
        if (_currentWord == null || _wordList.All(w => w.Id != _currentWord.Id))
        {
            DisplayRandomWord();
        }
    }

    private void OnPointerDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _inQuiz)
        {
            return;
        }

        _dragging = true;
        _dragMoved = false;
        _dragCursor = Cursor.Position;
        _dragForm = Location;
    }

    private void OnPointerMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        var delta = new Point(Cursor.Position.X - _dragCursor.X, Cursor.Position.Y - _dragCursor.Y);
        if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
        {
            _dragMoved = true;
        }

        if (_dragMoved)
        {
            Location = new Point(_dragForm.X + delta.X, _dragForm.Y + delta.Y);
        }
    }

    private void OnPointerUp(object? sender, MouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        if (!_dragMoved && e.Button == MouseButtons.Left && !_inFeedback)
        {
            StartQuizMode();
        }
    }

    private void OnWidgetKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_inQuiz || _question == null)
        {
            return;
        }

        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            CancelQuizUi();
            ShowPrompt();
            RestartRotationTimer();
            return;
        }

        int index = e.KeyCode switch
        {
            Keys.D1 or Keys.NumPad1 => 0,
            Keys.D2 or Keys.NumPad2 => 1,
            Keys.D3 or Keys.NumPad3 => 2,
            Keys.D4 or Keys.NumPad4 => 3,
            Keys.A => 0,
            Keys.B => 1,
            Keys.C => 2,
            Keys.D => 3,
            _ => -1
        };
        if (index >= 0)
        {
            e.SuppressKeyPress = true;
            OnChoiceClicked(index);
        }
    }

    private void DisplayRandomWord()
    {
        _inQuiz = false;
        _inFeedback = false;
        _question = null;
        ApplyIdleLayout();
        _borderColor = IdleBorder;
        Invalidate();

        _currentWord = _scheduler.PickNext(_wordList, _currentWord);
        if (_currentWord == null)
        {
            _lblContent.Visible = true;
            _quizPanel.Visible = false;
            _lblContent.Text = "Chưa có từ vựng!";
            _lblStatus.Text = "Chuột phải → Quản lý từ vựng";
            _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
            return;
        }

        _isShowingWord = ReviewScheduler.CoinFlipShowWord();
        ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (_currentWord == null)
        {
            return;
        }

        ApplyIdleLayout();
        _lblContent.Visible = true;
        _quizPanel.Visible = false;
        _lblContent.ForeColor = Color.White;
        _lblContent.Text = _isShowingWord ? _currentWord.Word : _currentWord.Definition;
        _lblStatus.Text = _isShowingWord ? "Click → chọn định nghĩa" : "Click → chọn từ vựng";
        _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
    }

    private void StartQuizMode()
    {
        if (_currentWord == null || _inQuiz)
        {
            return;
        }

        _question = MultipleChoiceBuilder.Build(_wordList, _currentWord, _isShowingWord, _random);
        _inQuiz = true;
        _timer.Stop();
        ApplyQuizLayout();
        _lblContent.Visible = false;
        _quizPanel.Visible = true;
        _lblPrompt.Text = _question.Prompt;
        for (int i = 0; i < _choiceLabels.Length; i++)
        {
            if (i < _question.Options.Count)
            {
                _choiceLabels[i].Visible = true;
                _choiceLabels[i].Enabled = true;
                _choiceLabels[i].BackColor = Color.FromArgb(48, 50, 54);
                _choiceLabels[i].ForeColor = Color.White;
                _choiceLabels[i].Text = $"{i + 1}. {_question.Options[i]}";
            }
            else
            {
                _choiceLabels[i].Visible = false;
            }
        }

        _lblStatus.Text = "Chọn đáp án · Esc hủy";
        _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
    }

    private void OnChoiceClicked(int index)
    {
        if (!_inQuiz || _question == null || index < 0 || index >= _question.Options.Count)
        {
            return;
        }

        string choice = _question.Options[index];
        bool isCorrect = _question.IsCorrect(choice);
        Grade(isCorrect, choice);
    }

    private void CancelQuizUi()
    {
        _inQuiz = false;
        _inFeedback = false;
        _question = null;
        _feedbackTimer.Stop();
        ApplyIdleLayout();
        _quizPanel.Visible = false;
        _lblContent.Visible = true;
        _borderColor = IdleBorder;
        Invalidate();
    }

    private void Grade(bool isCorrect, string chosen)
    {
        if (_currentWord == null || _question == null)
        {
            return;
        }

        _currentWord.ReviewCount++;
        if (isCorrect)
        {
            _currentWord.CorrectCount++;
        }

        _dataService.SaveWords(_wordList);

        _inQuiz = false;
        _inFeedback = true;
        ApplyIdleLayout();
        _quizPanel.Visible = false;
        _lblContent.Visible = true;

        if (isCorrect)
        {
            _borderColor = Color.FromArgb(46, 204, 113);
            _lblContent.ForeColor = Color.FromArgb(46, 204, 113);
            _lblContent.Text = "Chính xác!";
            _lblStatus.Text = _question.CorrectAnswer;
            _lblStatus.ForeColor = Color.FromArgb(144, 238, 144);
            _feedbackTimer.Interval = 1500;
            _feedbackTimer.Tag = true;
        }
        else
        {
            _borderColor = Color.FromArgb(231, 76, 60);
            _lblContent.ForeColor = Color.FromArgb(255, 160, 150);
            _lblContent.Text = $"Sai rồi! Đáp án: {_question.CorrectAnswer}";
            _lblStatus.Text = $"Bạn chọn: {chosen}";
            _lblStatus.ForeColor = Color.Salmon;
            _feedbackTimer.Interval = 2000;
            _feedbackTimer.Tag = false;
        }

        Invalidate();
        _feedbackTimer.Stop();
        _feedbackTimer.Start();
    }

    private void OnFeedbackElapsed(object? sender, EventArgs e)
    {
        _feedbackTimer.Stop();
        _inFeedback = false;
        bool wasCorrect = _feedbackTimer.Tag is true;
        _borderColor = IdleBorder;
        Invalidate();

        if (wasCorrect)
        {
            DisplayRandomWord();
        }
        else
        {
            ShowPrompt();
        }

        RestartRotationTimer();
    }

    private void ApplyIdleLayout()
    {
        AnchorBottomRight(IdleSize);
    }

    private void ApplyQuizLayout()
    {
        AnchorBottomRight(QuizSize);
    }

    private void AnchorBottomRight(Size next)
    {
        int right = Location.X + Width;
        int bottom = Location.Y + Height;
        Size = next;
        Location = new Point(right - Width, bottom - Height);
    }

    private void RestartRotationTimer()
    {
        _timer.Stop();
        _timer.Interval = _settings.TimerMilliseconds;
        _timer.Start();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int CS_DROPSHADOW = 0x00020000;
            CreateParams cp = base.CreateParams;
            cp.ClassStyle |= CS_DROPSHADOW;
            return cp;
        }
    }
}
