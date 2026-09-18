using System.Drawing.Drawing2D;
using VocabularyWidget.Models;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms;

public class WidgetForm : Form
{
    private readonly DataService _dataService;
    private readonly ReviewScheduler _scheduler = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly System.Windows.Forms.Timer _feedbackTimer;

    private List<WordItem> _wordList;
    private AppSettings _settings;
    private WordItem? _currentWord;
    private bool _isShowingWord;
    private bool _inQuiz;
    private bool _inFeedback;
    private bool _dragging;
    private bool _dragMoved;
    private Point _dragCursor;
    private Point _dragForm;
    private Color _borderColor = Color.FromArgb(70, 72, 76);

    private readonly Label _lblContent;
    private readonly TextBox _txtAnswer;
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
        Size = new Size(260, 110);
        DoubleBuffered = true;
        Text = "Vocabulary Widget";
        Padding = new Padding(2);

        Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        Location = new Point(workingArea.Right - Width - 20, workingArea.Bottom - Height - 40);

        _lblContent = new Label
        {
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent
        };
        _lblContent.MouseDown += OnPointerDown;
        _lblContent.MouseMove += OnPointerMove;
        _lblContent.MouseUp += OnPointerUp;

        _txtAnswer = new TextBox
        {
            Visible = false,
            Font = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle,
            Width = 220,
            Location = new Point(20, 36)
        };
        _txtAnswer.KeyDown += TxtAnswer_KeyDown;

        _lblStatus = new Label
        {
            ForeColor = Color.FromArgb(160, 160, 160),
            Font = new Font("Segoe UI", 8),
            Dock = DockStyle.Bottom,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 22,
            Text = "Click để kiểm tra",
            Cursor = Cursors.SizeAll
        };
        _lblStatus.MouseDown += OnPointerDown;
        _lblStatus.MouseMove += OnPointerMove;
        _lblStatus.MouseUp += OnPointerUp;

        MouseDown += OnPointerDown;
        MouseMove += OnPointerMove;
        MouseUp += OnPointerUp;
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

        Controls.Add(_txtAnswer);
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

    private void DisplayRandomWord()
    {
        _inQuiz = false;
        _inFeedback = false;
        _borderColor = Color.FromArgb(70, 72, 76);
        Invalidate();

        _currentWord = _scheduler.PickNext(_wordList, _currentWord);
        if (_currentWord == null)
        {
            _lblContent.Visible = true;
            _txtAnswer.Visible = false;
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

        _lblContent.Visible = true;
        _txtAnswer.Visible = false;
        _lblContent.ForeColor = Color.White;
        _lblContent.Text = _isShowingWord ? _currentWord.Word : _currentWord.Definition;
        _lblStatus.Text = _isShowingWord ? "Nhập định nghĩa..." : "Nhập từ vựng...";
        _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
    }

    private void StartQuizMode()
    {
        if (_currentWord == null || _inQuiz)
        {
            return;
        }

        _inQuiz = true;
        _timer.Stop();
        _lblContent.Visible = false;
        _txtAnswer.Visible = true;
        _txtAnswer.Clear();
        _txtAnswer.ForeColor = Color.Black;
        _txtAnswer.BackColor = Color.White;
        _txtAnswer.Focus();
        _lblStatus.Text = _isShowingWord ? "Enter = gửi · Esc = hủy" : "Enter = gửi · Esc = hủy";
    }

    private void TxtAnswer_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            CancelQuizUi();
            ShowPrompt();
            RestartRotationTimer();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            CheckAnswer();
        }
    }

    private void CancelQuizUi()
    {
        _inQuiz = false;
        _inFeedback = false;
        _feedbackTimer.Stop();
        _txtAnswer.Visible = false;
        _lblContent.Visible = true;
        _borderColor = Color.FromArgb(70, 72, 76);
        Invalidate();
    }

    private void CheckAnswer()
    {
        if (_currentWord == null)
        {
            return;
        }

        string target = _isShowingWord ? _currentWord.Definition : _currentWord.Word;
        string input = _txtAnswer.Text;
        bool isCorrect = AnswerChecker.IsCorrect(input, target);

        _currentWord.ReviewCount++;
        if (isCorrect)
        {
            _currentWord.CorrectCount++;
        }

        _dataService.SaveWords(_wordList);

        _inQuiz = false;
        _inFeedback = true;
        _txtAnswer.Visible = false;
        _lblContent.Visible = true;

        if (isCorrect)
        {
            _borderColor = Color.FromArgb(46, 204, 113);
            _lblContent.ForeColor = Color.FromArgb(46, 204, 113);
            _lblContent.Text = "Chính xác!";
            _lblStatus.Text = target;
            _lblStatus.ForeColor = Color.FromArgb(144, 238, 144);
            _feedbackTimer.Interval = 1500;
            _feedbackTimer.Tag = true;
        }
        else
        {
            _borderColor = Color.FromArgb(231, 76, 60);
            _lblContent.ForeColor = Color.FromArgb(255, 160, 150);
            _lblContent.Text = $"Sai rồi! Đáp án: {target}";
            _lblStatus.Text = string.IsNullOrWhiteSpace(input)
                ? "Bạn chưa nhập gì"
                : $"Bạn đã nhập: {input.Trim()}";
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
        _borderColor = Color.FromArgb(70, 72, 76);
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
