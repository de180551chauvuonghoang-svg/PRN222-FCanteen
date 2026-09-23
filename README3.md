# FCanteen — Lab 03: Dependency Injection & Layered Architecture
> **Môn học:** PRN222 — Lập trình .NET nâng cao  
> **Trường:** FPT University Đà Nẵng  
> **Case Study:** Hệ thống quản lý căng tin FCanteen (Tái cấu trúc phân tầng & Áp dụng Dependency Injection)  
> **Tài liệu:** Hướng dẫn chi tiết mã nguồn và trả lời câu hỏi lý thuyết Lab 03

---

## 📋 Mục lục & Thang điểm

| Yêu cầu | Nội dung chính | Trọng tâm kỹ thuật | Điểm | Trạng thái |
| :--- | :--- | :--- | :---: | :---: |
| **YC1** | Tái cấu trúc theo tầng & Đăng ký DI | Tách 4 project (`Data`, `Repositories`, `Services`, `ConsoleApp`), thêm `Staff`, `Host.CreateApplicationBuilder` | 2.0đ | ✅ Hoàn thành |
| **YC2** | Chính sách giảm giá theo Open/Closed (OCP) | `IDiscountPolicy` (Sinh viên, Giảng viên, Combo), ghi log `DiscountPolicyLog`, kịch bản thêm chính sách thứ 4 khi bảo vệ | 2.0đ | ✅ Hoàn thành |
| **YC3** | Đảo ngược phụ thuộc kênh thông báo (DIP) | `INotificationService` với 3 cài đặt: Console, File, Email. Đổi 1 dòng DI để chuyển kênh | 1.5đ | ✅ Hoàn thành |
| **YC4** | Service Lifetimes & Captive Dependency | Đo `InstanceId` (Transient, Scoped, Singleton) qua 2 scope. Bắt lỗi Captive Dependency và sửa | 2.0đ | ✅ Hoàn thành |
| **YC5** | Bốn kiểu Injection trong OrderService | Constructor, Property (AuditLogger), Method (ExportReceipt), Ambient Context (`AsyncLocal<Staff>`) | 1.5đ | ✅ Hoàn thành |
| **Lý thuyết** | Trả lời 2 câu hỏi lý thuyết | Giải thích cơ chế Singleton phụ thuộc Scoped & Nhược điểm của Ambient Context | 1.0đ | ✅ Hoàn thành |
| **TỔNG** | **Toàn bộ Lab 03** | **Kiến trúc sạch (Clean Architecture) & DI Container** | **10.0đ** | **Sẵn sàng nghiệm thu** |

---

## 🏗️ 1. Kiến trúc phân tầng của Solution (YC1)

Mã nguồn được tổ chức thành 4 project độc lập theo đúng nguyên lý phân tách trách nhiệm (Separation of Concerns):

```text
FCanteen (Solution)
 ├── FCanteen.Data           (Class Library)
 │    ├── Entities/          (OrderTicket, TicketLine, MenuItem, Ingredient, Staff, DiscountPolicyLog...)
 │    └── FCanteenContext.cs (EF Core DbContext & Migrations)
 │
 ├── FCanteen.Repositories   (Class Library - Tham chiếu Data)
 │    ├── Interfaces/        (IMenuItemRepository, IOrderRepository, IIngredientRepository, IStaffRepository)
 │    └── Implementations/   (MenuItemRepository, OrderRepository, IngredientRepository, StaffRepository)
 │
 ├── FCanteen.Services       (Class Library - Tham chiếu Repositories & Data)
 │    ├── Interfaces/        (IOrderService, IReportService, IInventoryService, INotificationService, IDiscountPolicy...)
 │    ├── Discounts/         (StudentDiscountPolicy, TeacherStaffDiscountPolicy, ComboDiscountPolicy)
 │    ├── Notifications/     (ConsoleNotificationService, FileNotificationService, EmailNotificationService)
 │    └── Implementations/   (OrderService, ReportService, InventoryService)
 │
 └── FCanteen.ConsoleApp     (Console Application - Tham chiếu Services, Repositories, Data)
      ├── Program.cs         (Cấu hình Host.CreateApplicationBuilder, đăng ký DI container & CLI Menu)
      └── appsettings.json   (Chuỗi kết nối SQL Server)
```

---

