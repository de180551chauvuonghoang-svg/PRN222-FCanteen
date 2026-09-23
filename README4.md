# FCanteen — Lab 04: ASP.NET Core MVC & Web Administration
> **Môn học:** PRN222 — Lập trình .NET nâng cao  
> **Trường:** FPT University Đà Nẵng  
> **Case Study:** Hệ thống quản lý căng tin FCanteen (Xây dựng Cổng thông tin Quản trị Web MVC)  
> **Tài liệu:** Hướng dẫn chi tiết thực hiện, Checklist nghiệm thu và Trả lời câu hỏi lý thuyết

---

## 📋 1. Mục lục & Thang điểm

| Yêu cầu | Nội dung chính | Trọng tâm kỹ thuật | Điểm |
| :--- | :--- | :--- | :---: |
| **YC1** | Mở rộng Schema & Khởi tạo ASP.NET Core MVC | Thêm `Category`, `Supplier`, `MenuItemIngredient`, `PurchaseOrder`. Tạo project `FCanteen.Web` tích hợp DI | 1.5đ |
| **YC2** | CRUD thực đơn & Validation hai phía (Client/Server) | `MenuItemsController` đủ 5 actions, Data Annotations, Regex mã món, Custom Validation `[MinMargin]`, Server Validation | 2.5đ |
| **YC3** | Tìm kiếm, Lọc, Sắp xếp & 4 cách truyền dữ liệu | Search/Filter/Sort giữ trạng thái; áp dụng đủ: `ViewData`, `ViewBag`, `Model strongly typed`, `TempData` (PRG pattern) | 2.0đ |
| **YC4** | Giỏ đặt hàng nguyên liệu bằng Session | Quản lý giỏ hàng qua `ISession`, badge số lượng trên Navbar, tạo đơn đặt hàng `PurchaseOrder` lưu CSDL | 2.0đ |
| **YC5** | Gán nguyên liệu, tính giá vốn & JSON Endpoint | Quan hệ N-N định lượng, tự tính Cost Price; Endpoint JSON `GetAvailableMenuItems` phục vụ PosClient qua `HttpClient` | 1.0đ |
| **Lý thuyết** | Trả lời 2 câu hỏi lý thuyết | Phân biệt TempData vs Session; Vì sao Client Validation không thể thay thế Server Validation | 1.0đ |
| **TỔNG** | **Toàn bộ Lab 04** | **ASP.NET Core MVC Hoàn chỉnh** | **10.0đ** |

---

## 📚 2. Trả lời câu hỏi lý thuyết (Đúng trọng tâm, ngắn gọn)

### ❓ Câu 1: Phân biệt TempData và Session về nơi lưu trữ và vòng đời. Nêu một tình huống trong FCanteen chỉ dùng được TempData.

#### 1. Bảng so sánh TempData vs Session:
| Tiêu chí | Session | TempData |
| :--- | :--- | :--- |
| **Nơi lưu trữ** | Mặc định lưu trữ trên **Server Memory** (hoặc Redis/Distributed Cache), Client chỉ giữ Cookie `SessionId`. | Mặc định lưu trữ trong **Encrypted Cookie** (`CookieTempDataProvider`) gửi về client, hoặc dùng Session provider. |
| **Vòng đời** | Tồn tại **lâu dài** xuyên suốt phiên làm việc của người dùng (mặc định 20 phút kể từ lần request cuối). | Tồn tại **ngắn hạn** — chỉ sống từ Request hiện tại sang **đúng một Request tiếp theo** (khi đã đọc là tự động đánh dấu xóa). |
| **Mục đích** | Lưu dữ liệu trạng thái lâu dài (Giỏ hàng nguyên liệu, thông tin đăng nhập). | Truyền thông điệp trạng thái tạm thời giữa hai action trong mô hình **PRG (Post/Redirect/Get)**. |

#### 2. Tình huống trong FCanteen chỉ dùng được TempData:
* **Tình huống:** Sau khi người quản lý bấm **"Tạo món ăn mới"** (phương thức `POST /MenuItems/Create`), controller thực hiện lưu CSDL rồi chuyển hướng `RedirectToAction(nameof(Index))`.
* **Vì sao chỉ dùng TempData:** Ta cần hiển thị thông báo alert: *"Thêm món Cơm gà thành công!"* trên trang `Index`. 
  * Nếu dùng `ViewBag`/`ViewData`, dữ liệu sẽ **bị mất ngay lập tức** do lệnh chuyển hướng Redirect tạo ra một HTTP Request hoàn toàn mới.
  * Nếu dùng `Session`, thông báo sẽ **tồn tại mãi mãi** và hiện lại liên tục mỗi khi người dùng F5 hoặc bấm sang trang khác trừ khi phải viết code tự xóa thủ công.
  * `TempData` là giải pháp hoàn hảo duy nhất vì nó tự động truyền qua Redirect và tự hủy ngay sau khi trang `Index` hiển thị thông báo một lần.

