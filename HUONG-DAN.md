# Hướng dẫn cài đặt và sử dụng — Vocabulary Floating Widget

Widget nổi trên Windows để ôn **từ HSK tiếng Trung → nghĩa tiếng Việt**. Có bốn chế độ: **trắc nghiệm 4 đáp án**, **nghe** (tự phát âm), **làm việc** (thanh chữ chạy trên đỉnh màn hình), **viết** (gõ một từ). Khoảng 496 từ đi kèm sẵn.

Ứng dụng **không cần cài đặt**, **không cần quyền Administrator**, **không cần cài .NET** trên máy dùng.

---

## 1. Chạy trên Windows (cách dùng chính)

Cần Windows 10/11 bản 64-bit.

### 1.1. Xuất file chạy (một lần, trên máy có .NET SDK)

Mở Terminal / PowerShell tại thư mục repo rồi chạy:

```bash
dotnet publish VocabularyWidget/VocabularyWidget.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

File ra đây:

`VocabularyWidget\bin\Release\net8.0-windows\win-x64\publish\VocabularyWidget.exe`

Cùng thư mục đó có `words.json` (danh sách từ). Copy **cả thư mục publish** (ít nhất là `.exe` và `words.json`) sang USB, Desktop, v.v.

### 1.2. Chạy hàng ngày

1. Double-click `VocabularyWidget.exe`.
2. Widget nhỏ (~260×110) hiện góc dưới bên phải, luôn nổi trên cửa sổ khác, **không hiện trên thanh Taskbar** (có icon khay hệ thống).
3. Lần đầu dùng, dữ liệu lấy từ `words.json` **nằm cạnh file .exe**.

Khi bạn ôn bài, số lần ôn / số lần đúng được ghi lại vào `words.json`. Thời gian đổi thẻ, chế độ đang chọn, và tùy chọn âm thanh lưu trong `settings.json`, cũng **cạnh file .exe**.

Giữ `words.json` đi cùng `.exe`. Nếu chỉ copy mỗi file exe sang máy khác, danh sách từ và tiến độ ôn sẽ mất.

Nếu thư mục chứa exe không ghi được (ví dụ ổ USB khóa ghi), app ghi dữ liệu vào `%AppData%\VocabularyWidget\`.

Thoát: chuột phải widget → **Thoát ứng dụng**, hoặc chuột phải icon khay hệ thống.

---

## 2. Dùng hàng ngày

### Kéo và click — khác nhau

| Thao tác | Kết quả |
|---|---|
| Nhấn giữ chuột trái rồi **kéo** | Di chuyển widget. Không mở quiz. |
| **Click** (không kéo) vào chữ | Mở trắc nghiệm 4 đáp án (chỉ **chế độ Trắc nghiệm**). |

### Thẻ đang hiện

Mỗi chu kỳ (mặc định **5 phút**) widget chọn một từ và ngẫu nhiên hiện:

- **chữ Hán** → click xong bạn chọn **nghĩa tiếng Việt**, hoặc
- **nghĩa tiếng Việt** → click xong bạn chọn **chữ Hán**.

Dòng chữ nhỏ phía dưới (`Click → chọn nghĩa` / `Click → chọn từ Hán`) cho biết lần click tới sẽ hỏi gì.

### Làm trắc nghiệm

1. Click vào thẻ. Widget **phóng to** một chút, hiện đề và **4 nút** đánh số 1–4.
2. Chọn đáp án:
   - click một nút, hoặc
   - phím **1 / 2 / 3 / 4** (cũng dùng **A / B / C / D**).
3. **Esc** = hủy, quay về thẻ đang hiện, timer chạy tiếp.
4. **S** hoặc nút **听** = nghe phát âm chữ Hán (kể cả khi đề đang hiện nghĩa tiếng Việt).

Kết quả:

- **Đúng** — viền xanh, “Chính xác!”, sang từ khác sau khoảng 1,5 giây.
- **Sai** — viền đỏ, hiện đáp án đúng, **không bỏ thẻ**; sau khoảng 2 giây thẻ cũ hiện lại để bạn ôn tiếp.

Ba đáp án nhiễu lấy từ các từ khác trong danh sách. Những từ bạn hay sai sẽ được hiện thường hơn.

### Bốn chế độ (chuột phải → Chế độ)

Mặc định là **Trắc nghiệm** như trên. Đổi chế độ bất cứ lúc nào; lựa chọn được lưu trong `settings.json`.

#### Chế độ nghe

Widget vẫn nhỏ (~260×110). **Tự chạy**: khoảng **10 giây** đổi một từ ngẫu nhiên và **tự đọc chữ Hán** (SAPI, không cần bấm 听). Phù hợp nghe rảnh tay. Click vào thẻ **không** mở quiz. Kéo để dời vị trí vẫn được. 听 / S vẫn đọc lại nếu muốn.

#### Chế độ làm việc

Một **thanh ngang trên đỉnh màn hình** (full chiều rộng, cao khoảng 64px). Chữ Hán và nghĩa tiếng Việt **chạy từ trái sang phải**. Cỡ chữ khoảng **26pt** — to rõ hơn nhiều so với widget 260×110 (12pt). Khoảng **20 giây** đổi thẻ. Thanh này **ghim trên cùng**, không kéo dời. 听 / S vẫn nghe được.

#### Chế độ viết

Hiện đề (chữ Hán hoặc nghĩa Việt) và ô gõ. **Không cần gõ đúng cả cụm** — trùng **một từ** trong đáp án là đủ (ví dụ nghĩa `Yêu; thương; yêu quý` thì gõ `thương` được tính đúng). Enter gửi; Esc xóa ô. Đúng/sai giống quiz (viền xanh/đỏ). Timer tạm dừng khi đang chờ bạn gõ.

### Nghe phát âm

Nút **听** góc trên phải (hoặc phím **S**) đọc **chữ Hán** bằng giọng có sẵn trên Windows — **không cần mạng**, không tài khoản, không file âm thanh kèm theo.

Mặc định **không tự đọc** khi đổi thẻ (tránh ồn). Chuột phải → **Tự phát âm khi hiện thẻ** nếu muốn widget tự đọc mỗi lần hiện từ.

**Âm thanh đúng/sai** (tiếng beep rất ngắn, nhỏ) cũng tắt mặc định. Bật trong cùng menu nếu thích.

Windows cần **giọng tiếng Trung** (Cài đặt → Thời gian và ngôn ngữ → Lời nói / Speech). Máy cài tiếng Trung thường đã có. Nếu không có giọng ZH, nút 听 vẫn thử đọc nhưng có thể không rõ.

### Chuột phải (menu)

- **Quản lý từ vựng** — cửa sổ thêm / sửa / xóa / import.
- **Đổi từ khác ngay lập tức** — bỏ qua thẻ hiện tại.
- **Chế độ** — Trắc nghiệm / Nghe / Làm việc / Viết.
- **Chỉnh thời gian** — 1, 3, 5 hoặc 10 phút một lần đổi thẻ (áp dụng chế độ trắc nghiệm).
- **Nghe phát âm** — đọc chữ Hán của thẻ đang hiện.
- **Tự phát âm khi hiện thẻ** — bật/tắt (mặc định tắt; chế độ nghe thì luôn tự đọc).
- **Âm thanh đúng/sai** — bật/tắt beep (mặc định tắt).
- **Thoát ứng dụng**.

Cùng menu đó cũng có khi chuột phải icon khay hệ thống. Double-click icon để hiện lại widget nếu đang bị khuất.

### Quản lý từ và import

Trong **Quản lý từ vựng**:

- Cột: từ Hán, nghĩa Việt, số lần ôn, số lần đúng.
- **Thêm**: nhập từ + nghĩa rồi bấm Thêm.
- Sửa ngay trên bảng.
- **Xóa**: chọn một dòng rồi Xóa.
- **Import CSV/TXT**:
  - CSV: `word,definition` (có thể có dòng tiêu đề). Nghĩa có dấu phẩy thì bọc trong ngoặc kép.
  - TXT: `từ | nghĩa`, hoặc `từ - nghĩa`, hoặc hai cột cách bằng Tab (đúng kiểu file HSK).

Ví dụ CSV:

```csv
word,definition
爱,"Yêu; thương; yêu quý"
电脑,Máy tính
```

---

## 3. Xem trước HTML (không phải Windows)

`preview/index.html` chỉ là **bản demo hành vi** (cùng 4 chế độ, cùng danh sách HSK) khi bạn không có Windows. **Đây không phải app cài cho máy học**.

Mở file bằng trình duyệt, hoặc từ thư mục repo:

```bash
python3 -m http.server 8765 --directory preview
```

Rồi vào `http://localhost:8765/`.