## 🛠️ 2. Chi tiết từng yêu cầu & Cách vận hành

### YC1. Tái cấu trúc theo tầng và đăng ký DI (2 điểm)
* **Bổ sung thực thể `Staff`:**
  * `StaffId`: Khóa chính.
  * `StaffCode`: Mã nhân viên (ví dụ: `NV01`, `NV02`).
  * `FullName`: Họ và tên.
  * `Role`: Vai trò (`Cashier`, `Manager`, `Chef`).
  * `BranchCode`: Cơ sở làm việc (`CS1`, `CS2`, `CS3`).
* **Đăng ký DI Container:** Sử dụng `Host.CreateApplicationBuilder(args)` trong `FCanteen.ConsoleApp`.
* **Quy tắc tuyệt đối:** Không còn bất kỳ từ khóa `new` nào để tạo Repository hoặc Service. Tất cả đều được tiêm tự động thông qua Constructor Injection.

---

### YC2. Chính sách giảm giá theo Open/Closed Principle (OCP) (2 điểm)
* **Interface `IDiscountPolicy`:**
  ```csharp
  public interface IDiscountPolicy
  {
      string PolicyName { get; }
      int Priority { get; } // Thứ tự ưu tiên áp dụng (số nhỏ chạy trước)
      decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason);
  }
  ```
* **3 Cài đặt ban đầu:**
  1. `StudentDiscountPolicy`: Giảm 10% tổng hóa đơn cho đối tượng sinh viên.
  2. `TeacherStaffDiscountPolicy`: Giảm 15% tổng hóa đơn cho giảng viên / nhân viên FPT.
  3. `ComboDiscountPolicy`: Khách mua món chính (cơm, bún, phở) kèm nước uống $\rightarrow$ Giảm ngay 5.000đ.
* **Ghi log giảm giá:** Mỗi khi áp dụng chính sách, hệ thống tự động ghi 1 dòng vào bảng `DiscountPolicyLog` (mã phiếu, tên chính sách, số tiền giảm, thời điểm).
* 🎯 **KỊCH BẢN NGHIỆM THU TẠI BUỔI BẢO VỆ (Thầy cô yêu cầu thêm chính sách thứ 4):**
  * *Ví dụ đề bài của thầy:* "Thêm chính sách Happy Hour: Giảm 20% cho các hóa đơn tạo sau 19h tối".
  * *Cách xử lý chuẩn OCP:* 
    1. Tạo 1 file mới `HappyHourDiscountPolicy.cs` kế thừa `IDiscountPolicy`.
    2. Vào `Program.cs` thêm đúng 1 dòng: `builder.Services.AddSingleton<IDiscountPolicy, HappyHourDiscountPolicy>();`.
    3. **Tuyệt đối KHÔNG SỬA** bất kỳ dòng code nào trong `OrderService` hay 3 class giảm giá cũ!

---

### YC3. Đảo ngược phụ thuộc cho kênh thông báo (DIP) (1.5 điểm)
* Định nghĩa `INotificationService`:
  ```csharp
  public interface INotificationService
  {
      Task SendNotificationAsync(string title, string message);
  }
  ```
* Cài đặt 3 kênh thông báo:
  * `ConsoleNotificationService`: In thông báo màu mè trực tiếp ra màn hình Console.
  * `FileNotificationService`: Ghi nối thông báo vào file text `notifications.log`.
  * `EmailNotificationService`: Mô phỏng gửi email thông báo đơn hàng cho khách.
* `OrderService` chỉ phụ thuộc vào interface `INotificationService`.
* Khi muốn đổi kênh thông báo, chỉ cần đổi **đúng 1 dòng** trong `Program.cs`:
  ```csharp
  // Kênh 1: Console
  builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
  // Đổi sang Kênh 2: File
  // builder.Services.AddScoped<INotificationService, FileNotificationService>();
  // Đổi sang Kênh 3: Email
  // builder.Services.AddScoped<INotificationService, EmailNotificationService>();
  ```

---

### YC4. Service Lifetimes & Lỗi Captive Dependency (2 điểm)

