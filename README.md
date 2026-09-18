📌 Vocabulary Floating Widget
Ứng dụng widget nổi trên màn hình Windows giúp ghi nhớ từ vựng thụ động và chủ động, cực nhẹ, dạng Portable (1 file .exe duy nhất) và không cần quyền Administrator.
🌟 Điểm nổi bật
🚀 Portable & Zero-Install: Tải về là chạy ngay, không cần cài đặt (No installer), không đòi hỏi quyền Admin (UAC).
🪶 Siêu nhẹ tài nguyên: Chiếm chưa tới 25 MB RAM và ~0% CPU, không làm chậm máy khi làm việc hoặc chơi game.
📌 Always-on-Top: Widget luôn nổi trên cùng (TopMost), có thể kéo thả tự do đến vị trí tiện lợi trên màn hình.
🔄 Học thụ động & Đa chiều: Tự động chuyển đổi từ vựng theo chu kỳ (mặc định 5 phút). Hệ thống sẽ ngẫu nhiên hiển thị từ vựng (để bạn nhớ nghĩa) hoặc hiển thị nghĩa (để bạn nhớ lại từ).
🎯 Kiểm tra chủ động (Interactive Quiz): Nhấp chuột trực tiếp vào từ để mở ô nhập đáp án kiểm tra nhanh trí nhớ.
💾 Dữ liệu cục bộ (Local JSON): Dễ dàng sao lưu, chỉnh sửa danh sách từ vựng bằng file words.json.
🖥️ Yêu cầu hệ thống
Hệ điều hành: Windows 10 / Windows 11 (64-bit).
Môi trường: Đã đóng gói sẵn runtime (Self-Contained), không cần cài thêm .NET Runtime trên máy người dùng.
🚀 Hướng dẫn sử dụng nhanh
Tải file VocabularyWidget.exe từ trang Releases về thư mục bất kỳ (ví dụ: D:\Tools\VocabularyWidget).
Nhấp đúp chuột vào VocabularyWidget.exe để khởi chạy.
Khi khởi động lần đầu, ứng dụng sẽ tự động sinh file words.json chứa các từ vựng mẫu cùng thư mục.
Thao tác điều khiển:
Thao tác
Hành động
Nhấp giữ chuột trái
Kéo thả di chuyển widget trên màn hình.
Click chuột trái vào chữ
Mở ô nhập đáp án (Quiz Mode).
Enter (khi đang gõ)
Kiểm tra đáp án đúng/sai.
Esc (khi đang gõ)
Hủy kiểm tra, quay lại chế độ hiển thị thông thường.
Chuột phải vào Widget
Mở Menu ngữ cảnh: Chuyển từ tiếp theo, Quản lý từ vựng, Thoát ứng dụng.

📝 Quản lý từ vựng (words.json)
Bạn có thể chỉnh sửa trực tiếp file words.json bằng bất kỳ trình soạn thảo nào (Notepad, VS Code,...):
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


🛠️ Hướng dẫn tự biên dịch mã nguồn (Build from Source)
1. Chuẩn bị môi trường
.NET 8.0 SDK trở lên.
Visual Studio 2022 hoặc Visual Studio Code.
2. Clone dự án
git clone https://github.com/your-username/VocabularyWidget.git
cd VocabularyWidget


3. Biên dịch ra 1 file thực thi duy nhất (Single-File Portable Executable)
Chạy lệnh sau trong PowerShell hoặc Terminal:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true


File sau khi xuất bản sẽ nằm tại:
bin/Release/net8.0-windows/win-x64/publish/VocabularyWidget.exe


🗺️ Lộ trình phát triển (Roadmap)
[x] Widget nổi kéo thả, không viền, Always-on-top.
[x] Chế độ tự đổi từ sau 5 phút & Click để nhập đáp án.
[x] Lưu trữ cục bộ bằng file JSON.
[ ] Giao diện quản lý danh sách từ (CRUD) trực quan không cần sửa file JSON thủ công.
[ ] Hỗ trợ nhập/xuất file danh sách .csv hoặc .txt.
[ ] Thuật toán Lặp lại ngắt quãng (Spaced Repetition - Leitner System/SM-2) để ưu tiên từ hay quên.
[ ] Phát âm từ vựng (Text-to-Speech).
📄 Bản quyền (License)
Dự án được phân phối dưới giấy phép MIT License. Thoải mái sử dụng, tùy biến và chia sẻ cho mục đích cá nhân hoặc thương mại.