Thao tác giống widget Windows: kéo để dời, **听** / phím S để nghe chữ Hán, click để trắc nghiệm, chuột phải → **Chế độ** để chuyển Nghe / Làm việc / Viết. Dữ liệu preview lưu trong trình duyệt (localStorage), **không** ghi vào `words.json` của file exe.

Preview dùng **Web Speech API** của trình duyệt (`zh-CN`), không dùng SAPI của Windows. Chrome thường có giọng Trung; Firefox có thể không. Trình duyệt đôi khi chặn tự phát âm cho đến khi bạn click 听 một lần.

---

## 4. Gặp sự cố nhanh

| Hiện tượng | Việc nên làm |
|---|---|
| Không thấy widget | Xem khay hệ thống (góc phải Taskbar); double-click icon. |
| Click bị thành kéo | Thả chuột ngay, đừng dịch chuột khi bấm. |
| Mất hết từ / về danh sách trống | Kiểm tra `words.json` còn cạnh `.exe` không; copy lại từ thư mục `publish`. |
| Muốn làm lại từ đầu | Đóng app, xóa `words.json` và `settings.json` cạnh exe, copy lại `words.json` gốc rồi mở app. |
| Không nghe được tiếng Trung | Cài giọng Chinese (Simplified) trong Windows Speech. Preview: dùng Chrome và click **听**. |
| Máy không phải Windows | Dùng bản preview HTML ở mục 3. |

Nguồn danh sách gốc trong repo: `data/hsk-vocabulary.md` (xuất ra `words.json` khi đóng gói).