#### 1. Kiểm chứng vòng đời Service (Transient vs Scoped vs Singleton):
* Cả 3 service đều có thuộc tính `public Guid InstanceId { get; } = Guid.NewGuid();`.
* Khi chạy Menu kiểm tra vòng đời: Hệ thống tạo 2 `IServiceScope` độc lập, mỗi scope lấy service 2 lần:
  * **Transient:** Mỗi lần `GetService` là 1 `Guid` mới hoàn toàn (4 lần = 4 ID khác nhau).
  * **Scoped:** Trong cùng 1 Scope thì giữ nguyên `Guid`, sang Scope khác thì sinh `Guid` mới (Scope 1 có ID_A, Scope 2 có ID_B).
  * **Singleton:** Toàn bộ ứng dụng từ đầu đến cuối chỉ có đúng 1 `Guid` duy nhất (4 lần = cùng 1 ID).

#### 2. Lỗi Captive Dependency (Bắt lỗi & Sửa lỗi):
* **Cách tạo lỗi:** Đăng ký một service `Singleton` nhưng lại tiêm `FCanteenContext` (vốn là `Scoped`) vào constructor của nó:
  ```csharp
  builder.Services.AddSingleton<CaptiveServiceDemo>(); // Chứa DbContext bên trong
  ```
* **Khi bật kiểm tra scope:** `Host.CreateDefaultBuilder` hoặc `builder.Host.UseDefaultServiceProvider(o => o.ValidateScopes = true)`.
* **Kết quả:** Chương trình văng lỗi ngay khi khởi động:  
  `System.InvalidOperationException: Cannot consume scoped service 'FCanteen.Data.FCanteenContext' from singleton 'CaptiveServiceDemo'.`
* **Cách khắc phục:** Sửa vòng đời của service thành `Scoped` hoặc dùng `IServiceScopeFactory` để tạo scope khi cần.

---

### YC5. Bốn kiểu Injection trong OrderService (1.5 điểm)

Trong class `OrderService`, áp dụng cùng lúc 4 kiểu Dependency Injection:

1. **Constructor Injection (Chính yếu):**
   * Tiêm `IOrderRepository`, `IMenuItemRepository`, `IEnumerable<IDiscountPolicy>`, `INotificationService`.
   * Bắt buộc phải có để đối tượng hoạt động.
2. **Property Injection (Setter Injection):**
   * `public IAuditLogger? AuditLogger { get; set; }`
   * Dùng cho phụ thuộc không bắt buộc (Optional Dependency). Nếu có thì ghi log kiểm toán, nếu `null` thì hệ thống vẫn hoạt động bình thường.
3. **Method Injection:**
   * `public void ExportReceipt(OrderTicket ticket, IReceiptFormatter formatter)`
   * Phụ thuộc `IReceiptFormatter` chỉ cần dùng riêng cho đúng 1 hành động xuất hóa đơn (PDF, TXT, JSON), không cần phải giữ trong state của cả class.
4. **Ambient Context (Dùng `AsyncLocal<Staff>`):**
   * Sử dụng `StaffContext.Current`:
     ```csharp
     public static class StaffContext
     {
         private static readonly AsyncLocal<Staff?> _currentStaff = new();
         public static Staff? Current
         {
             get => _currentStaff.Value;
             set => _currentStaff.Value = value;
         }
     }
     ```
   * Giúp toàn bộ các hàm sâu bên dưới đều biết ai đang đăng nhập thao tác mà không phải truyền tham số `Staff staff` qua hàng loạt hàm trung gian.

---

## 📚 3. Trả lời 2 câu hỏi lý thuyết (Theo kiểu hiểu & Ví dụ đời thực)

---

### ❓ Câu 1: Vì sao .NET ném ngoại lệ khi một Singleton phụ thuộc vào một Scoped, mà không phải ngược lại?

#### 💡 Trả lời theo kiểu hiểu:
* **Khái niệm tuổi thọ:**
  * **Singleton:** Sống suốt đời ứng dụng (từ lúc bật app đến lúc tắt máy).
  * **Scoped:** Chỉ sống trong một phiên làm việc ngắn (ví dụ: trong 1 request web hoặc 1 ca làm việc rồi bị hủy/Dispose).
