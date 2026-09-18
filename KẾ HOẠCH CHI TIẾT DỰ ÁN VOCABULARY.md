KẾ HOẠCH CHI TIẾT DỰ ÁN: VOCABULARY FLOATING WIDGET (C# .NET)
1. Mục tiêu và Tiêu chí Kỹ thuật
Loại ứng dụng: Windows Desktop Application (Windows Forms / WinForms).
Mục tiêu vận hành:
Chạy dạng Portable (1 file .exe duy nhất), không cần bộ cài đặt (installer).
Không đòi hỏi quyền Administrator (ghi dữ liệu trực tiếp cùng thư mục hoặc thư mục %AppData%).
Tiêu tốn ít tài nguyên: RAM < 25 MB, CPU ~ 0%.
Widget nhỏ gọn, luôn hiển thị trên màn hình (TopMost), có thể kéo thả tự do.
Tự động đổi từ vựng sau mỗi chu kỳ (mặc định 5 phút).
Click vào từ để kiểm tra kiến thức (nhập nghĩa hoặc nhập từ tương ứng).
2. Kiến trúc Ứng dụng & Dữ liệu
2.1. Cấu trúc Thư mục và File
VocabularyWidget/
│
├── VocabularyWidget.sln
└── VocabularyWidget/
    ├── Program.cs             // Entry point
    ├── Models/
    │   └── WordItem.cs        // Model dữ liệu từ vựng
    ├── Services/
    │   └── DataService.cs     // Đọc/Ghi file JSON
    ├── Forms/
    │   ├── WidgetForm.cs      // Giao diện widget nổi chính
    │   └── ManageForm.cs      // Giao diện thêm/sửa từ vựng
    └── words.json             // File lưu trữ dữ liệu


2.2. Cấu trúc Dữ liệu (words.json)
[
  {
    "Id": "1",
    "Word": "resilient",
    "Definition": "kiên cường, có khả năng phục hồi nhanh",
    "ReviewCount": 0,
    "CorrectCount": 0
  },
  {
    "Id": "2",
    "Word": "ubiquitous",
    "Definition": "có mặt ở khắp mọi nơi, phổ biến",
    "ReviewCount": 0,
    "CorrectCount": 0
  }
]


3. Luồng Hoạt động Chi tiết (State Machine)
3.1. Chế độ Hiển thị Thụ động (Display Mode)
Khởi động ứng dụng  Nạp file words.json.
Widget xuất hiện ở góc dưới bên phải màn hình.
Kích hoạt System.Windows.Forms.Timer với chu kỳ 300 giây (5 phút).
Mỗi chu kỳ, hệ thống:
Chọn ngẫu nhiên 1 mục từ danh sách.
Chọn ngẫu nhiên trạng thái hiển thị:
State A: Hiển thị Word (Yêu cầu trả lời: Definition).
State B: Hiển thị Definition (Yêu cầu trả lời: Word).
3.2. Chế độ Kiểm tra Chủ động (Quiz Mode)
Người dùng bấm chuột trái (Click) vào khung chữ.
Dòng chữ biến mất, thay bằng một ô nhập liệu (TextBox).
Tạm dừng Timer đổi từ trong lúc người dùng đang gõ.
Người dùng gõ đáp án và bấm Enter:
Đúng: Khung nhập viền xanh, hiển thị thông báo "Chính xác!", chuyển sang từ khác sau 1.5 giây.
Sai: Khung viền đỏ, hiển thị đáp án đúng để người dùng ghi nhớ, sau đó quay lại chu kỳ Timer bình thường.
Người dùng bấm Escape (Esc): Hủy bỏ chế độ kiểm tra, quay về hiển thị ban đầu.
4. Thiết kế Giao diện (UI/UX)
Kích thước Widget: Rộng 260px, Cao 110px.
Thuộc tính Form:
FormBorderStyle = FormBorderStyle.None (Không thanh tiêu đề Windows).
TopMost = true (Luôn nổi trên mọi ứng dụng khác).
ShowInTaskbar = false (Thu gọn vào System Tray ở thanh Taskbar).
Tương tác:
Nhấp chuột và giữ chuột trái để kéo thả di chuyển (Drag & Drop).
Chuột phải vào Widget để mở ContextMenu:
Quản lý từ vựng (Mở cửa sổ thêm/xóa).
Đổi từ khác ngay lập tức.
Chỉnh thời gian (1 phút, 3 phút, 5 phút, 10 phút).
Thoát ứng dụng.
5. Mã Nguồn Minh Họa Chi Tiết
5.1. Model Dữ liệu (WordItem.cs)
namespace VocabularyWidget.Models
{
    public class WordItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Word { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public int ReviewCount { get; set; } = 0;
        public int CorrectCount { get; set; } = 0;
    }
}


5.2. Lớp Xử lý Lưu trữ (DataService.cs)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VocabularyWidget.Models;

namespace VocabularyWidget.Services
{
    public class DataService
    {
        private readonly string _filePath;

        public DataService()
        {
            // Lưu file json cùng cấp với file thực thi
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _filePath = Path.Combine(appDir, "words.json");
            EnsureDataFileExists();
        }

        private void EnsureDataFileExists()
        {
            if (!File.Exists(_filePath))
            {
                var sampleList = new List<WordItem>
                {
                    new WordItem { Word = "diligent", Definition = "chăm chỉ, siêng năng" },
                    new WordItem { Word = "innovative", Definition = "đổi mới, sáng tạo" },
                    new WordItem { Word = "perspective", Definition = "góc nhìn, quan điểm" }
                };
                SaveWords(sampleList);
            }
        }

        public List<WordItem> LoadWords()
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<WordItem>>(json) ?? new List<WordItem>();
            }
            catch
            {
                return new List<WordItem>();
            }
        }

        public void SaveWords(List<WordItem> words)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(words, options);
            File.WriteAllText(_filePath, json);
        }
    }
}


