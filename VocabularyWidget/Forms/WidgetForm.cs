using System.Drawing.Drawing2D;
using VocabularyWidget.Models;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms;

public class WidgetForm : Form
{
    private static readonly Size IdleSize = new(260, 110);
    private static readonly Size QuizSize = new(300, 252);
    private static readonly Size WriteSize = new(320, 176);
    private const int WorkBarHeight = 64;
    private static readonly Color IdleBorder = Color.FromArgb(70, 72, 76);

    private readonly DataService _dataService;
    private readonly ReviewScheduler _scheduler = new();
    private readonly Random _random = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly System.Windows.Forms.Timer _feedbackTimer;
    private readonly System.Windows.Forms.Timer _marqueeTimer;

    private List<WordItem> _wordList;
    private AppSettings _settings;
    private WordItem? _currentWord;
    private MultipleChoiceQuestion? _question;
    private bool _isShowingWord;
    private bool _inQuiz;
    private bool _inFeedback;
    private bool _dragging;
    private bool _dragMoved;
    private bool _workChrome;
    private bool _savedIdleLocation;
    private Point _idleLocation;
    private Point _dragCursor;
    private Point _dragForm;
    private Color _borderColor = IdleBorder;
    private float _marqueeX;

    private readonly Label _lblContent;
    private readonly Panel _quizPanel;
    private readonly Panel _workPanel;
    private readonly Label _lblMarquee;
    private readonly Label _lblPrompt;
    private readonly Label[] _choiceLabels = new Label[4];
    private readonly Label _lblStatus;
    private readonly TextBox _writeBox;
    private readonly SpeechService _speech = new();
    private readonly Button _btnSpeak;
    private readonly NotifyIcon _tray;
    private readonly ContextMenuStrip _contextMenu;
    private readonly Dictionary<WidgetMode, ToolStripMenuItem> _modeItems = new();
    private ToolStripMenuItem _autoSpeakItem = null!;
    private ToolStripMenuItem _feedbackSoundItem = null!;
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

        _idleLocation = FallbackIdleLocation();
        Location = _idleLocation;
        _savedIdleLocation = true;

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