* **Hiện tượng "Bắt cóc con tin" (Captive Dependency):**
  * Tưởng tượng **Singleton** là một ông lão sống 100 năm, còn **Scoped** là một cốc kem tươi chỉ để được trong 15 phút.
  * Nếu ông lão (Singleton) cầm cốc kem (Scoped) cất vào túi áo của mình: Cốc kem đáng lẽ phải tan chảy và vứt đi sau 15 phút, nhưng vì ông lão sống 100 năm nên cốc kem bị giữ lại mãi mãi trong túi áo ông lão!
  * Trong C#, khi Singleton giữ tham chiếu đến `DbContext` (Scoped): `DbContext` không bao giờ được giải phóng. Nó sẽ giữ nguyên các kết nối CSDL cũ, cache dữ liệu rác ngày càng phình to gây tràn RAM (Memory Leak), và khi nhiều luồng cùng gọi qua Singleton sẽ làm sập `DbContext` vì nó không thread-safe.
* **Tại sao Scoped phụ thuộc Singleton thì lại HOÀN TOÀN HỢP LỆ?**
  * Ngược lại, nếu cốc kem (Scoped - sống 15 phút) đứng cạnh ông lão (Singleton - sống 100 năm) thì hoàn toàn vô hại. Sau 15 phút cốc kem tan hết và biến mất, ông lão vẫn tiếp tục sống khỏe mạnh mà không gây ra bất kỳ rò rỉ bộ nhớ nào.

---

### ❓ Câu 2: Ambient Context bị nhiều tác giả xem là mẫu thiết kế nguy hiểm. Nêu hai nhược điểm và cho biết vì sao vẫn chấp nhận được trong tình huống ở YC5?

#### 💡 Trả lời theo kiểu hiểu:

#### 1. Hai nhược điểm lớn của Ambient Context:
* **Nhược điểm 1: Che giấu phụ thuộc (Hidden Dependency / Liar API):**
  * Nhìn vào khai báo hàm `CreateOrder(ticket)`, người lập trình tưởng rằng chỉ cần truyền `ticket` là hàm chạy được. Nhưng khi chạy thì bị văng lỗi `NullReferenceException` vì bên trong hàm âm thầm đòi hỏi `StaffContext.Current` phải có dữ liệu. Phụ thuộc này bị giấu kín, không hiện rõ ở Constructor.
* **Nhược điểm 2: Rất khó viết Unit Test và dễ bị nhiễm bẩn dữ liệu chéo luồng:**
  * Do dùng trạng thái toàn cục/ngầm (`static`), khi chạy các bài test tự động song song, việc gán `StaffContext.Current` ở test case này có thể vô tình làm ảnh hưởng hoặc sai lệch kết quả của test case khác nếu không dọn dẹp kỹ.

#### 2. Vì sao trong tình huống ở YC5 vẫn chấp nhận được?
* **Tránh "Ô nhiễm tham số" (Parameter Drilling / Prop Drilling):** Thông tin nhân viên thu ngân (`Staff`) là thông tin xuyên suốt phiên làm việc. Nếu không dùng Ambient Context, lập trình viên sẽ buộc phải nhét tham số `Staff staff` vào mọi tầng, mọi hàm (từ Controller $\rightarrow$ Service $\rightarrow$ Repository $\rightarrow$ Logger), làm mã nguồn cực kỳ rườm rà.
* **An toàn nhờ `AsyncLocal`:** .NET sử dụng `AsyncLocal<T>`, đảm bảo giá trị ngữ cảnh được lưu riêng biệt theo từng luồng thực thi bất đồng bộ (ExecutionContext Flow), không bị lẫn lộn giữa các nhân viên ở các ca trực khác nhau.

---

## 💻 4. Hướng dẫn chạy chương trình trong Visual Studio

1. Trong Solution Explorer, click chuột phải vào **`FCanteen.ConsoleApp`** $\rightarrow$ chọn **Set as Startup Project**.
2. Nhấn **Ctrl + F5** (hoặc nút Start).
3. Menu tương tác hiển thị sẵn các tùy chọn:
   * **Phím 1:** Tạo đơn hàng mẫu & Áp dụng chính sách giảm giá OCP (YC2).
   * **Phím 2:** Thử nghiệm đổi kênh thông báo DIP (Console / File / Email) (YC3).
   * **Phím 3:** Kiểm tra vòng đời Service (Transient / Scoped / Singleton) (YC4).
   * **Phím 4:** Demo lỗi Captive Dependency & cách sửa (YC4).
   * **Phím 5:** Demo 4 kiểu Injection & Ambient Context (YC5).
   * **Phím 0:** Thoát.