5.3. Form Giao diện Widget (WidgetForm.cs)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VocabularyWidget.Models;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms
{
    public partial class WidgetForm : Form
    {
        // Win32 API để cho phép kéo thả Form không viền
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private readonly DataService _dataService;
        private List<WordItem> _wordList;
        private WordItem? _currentWord;
        private bool _isShowingWord; // True: hiện từ, False: hiện định nghĩa
        
        private readonly System.Windows.Forms.Timer _timer;
        private Label _lblContent;
        private TextBox _txtAnswer;
        private Label _lblStatus;

        public WidgetForm()
        {
            _dataService = new DataService();
            _wordList = _dataService.LoadWords();

            InitializeComponentCustom();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 5 * 60 * 1000; // 5 phút (300,000 ms)
            _timer.Tick += (s, e) => DisplayRandomWord();
            _timer.Start();

            DisplayRandomWord();
        }

        private void InitializeComponentCustom()
        {
            // Thiết lập cửa sổ
            this.Size = new Size(280, 120);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(32, 33, 36);
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;

            // Đặt vị trí ban đầu ở góc dưới bên phải màn hình
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 20, workingArea.Bottom - this.Height - 40);

            // Cho phép kéo thả form
            this.MouseDown += OnFormMouseDown;

            // Nhãn hiển thị từ / định nghĩa
            _lblContent = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            _lblContent.MouseDown += OnFormMouseDown;
            _lblContent.Click += (s, e) => StartQuizMode();

            // Ô nhập câu trả lời (ẩn mặc định)
            _txtAnswer = new TextBox
            {
                Visible = false,
                Font = new Font("Segoe UI", 11),
                Width = 240,
                Location = new Point(20, 45)
            };
            _txtAnswer.KeyDown += TxtAnswer_KeyDown;

            // Nhãn báo đúng / sai nhỏ ở góc dưới
            _lblStatus = new Label
            {
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 8),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 20,
                Text = "Click để kiểm tra"
            };

            // Menu ngữ cảnh khi bấm chuột phải
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Từ tiếp theo", null, (s, e) => DisplayRandomWord());
            contextMenu.Items.Add("Đóng Widget", null, (s, e) => Application.Exit());
            this.ContextMenuStrip = contextMenu;

            this.Controls.Add(_txtAnswer);
            this.Controls.Add(_lblContent);
            this.Controls.Add(_lblStatus);
        }

        private void OnFormMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void DisplayRandomWord()
        {
            if (_wordList.Count == 0)
            {
                _lblContent.Text = "Chưa có từ vựng!";
                return;
            }

            var random = new Random();
            _currentWord = _wordList[random.Next(_wordList.Count)];
            _isShowingWord = random.Next(2) == 0;

            _lblContent.Visible = true;
            _txtAnswer.Visible = false;
            _lblContent.Text = _isShowingWord ? _currentWord.Word : _currentWord.Definition;
            _lblStatus.Text = _isShowingWord ? "Nhập định nghĩa..." : "Nhập từ vựng...";
            _lblStatus.ForeColor = Color.DarkGray;
        }

        private void StartQuizMode()
        {
            if (_currentWord == null) return;

            _timer.Stop();
            _lblContent.Visible = false;
            _txtAnswer.Visible = true;
            _txtAnswer.Clear();
            _txtAnswer.Focus();
        }

        private void TxtAnswer_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _lblContent.Visible = true;
                _txtAnswer.Visible = false;
                _timer.Start();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Ngăn tiếng bíp
                CheckAnswer();
            }
        }

        private void CheckAnswer()
        {
            if (_currentWord == null) return;

            string target = _isShowingWord ? _currentWord.Definition : _currentWord.Word;
            string input = _txtAnswer.Text.Trim();

            // So sánh không phân biệt hoa thường
            bool isCorrect = string.Equals(input, target, StringComparison.OrdinalIgnoreCase);

            _txtAnswer.Visible = false;
            _lblContent.Visible = true;

            if (isCorrect)
            {
                _lblContent.Text = "Chính xác! 🎉";
                _lblStatus.Text = target;
                _lblStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                _lblContent.Text = $"Sai rồi! Đáp án: {target}";
                _lblStatus.Text = $"Bạn đã nhập: {input}";
                _lblStatus.ForeColor = Color.Salmon;
            }

            // Đợi 2 giây rồi chuyển sang từ kế tiếp
            var delayTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();
                DisplayRandomWord();
                _timer.Start();
            };
            delayTimer.Start();
        }
    }
}