---

### ❓ Câu 2: Vì sao validation phía client không thay thế được validation phía server?

1. **Client Validation dễ dàng bị vô hiệu hóa hoặc vượt mặt (Bypass):**
   * Người dùng chỉ cần tắt JavaScript trên trình duyệt (Disable JavaScript) là toàn bộ jQuery Validation bị vô hiệu hoàn toàn.
   * Kẻ xấu có thể dùng các công cụ kiểm thử API (như Postman, cURL, Burp Suite, Fiddler) để gửi thẳng HTTP POST request chứa dữ liệu sai lệch (ví dụ giá âm, mã món sai định dạng) lên server mà không hề thông qua giao diện web.
2. **Client Validation không thể kiểm tra các ràng buộc nghiệp vụ phụ thuộc CSDL:**
   * Các nghiệp vụ như: *"Kiểm tra mã món `MON-0001` đã bị trùng trong Database hay chưa"* bắt buộc phải truy vấn SQL Server ở phía Backend. Trình duyệt client không thể tự kết nối trực tiếp vào CSDL để xác thực.
3. **Nguyên tắc cốt lõi về bảo mật:**
   * **Client Validation** chỉ có vai trò cải thiện **trải nghiệm người dùng (UX)**: cảnh báo lỗi tức thì, không cần tải lại trang.
   * **Server Validation** (`ModelState.IsValid`) là **tuyến phòng thủ bắt buộc cuối cùng** bảo vệ tính toàn vẹn dữ liệu và an toàn của hệ thống.

---

## 🛠️ 3. Chi tiết triển khai mã nguồn từng yêu cầu

### YC1. Schema mở rộng & Project MVC
* **Thêm thực thể mới:**
  * `Category`: `CategoryId`, `Name`, `Description`.
  * `Supplier`: `SupplierId`, `Name`, `Phone`, `Email`, `Address`.
  * `MenuItemIngredient`: Khóa chính phức hợp (`MenuItemId`, `IngredientId`), trường `Quantity` (định lượng nguyên liệu của món).
  * `PurchaseOrder` & `PurchaseOrderLine`: Quản lý đơn nhập nguyên liệu từ nhà cung cấp.
* **Cập nhật thực thể cũ:**
  * `MenuItem`: Thêm `ItemCode` (Mã món), `CategoryId` (Khóa ngoại nhóm món).
  * `Ingredient`: Thêm `CostPrice` (Giá vốn nguyên liệu), `SupplierId` (Khóa ngoại nhà cung cấp).
* **Tạo project MVC `FCanteen.Web`:** Tham chiếu `FCanteen.Data`, `FCanteen.Repositories`, `FCanteen.Services`. Cấu hình Session và DI trong `Program.cs`.

### YC2. CRUD Thực đơn & Validation hai phía
* **Controller:** `MenuItemsController` với đủ 5 View: `Index`, `Details`, `Create`, `Edit`, `Delete`.
* **Thuộc tính kiểm tra dữ liệu (Model Validation):**
  * `[Required(ErrorMessage = "Tên món không được để trống")]`
  * `[StringLength(100, MinimumLength = 3, ErrorMessage = "Tên món từ 3 đến 100 ký tự")]`
  * `[RegularExpression(@"^MON-\d{4}$", ErrorMessage = "Mã món phải đúng định dạng MON-xxxx (Ví dụ: MON-0001)")]`
  * `[Range(1000, 10000000, ErrorMessage = "Giá bán từ 1.000đ đến 10.000.000đ")]`
  * `[MinMargin(0.20)]`: Custom Validation Attribute kiểm tra giá bán phải cao hơn giá vốn ít nhất 20%.
* **Kiểm tra trùng mã món:** Server-side validation bằng `ModelState.AddModelError("ItemCode", "Mã món này đã tồn tại trong hệ thống!")`.
* **Thực nghiệm tắt JS:** Tắt JavaScript trong DevTools trình duyệt, bấm Submit $\rightarrow$ Server vẫn chặn và hiển thị thông báo lỗi màu đỏ.

### YC3. Tìm kiếm, Lọc, Sắp xếp & 4 cách truyền dữ liệu
* **Trang danh sách món (`Index`):**
  * Ô tìm kiếm: Nhập tên hoặc mã món (`searchString`).
  * Bộ lọc Dropdown: Lọc theo Nhóm món (`categoryId`) và Trạng thái còn bán (`isAvailable`).
  * Sắp xếp bảng: Click cột Tên món hoặc Giá bán để đảo chiều (Tăng dần / Giảm dần).
  * Giữ nguyên trạng thái qua query string trên thanh URL.