        _workPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Visible = false,
            BackColor = Color.Transparent,
            Padding = new Padding(12, 0, 40, 0)
        };
        _lblMarquee = new Label
        {
            AutoSize = true,
            ForeColor = Color.White,
            Font = UiFont(26, FontStyle.Bold),
            BackColor = Color.Transparent,
            UseMnemonic = false
        };
        _workPanel.Controls.Add(_lblMarquee);
        _workPanel.MouseDown += OnPointerDown;
        _lblMarquee.MouseDown += OnPointerDown;

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

        _writeBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Visible = false,
            Font = UiFont(11),
            PlaceholderText = "Gõ một từ rồi Enter",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(48, 50, 54),
            ForeColor = Color.White
        };
        _writeBox.KeyDown += OnWriteKeyDown;

        _btnSpeak = new Button
        {
            Text = "听",
            Size = new Size(28, 22),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(48, 50, 54),
            Font = UiFont(9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnSpeak.FlatAppearance.BorderSize = 0;
        _btnSpeak.Location = new Point(Width - _btnSpeak.Width - 8, 6);
        _btnSpeak.Click += (_, _) => SpeakCurrent(fromUser: true);
        var speakTip = new ToolTip();
        speakTip.SetToolTip(_btnSpeak, "Nghe phát âm chữ Hán (phím S)");

        MouseDown += OnPointerDown;
        MouseMove += OnPointerMove;
        MouseUp += OnPointerUp;
        KeyDown += OnWidgetKeyDown;
        Resize += (_, _) => PositionSpeakButton();
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
        _workPanel.ContextMenuStrip = _contextMenu;
        _lblMarquee.ContextMenuStrip = _contextMenu;
        _writeBox.ContextMenuStrip = _contextMenu;

        Controls.Add(_workPanel);
        Controls.Add(_quizPanel);
        Controls.Add(_lblContent);
        Controls.Add(_writeBox);
        Controls.Add(_lblStatus);
        Controls.Add(_btnSpeak);
        _btnSpeak.BringToFront();

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

        _timer = new System.Windows.Forms.Timer { Interval = RotationIntervalMs() };
        _timer.Tick += (_, _) => DisplayRandomWord();

        _feedbackTimer = new System.Windows.Forms.Timer();
        _feedbackTimer.Tick += OnFeedbackElapsed;

        _marqueeTimer = new System.Windows.Forms.Timer { Interval = 30 };
        _marqueeTimer.Tick += OnMarqueeTick;

        FormClosed += (_, _) =>
        {
            _marqueeTimer.Stop();
            _tray.Visible = false;
            _tray.Dispose();
            _speech.Dispose();
        };

        Shown += (_, _) => FocusWriteBox();

        ApplyModeChrome();
        DisplayRandomWord();
        RestartRotationTimer();
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
            CancelInteractiveUi();
            DisplayRandomWord();
            RestartRotationTimer();
        });

        var modes = new ToolStripMenuItem("Chế độ");
        AddModeItem(modes, WidgetMode.Quiz, "Trắc nghiệm (4 đáp án)");
        AddModeItem(modes, WidgetMode.Listen, "Nghe (tự phát âm)");
        AddModeItem(modes, WidgetMode.Work, "Làm việc (thanh trên màn hình)");
        AddModeItem(modes, WidgetMode.Write, "Viết (gõ một từ)");
        menu.Items.Add(modes);

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
        menu.Items.Add("Nghe phát âm", null, (_, _) => SpeakCurrent(fromUser: true));
        _autoSpeakItem = new ToolStripMenuItem("Tự phát âm khi hiện thẻ")
        {
            Checked = _settings.AutoSpeakHanzi,
            CheckOnClick = true
        };
        _autoSpeakItem.Click += (_, _) =>
        {
            _settings.AutoSpeakHanzi = _autoSpeakItem.Checked;
            _dataService.SaveSettings(_settings);
        };
        _feedbackSoundItem = new ToolStripMenuItem("Âm thanh đúng/sai")
        {
            Checked = _settings.FeedbackSounds,
            CheckOnClick = true
        };
        _feedbackSoundItem.Click += (_, _) =>
        {
            _settings.FeedbackSounds = _feedbackSoundItem.Checked;
            _dataService.SaveSettings(_settings);
        };
        menu.Items.Add(_autoSpeakItem);
        menu.Items.Add(_feedbackSoundItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Thoát ứng dụng", null, (_, _) => Application.Exit());
        menu.Opening += (_, _) => RefreshMenuChecks();
        return menu;
    }

    private void AddModeItem(ToolStripMenuItem parent, WidgetMode mode, string text)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += (_, _) => SetMode(mode);
        parent.DropDownItems.Add(item);
        _modeItems[mode] = item;
    }

    private void RefreshMenuChecks()
    {
        WidgetMode current = _settings.GetMode();
        foreach (var pair in _modeItems)
        {
            pair.Value.Checked = pair.Key == current;
        }

        if (_contextMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == "Chỉnh thời gian")
            is ToolStripMenuItem interval)
        {
            foreach (ToolStripItem raw in interval.DropDownItems)
            {
                if (raw is ToolStripMenuItem item && item.Tag is int minutes)
                {
                    item.Checked = minutes == _settings.TimerMinutes;
                }
            }
        }

        _autoSpeakItem.Checked = _settings.AutoSpeakHanzi;
        _feedbackSoundItem.Checked = _settings.FeedbackSounds;
    }

    private void SetMode(WidgetMode mode)
    {
        if (_settings.GetMode() == mode)
        {
            return;
        }

        CancelInteractiveUi();
        _settings.SetMode(mode);
        _dataService.SaveSettings(_settings);
        ApplyModeChrome();
        DisplayRandomWord();
        RestartRotationTimer();
    }

    private void SetInterval(int minutes)
    {
        _settings.SetTimerMinutes(minutes);
        _dataService.SaveSettings(_settings);
        RefreshMenuChecks();
        if (!_inQuiz && !_inFeedback && _settings.GetMode() is WidgetMode.Quiz or WidgetMode.Listen)
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

    private bool AllowsDrag => _settings.GetMode() != WidgetMode.Work;

    private void OnPointerDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _inQuiz || !AllowsDrag)
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
            _idleLocation = Location;
            _savedIdleLocation = true;
        }
    }

    private void OnPointerUp(object? sender, MouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        if (!_dragMoved && e.Button == MouseButtons.Left && !_inFeedback
            && _settings.GetMode() == WidgetMode.Quiz)
        {
            StartQuizMode();
        }
    }

    private void OnWidgetKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.S && !_writeBox.Focused)
        {
            e.SuppressKeyPress = true;
            SpeakCurrent(fromUser: true);
            return;
        }

        if (_settings.GetMode() == WidgetMode.Write && !_inFeedback && _currentWord != null)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SubmitWrite();
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                _writeBox.Clear();
                _writeBox.Focus();
                return;
            }
        }

        if (!_inQuiz || _question == null)
        {
            return;
        }

        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            CancelInteractiveUi();
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

    private void OnWriteKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            SubmitWrite();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            _writeBox.Clear();
        }
    }

    private void DisplayRandomWord()
    {
        _inQuiz = false;
        _inFeedback = false;
        _question = null;
        ApplyModeChrome();
        _borderColor = IdleBorder;
        Invalidate();

        _currentWord = _scheduler.PickNext(_wordList, _currentWord);
        if (_currentWord == null)
        {
            ShowEmptyState();
            return;
        }

        _isShowingWord = ReviewScheduler.CoinFlipShowWord();
        ShowPrompt();
    }

    private void ShowEmptyState()
    {
        StopMarquee();
        _workPanel.Visible = _settings.GetMode() == WidgetMode.Work;
        _quizPanel.Visible = false;
        _writeBox.Visible = false;
        _lblContent.Visible = _settings.GetMode() != WidgetMode.Work;
        _lblStatus.Visible = _settings.GetMode() != WidgetMode.Work;
        if (_settings.GetMode() == WidgetMode.Work)
        {
            StartMarquee("Chưa có từ vựng — chuột phải → Quản lý từ vựng");
        }
        else
        {
            _lblContent.Text = "Chưa có từ vựng!";
            _lblStatus.Text = "Chuột phải → Quản lý từ vựng";
            _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
        }
    }

    private void ShowPrompt()
    {
        if (_currentWord == null)
        {
            ShowEmptyState();
            return;
        }

        ApplyModeChrome();
        _lblContent.ForeColor = Color.White;
        _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);

        switch (_settings.GetMode())
        {
            case WidgetMode.Listen:
                ShowListenPrompt();
                break;
            case WidgetMode.Work:
                ShowWorkPrompt();
                break;
            case WidgetMode.Write:
                ShowWritePrompt();
                break;
            default:
                ShowQuizIdlePrompt();
                break;
        }
    }

    private void ShowQuizIdlePrompt()
    {
        if (_currentWord == null)
        {
            return;
        }

        _lblContent.Visible = true;
        _quizPanel.Visible = false;
        _writeBox.Visible = false;
        _workPanel.Visible = false;
        _lblStatus.Visible = true;
        _lblContent.Text = _isShowingWord ? _currentWord.Word : _currentWord.Definition;
        _lblStatus.Text = _isShowingWord ? "听 nghe · click quiz" : "听 chữ Hán · click quiz";
        MaybeAutoSpeak();
    }

    private void ShowListenPrompt()
    {
        if (_currentWord == null)
        {
            return;
        }

        _lblContent.Visible = true;
        _quizPanel.Visible = false;
        _writeBox.Visible = false;
        _workPanel.Visible = false;
        _lblStatus.Visible = true;
        _lblContent.Text = _currentWord.Word;
        _lblStatus.Text = $"{_currentWord.Definition}  ·  tự chạy 10s";
        SpeakCurrent(fromUser: false);
    }

    private void ShowWorkPrompt()
    {
        if (_currentWord == null)
        {
            return;
        }

        _lblContent.Visible = false;
        _quizPanel.Visible = false;
        _writeBox.Visible = false;
        _lblStatus.Visible = false;
        _workPanel.Visible = true;
        StartMarquee($"{_currentWord.Word}    {_currentWord.Definition}");
        MaybeAutoSpeak();
    }

    private void ShowWritePrompt()
    {
        if (_currentWord == null)
        {
            return;
        }

        _lblContent.Visible = true;
        _quizPanel.Visible = false;
        _workPanel.Visible = false;
        _writeBox.Visible = true;
        _lblStatus.Visible = true;
        _lblContent.Text = _isShowingWord ? _currentWord.Word : _currentWord.Definition;
        _lblStatus.Text = _isShowingWord
            ? "Gõ một từ nghĩa Việt · Enter"
            : "Gõ một chữ Hán · Enter";
        _writeBox.Clear();
        _writeBox.Enabled = true;
        FocusWriteBox();
        _timer.Stop();
        MaybeAutoSpeak();
    }

    private void FocusWriteBox()
    {
        if (!_writeBox.Visible || _inFeedback || !IsHandleCreated)
        {
            return;
        }

        BeginInvoke(() =>
        {
            if (_writeBox.Visible && !_inFeedback)
            {
                _writeBox.Focus();
            }
        });
    }

    private void MaybeAutoSpeak()
    {
        if (_settings.AutoSpeakHanzi)
        {
            SpeakCurrent(fromUser: false);
        }
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
        _writeBox.Visible = false;
        _workPanel.Visible = false;
        _quizPanel.Visible = true;
        _lblStatus.Visible = true;
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

        _lblStatus.Text = "Chọn đáp án · Esc hủy · S nghe";
        _lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
    }

    private void OnChoiceClicked(int index)
    {
        if (!_inQuiz || _question == null || index < 0 || index >= _question.Options.Count)
        {
            return;
        }

        string choice = _question.Options[index];
        Grade(choice == _question.CorrectAnswer, choice, _question.CorrectAnswer);
    }

    private string WriteExpected()
    {
        if (_currentWord == null)
        {
            return string.Empty;
        }

        return _isShowingWord ? _currentWord.Definition : _currentWord.Word;
    }

    private void SubmitWrite()
    {
        if (_settings.GetMode() != WidgetMode.Write || _inFeedback || _currentWord == null)
        {
            return;
        }

        string typed = _writeBox.Text;
        string expected = WriteExpected();
        bool ok = AnswerChecker.MatchesAnyToken(typed, expected);
        Grade(ok, string.IsNullOrWhiteSpace(typed) ? "(trống)" : typed.Trim(), expected);
    }

    private void CancelInteractiveUi()
    {
        _inQuiz = false;
        _inFeedback = false;
        _question = null;
        _feedbackTimer.Stop();
        _writeBox.Enabled = true;
        _borderColor = IdleBorder;
        Invalidate();
        ApplyModeChrome();
        _quizPanel.Visible = false;
        if (_settings.GetMode() != WidgetMode.Write)
        {
            _writeBox.Visible = false;
        }

        if (_settings.GetMode() != WidgetMode.Work)
        {
            _lblContent.Visible = true;
        }
    }

    private void Grade(bool isCorrect, string chosen, string correctAnswer)
    {
        if (_currentWord == null)
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
        ApplyModeChrome();
        _quizPanel.Visible = false;
        _workPanel.Visible = false;
        _writeBox.Visible = false;
        _lblStatus.Visible = true;
        _lblContent.Visible = true;

        if (isCorrect)
        {
            _borderColor = Color.FromArgb(46, 204, 113);
            _lblContent.ForeColor = Color.FromArgb(46, 204, 113);
            _lblContent.Text = "Chính xác!";
            _lblStatus.Text = correctAnswer;
            _lblStatus.ForeColor = Color.FromArgb(144, 238, 144);
            _feedbackTimer.Interval = 1500;
            _feedbackTimer.Tag = true;
            if (_settings.FeedbackSounds)
            {
                FeedbackSounds.PlayCorrect();
            }
        }
        else
        {
            _borderColor = Color.FromArgb(231, 76, 60);
            _lblContent.ForeColor = Color.FromArgb(255, 160, 150);
            _lblContent.Text = $"Sai rồi! Đáp án: {correctAnswer}";
            _lblStatus.Text = $"Bạn chọn: {chosen}";
            _lblStatus.ForeColor = Color.Salmon;
            _feedbackTimer.Interval = 2000;
            _feedbackTimer.Tag = false;
            if (_settings.FeedbackSounds)
            {
                FeedbackSounds.PlayWrong();
            }
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

    private void SpeakCurrent(bool fromUser)
    {
        if (_currentWord == null)
        {
            return;
        }

        _speech.SpeakHanzi(_currentWord.Word);
        if (fromUser && !_speech.HasChineseVoice)
        {
            if (_settings.GetMode() != WidgetMode.Work)
            {
                _lblStatus.Visible = true;
                _lblStatus.Text = "Cài giọng Trung trong Windows (Speech)";
                _lblStatus.ForeColor = Color.Khaki;
            }
        }
    }

    private void ApplyModeChrome()
    {
        switch (_settings.GetMode())
        {
            case WidgetMode.Work:
                ApplyWorkLayout();
                break;
            case WidgetMode.Write:
                ApplyWriteLayout();
                break;
            default:
                ApplyIdleLayout();
                break;
        }
    }

    private void ApplyIdleLayout()
    {
        ApplyFloatingSize(IdleSize);
        _writeBox.Visible = false;
        _lblStatus.Visible = true;
        _lblStatus.Height = 22;
    }

    private void ApplyQuizLayout()
    {
        ApplyFloatingSize(QuizSize);
        _writeBox.Visible = false;
        _lblStatus.Visible = true;
    }

    private void ApplyWriteLayout()
    {
        ApplyFloatingSize(WriteSize);
        _lblStatus.Visible = true;
        if (!_inFeedback)
        {
            _writeBox.Visible = true;
        }
    }

    private void ApplyFloatingSize(Size next)
    {
        StopMarquee();
        _workPanel.Visible = false;
        if (_workChrome)
        {
            Size = IdleSize;
            Location = _savedIdleLocation ? _idleLocation : FallbackIdleLocation();
            _workChrome = false;
        }

        AnchorBottomRight(next);
        PositionSpeakButton();
    }

    private void ApplyWorkLayout()
    {
        if (!_workChrome)
        {
            _idleLocation = Location;
            _savedIdleLocation = true;
            _workChrome = true;
        }

        StopMarquee();
        _writeBox.Visible = false;
        _quizPanel.Visible = false;
        _lblStatus.Visible = false;
        Rectangle bounds = (Screen.FromPoint(_idleLocation) ?? Screen.PrimaryScreen)?.Bounds
                           ?? new Rectangle(0, 0, 1280, 64);
        Size = new Size(bounds.Width, WorkBarHeight);
        Location = new Point(bounds.X, bounds.Y);
        _workPanel.Visible = true;
        _workPanel.BringToFront();
        _btnSpeak.BringToFront();
        PositionSpeakButton();
    }

    private void StartMarquee(string text)
    {
        _lblMarquee.Font = UiFont(26, FontStyle.Bold);
        _lblMarquee.Text = text;
        Size preferred = _lblMarquee.PreferredSize;
        _lblMarquee.Height = preferred.Height;
        _lblMarquee.Top = Math.Max(0, (_workPanel.ClientSize.Height - preferred.Height) / 2);
        _marqueeX = -Math.Max(preferred.Width, 1);
        _lblMarquee.Left = (int)_marqueeX;
        _marqueeTimer.Start();
    }

    private void StopMarquee()
    {
        _marqueeTimer.Stop();
    }

    private void OnMarqueeTick(object? sender, EventArgs e)
    {
        if (!_workPanel.Visible)
        {
            return;
        }

        _marqueeX += 3;
        if (_marqueeX > _workPanel.ClientSize.Width)
        {
            _marqueeX = -Math.Max(_lblMarquee.Width, 1);
        }

        _lblMarquee.Left = (int)_marqueeX;
    }

    private void PositionSpeakButton()
    {
        if (_workChrome)
        {
            _btnSpeak.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnSpeak.Location = new Point(Width - _btnSpeak.Width - 10, Math.Max(4, (Height - _btnSpeak.Height) / 2));
        }
        else
        {
            _btnSpeak.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnSpeak.Location = new Point(Width - _btnSpeak.Width - 8, 6);
        }
    }

    private Point FallbackIdleLocation()
    {
        Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        return new Point(workingArea.Right - IdleSize.Width - 20, workingArea.Bottom - IdleSize.Height - 40);
    }

    private void AnchorBottomRight(Size next)
    {
        int right = Location.X + Width;
        int bottom = Location.Y + Height;
        Size = next;
        Location = new Point(right - Width, bottom - Height);
    }

    private int RotationIntervalMs()
    {
        return _settings.GetMode() switch
        {
            WidgetMode.Listen => AppSettings.ListenAdvanceMs,
            WidgetMode.Work => AppSettings.WorkAdvanceMs,
            WidgetMode.Write => _settings.TimerMilliseconds,
            _ => _settings.TimerMilliseconds
        };
    }

    private void RestartRotationTimer()
    {
        _timer.Stop();
        if (_settings.GetMode() == WidgetMode.Write && !_inFeedback)
        {
            return;
        }

        _timer.Interval = Math.Max(500, RotationIntervalMs());
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