6. Hướng Dẫn Biên Dịch Sang 1 File Chạy Duy Nhất (Single Portable Executable)
Để chạy được ở máy không có .NET runtime và không cần quyền Admin:
Mở Terminal / PowerShell tại thư mục dự án.
Chạy lệnh xuất bản (Publish):
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true


Giải thích các tham số:
-r win-x64: Biên dịch chuyên biệt cho Windows 64-bit.
--self-contained true: Đóng gói kèm toàn bộ .NET runtime bên trong. Máy người dùng không cần cài thêm bất kỳ file gì vẫn chạy được.
-p:PublishSingleFile=true: Đóng gói tất cả DLL, thư viện, mã nguồn thành duy nhất 1 file .exe.
-p:EnableCompressionInSingleFile=true: Nén dung lượng file thực thi để file nhỏ nhất có thể (~15 - 20 MB).
7. Lộ Trình Phát Triển Mở Rộng Tiếp Theo
Giai đoạn 1 (MVP - Hiện tại): Widget cơ bản, Timer 5 phút, Click nhập đáp án, lưu JSON cục bộ.
Giai đoạn 2 (Quản lý từ vựng): Tạo thêm cửa sổ ManageForm gồm danh sách bảng (DataGrid) để người dùng thêm, xóa, import file .csv hoặc .txt.
Giai đoạn 3 (Thuật toán lặp lại ngắt quãng - Spaced Repetition): Ưu tiên hiển thị những từ mà người dùng trả lời sai nhiều lần thay vì chọn ngẫu nhiên đồng đều.