* **4 cách truyền dữ liệu trong Code:**
  1. `Model strongly typed`: `@model MenuItemIndexViewModel` truyền danh sách dữ liệu chính xác kiểu.
  2. `ViewData`: `ViewData["Categories"]` truyền danh sách SelectList cho Dropdown nhóm món.
  3. `ViewBag`: `ViewBag.CurrentSort`, `ViewBag.TotalItems` truyền các biến hiển thị động.
  4. `TempData`: `TempData["SuccessMessage"]` truyền thông báo thành công sau khi lưu (PRG pattern).

### YC4. Giỏ đặt hàng nguyên liệu bằng Session
* **Session Manager:** Sử dụng `ISession` với helper `GetJson<T>` và `SetJson<T>`.
* **Giỏ hàng (`IngredientCart`):**
  * Xem danh sách nguyên liệu của các nhà cung cấp kèm tồn kho và giá vốn.
  * Bấm "Thêm vào giỏ", cập nhật số lượng nhập, xóa dòng.
  * Badge số lượng hiển thị trên thanh Menu điều hướng (`_Layout.cshtml`) ở mọi trang web.
  * Bấm **"Xác nhận đặt hàng"**: Tạo phiếu `PurchaseOrder`, sinh các dòng `PurchaseOrderLine`, lưu xuống CSDL và xóa sạch Session.

### YC5. Gán nguyên liệu & Endpoint JSON cho POS
* **Gán nguyên liệu (N-N):** Trang `AssignIngredients` cho phép chọn các nguyên liệu và điền định lượng (kg/lít/gam) cấu thành món ăn.
* **Tự động tính giá vốn:** $\text{Giá vốn món} = \sum (\text{Định lượng} \times \text{Giá vốn nguyên liệu})$.
* **Endpoint API JSON:** Action `[HttpGet] /MenuItems/GetAvailableMenuItems` trả về `JsonResult` danh sách các món `IsAvailable = true`.
* **Cập nhật PosClient:** Ứng dụng POS kết nối qua `HttpClient` gọi endpoint này lấy thực đơn mới nhất thay vì query trực tiếp database.

---

## 📋 4. Checklist nghiệm thu bài Lab 04

- [ ] **YC1: Dựng project & Database**
  - [ ] Project `FCanteen.Web` khởi chạy được trên trình duyệt (`https://localhost:...`).
  - [ ] Database có đầy đủ bảng mới: `Categories`, `Suppliers`, `MenuItemIngredients`, `PurchaseOrders`.
- [ ] **YC2: CRUD & Validation**
  - [ ] Thực hiện đủ Create, Read, Update, Delete món ăn.
  - [ ] Nhập sai mã món (ví dụ `COMGA`) $\rightarrow$ Báo lỗi Regex `MON-xxxx`.
  - [ ] Nhập giá bán thấp hơn giá vốn + 20% $\rightarrow$ Báo lỗi Custom Validation `[MinMargin]`.
  - [ ] Nhập trùng mã món $\rightarrow$ Server báo lỗi trùng mã qua `ModelState`.
  - [ ] Tắt JavaScript trên trình duyệt (F12 $\rightarrow$ Settings $\rightarrow$ Disable JavaScript), submit form $\rightarrow$ Server vẫn bắt lỗi chuẩn xác.
- [ ] **YC3: Search, Filter, Sort & 4 cách truyền dữ liệu**
  - [ ] Tìm kiếm theo tên/mã món hoạt động đúng.
  - [ ] Lọc theo Nhóm món và Còn bán hoạt động đúng.
  - [ ] Sắp xếp Tên/Giá theo 2 chiều hoạt động đúng.
  - [ ] Chỉ ra được trong code 4 chỗ sử dụng: `ViewData`, `ViewBag`, `Model strongly typed`, `TempData`.
- [ ] **YC4: Giỏ hàng Session**
  - [ ] Thêm nguyên liệu vào giỏ $\rightarrow$ Badge trên thanh Navbar tăng số lượng.
  - [ ] Chuyển qua các trang web khác nhau $\rightarrow$ Badge và giỏ hàng vẫn giữ nguyên.
  - [ ] Vào giỏ sửa số lượng, xóa dòng.
  - [ ] Bấm xác nhận đặt hàng $\rightarrow$ Lưu phiếu vào DB và làm rỗng giỏ hàng.
- [ ] **YC5: Định lượng & Endpoint JSON**
  - [ ] Vào trang gán nguyên liệu cho món, lưu định lượng $\rightarrow$ Giá vốn tự động cập nhật.
  - [ ] Mở URL `/MenuItems/GetAvailableMenuItems` trên trình duyệt $\rightarrow$ Trả về JSON danh sách món.
- [ ] **Lý thuyết:** Trả lời trôi chảy 2 câu hỏi lý thuyết theo bảng tóm tắt mục 2.
