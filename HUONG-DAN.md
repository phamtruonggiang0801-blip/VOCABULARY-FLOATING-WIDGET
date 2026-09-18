# Hướng dẫn cài đặt và sử dụng — Vocabulary Floating Widget

Widget nổi trên Windows để ôn **từ HSK tiếng Trung → nghĩa tiếng Việt**. Bấm vào thẻ là **trắc nghiệm 4 đáp án** (không gõ chữ). Khoảng 496 từ đi kèm sẵn.

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

Khi bạn ôn bài, số lần ôn / số lần đúng được ghi lại vào `words.json`. Thời gian đổi thẻ (1 / 3 / 5 / 10 phút) lưu trong `settings.json`, cũng **cạnh file .exe**.

Giữ `words.json` đi cùng `.exe`. Nếu chỉ copy mỗi file exe sang máy khác, danh sách từ và tiến độ ôn sẽ mất.

Nếu thư mục chứa exe không ghi được (ví dụ ổ USB khóa ghi), app ghi dữ liệu vào `%AppData%\VocabularyWidget\`.

Thoát: chuột phải widget → **Thoát ứng dụng**, hoặc chuột phải icon khay hệ thống.

---

## 2. Dùng hàng ngày

### Kéo và click — khác nhau

| Thao tác | Kết quả |
|---|---|
| Nhấn giữ chuột trái rồi **kéo** | Di chuyển widget. Không mở quiz. |
| **Click** (không kéo) vào chữ | Mở trắc nghiệm 4 đáp án. |

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

Kết quả:

- **Đúng** — viền xanh, “Chính xác!”, sang từ khác sau khoảng 1,5 giây.
- **Sai** — viền đỏ, hiện đáp án đúng, **không bỏ thẻ**; sau khoảng 2 giây thẻ cũ hiện lại để bạn ôn tiếp.

Ba đáp án nhiễu lấy từ các từ khác trong danh sách. Những từ bạn hay sai sẽ được hiện thường hơn.

### Chuột phải (menu)

- **Quản lý từ vựng** — cửa sổ thêm / sửa / xóa / import.
- **Đổi từ khác ngay lập tức** — bỏ qua thẻ hiện tại.
- **Chỉnh thời gian** — 1, 3, 5 hoặc 10 phút một lần đổi thẻ.
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

`preview/index.html` chỉ là **bản demo hành vi** (cùng kiểu trắc nghiệm 4 đáp án, cùng danh sách HSK) khi bạn không có Windows. **Đây không phải app cài cho máy học**.

Mở file bằng trình duyệt, hoặc từ thư mục repo:

```bash
python3 -m http.server 8765 --directory preview
```

Rồi vào `http://localhost:8765/`.

Thao tác giống widget Windows: kéo để dời, click để trắc nghiệm, chuột phải để mở menu. Dữ liệu preview lưu trong trình duyệt (localStorage), **không** ghi vào `words.json` của file exe.

---

## 4. Gặp sự cố nhanh

| Hiện tượng | Việc nên làm |
|---|---|
| Không thấy widget | Xem khay hệ thống (góc phải Taskbar); double-click icon. |
| Click bị thành kéo | Thả chuột ngay, đừng dịch chuột khi bấm. |
| Mất hết từ / về danh sách trống | Kiểm tra `words.json` còn cạnh `.exe` không; copy lại từ thư mục `publish`. |
| Muốn làm lại từ đầu | Đóng app, xóa `words.json` và `settings.json` cạnh exe, copy lại `words.json` gốc rồi mở app. |
| Máy không phải Windows | Dùng bản preview HTML ở mục 3. |

Nguồn danh sách gốc trong repo: `data/hsk-vocabulary.md` (xuất ra `words.json` khi đóng gói).
