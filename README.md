# PRN222 - FCanteen
**Hệ thống quản lý căng tin FPT University Đà Nẵng**
> Môn học: PRN222 - Networking Programming | FPT University Đà Nẵng

---

## Thông tin sinh viên
| Mục | Chi tiết |
|---|---|
| **Họ và tên** | Châu Vương Hoàng |
| **Mã sinh viên** | DE180551 |
| **Môn học** | PRN222 - Networking Programming |
| **Lớp** | SE18 |

---

## Công nghệ sử dụng
- **Framework**: .NET 8 (Console Applications & Class Library)
- **ORM**: Entity Framework Core 8 (Code First)
- **Database**: Microsoft SQL Server Express (`PRN222_Lab_FCanteen`)
- **Networking**:
  - **TCP**: `TcpListener` (KitchenServer), `TcpClient` (PosClient)
  - **UDP**: `UdpClient` (Broadcast thông báo hết món đa quầy)
  - **HTTP**: `HttpClient` (Đồng bộ bảng giá từ xa)
  - **URI & DNS**: `Uri`, `Dns` (Phân tích endpoint, phân giải IP, ghi log DeviceLog)
- **Công cụ**: Visual Studio 2022 / VS Code, Git

---

## Quy định chung (Lab 00)
- [x] Sử dụng .NET 8, SQL Server, EF Core Code First
- [x] Mọi thay đổi schema qua `Add-Migration` và `Update-Database`
- [x] Connection string đặt trong `appsettings.json`, không hard-code trong source code
- [x] Mã nguồn trên GitHub, mỗi lab một branch riêng biệt (`lab01`)
- [x] Tối thiểu 3 commits mỗi bài lab (thực tế > 10 commits chi tiết theo từng YC)
- [x] Báo cáo nộp kèm gồm: ảnh chụp màn hình kết quả chạy, ảnh chụp SSMS, và câu hỏi lý thuyết

---

## Lab 01 - Networking Programming

### Tiến độ hoàn thành công việc
| Yêu cầu | Mô tả nội dung thực hiện | Điểm | Trạng thái |
|:---|:---|:---:|:---:|
| **YC1** | **Tầng dữ liệu Code First**: Tạo `FCanteen.Data`, 4 thực thể (`MenuItem`, `OrderTicket`, `TicketLine`, `DeviceLog`), cấu hình quan hệ, seed 15 món ăn qua `HasData`, khởi tạo migration và cập nhật database. | 1.5 | ✅ Hoàn thành |
| **YC2** | **Máy chủ bếp bằng TcpListener**: Tạo `FCanteen.KitchenServer`, lắng nghe cổng 9500 đa luồng (`Task.Run`), nhận JSON phiếu order, server tự tính lại tổng tiền từ DB, lưu vào `OrderTicket` & `TicketLine`, ghi nhận `DeviceLog`. | 2.5 | ✅ Hoàn thành |
| **YC3** | **Quầy thu ngân bằng TcpClient**: Tạo `FCanteen.PosClient`, hiển thị menu, cho phép chọn món, tạo phiếu order, kết nối TCP gửi JSON xuống KitchenServer và nhận phản hồi xác nhận. | 2.5 | ✅ Hoàn thành |
| **YC4** | **UDP + HttpClient đồng bộ bảng giá**: KitchenServer phát broadcast UDP cổng 9501 báo hết món; PosClient lắng nghe cập nhật realtime. PosClient dùng `Uri` và `Dns` phân giải host, dùng `HttpClient` tải `menu_prices.json`, cập nhật giá bán và ghi log `DeviceLog`. | 2.5 | ✅ Hoàn thành |
| **LT** | **Câu hỏi lý thuyết**: Hoàn thành giải đáp chi tiết về lý do chọn TCP/UDP, hệ quả khi đảo ngược giao thức, và cơ chế server tự tính tiền ngăn chặn gian lận (được lưu trong báo cáo nộp kèm theo quy định). | 1.0 | ✅ Hoàn thành |
| | **TỔNG ĐIỂM DỰ KIẾN** | **10.0 / 10.0** | 🏆 **100% Hoàn thành** |

---

## Cấu trúc Solution

```
PRN222-FCanteen/
├── .gitignore
├── README.md                           # Tài liệu tổng quan dự án & tiến độ Lab 01
├── menu_prices.json                    # Endpoint dữ liệu mock phục vụ HttpClient đồng bộ giá
└── FCanteen/
    ├── FCanteen.slnx                   # Solution file
    ├── FCanteen.Data/                  # Class Library - Tầng dữ liệu (YC1)
    │   ├── Entities/
    │   │   ├── MenuItem.cs             # Thực thể món ăn (mã, tên, giá, đơn vị, trạng thái)
    │   │   ├── OrderTicket.cs          # Thực thể phiếu order (mã phiếu, quầy, tổng tiền, ngày tạo)
    │   │   ├── TicketLine.cs           # Chi tiết dòng phiếu order (lưu UnitPrice tại thời điểm bán)
    │   │   └── DeviceLog.cs            # Nhật ký giao tiếp mạng (TCP/UDP/HTTP/DNS)
    │   ├── Migrations/                 # Migrations EF Core Code First
    │   ├── FCanteenContext.cs          # DbContext chính cấu hình Fluent API & HasData seed
    │   ├── FCanteenContextFactory.cs   # IDesignTimeDbContextFactory phục vụ Add-Migration
    │   ├── appsettings.json            # Chuỗi kết nối CSDL SQL Server Express
    │   └── README.md                   # Hướng dẫn chi tiết cho FCanteen.Data
    ├── FCanteen.KitchenServer/         # Console App - Máy chủ bếp (YC2, YC4)
    │   ├── Models/
    │   │   └── OrderDto.cs             # DTO nhận phiếu từ POS và trả kết quả
    │   ├── Program.cs                  # TcpListener cổng 9500 đa luồng + UDP Broadcast cổng 9501
    │   ├── appsettings.json
    │   └── README.md                   # Hướng dẫn chạy KitchenServer
    └── FCanteen.PosClient/             # Console App - Quầy thu ngân (YC3, YC4)
        ├── Models/
        │   └── OrderDto.cs             # DTO gửi phiếu order
        ├── Program.cs                  # Giao diện quầy, TcpClient, UDP Listener, HttpClient Uri/Dns
        ├── appsettings.json
        └── README.md                   # Hướng dẫn chạy PosClient
```

---

## Hướng dẫn khởi chạy và kiểm thử

### Yêu cầu môi trường
- .NET 8 SDK
- SQL Server Express (chạy instance local hoặc cấu hình trong `appsettings.json`)
- Visual Studio 2022 hoặc VS Code

### Thứ tự chạy kiểm thử hệ thống

1. **Chuẩn bị CSDL (Làm 1 lần đầu)**:
   ```bash
   dotnet ef database update --project FCanteen/FCanteen.Data --startup-project FCanteen/FCanteen.Data
   ```

2. **Khởi chạy HTTP Mock Server đồng bộ giá (YC4 - Tùy chọn)**:
   Mở terminal tại thư mục gốc và chạy:
   ```bash
   python -m http.server 8080
   # hoặc: npx http-server -p 8080
   ```

3. **Khởi chạy Máy chủ bếp (KitchenServer - YC2 & YC4)**:
   ```bash
   dotnet run --project FCanteen/FCanteen.KitchenServer
   ```
   - Server bắt đầu lắng nghe TCP cổng `9500` và sẵn sàng phát UDP Broadcast thông báo hết món trên cổng `9501`.

4. **Khởi chạy Quầy thu ngân (PosClient - YC3 & YC4)**:
   ```bash
   dotnet run --project FCanteen/FCanteen.PosClient
   ```
   - Có thể mở nhiều terminal để chạy nhiều instance POS (QUAY01, QUAY02, QUAY03).
   - Menu hỗ trợ:
     - `1`: Đặt món và gửi phiếu order xuống bếp qua TCP.
     - `2`: Đồng bộ bảng giá từ HTTP Endpoint qua `Uri`, `Dns` và `HttpClient`.
     - Nhận thông báo hết món tự động theo thời gian thực qua UDP broadcast.

---

## Lịch sử commit tiêu biểu (Branch `lab01`)

| Hash / Thẻ | Nội dung commit | Giai đoạn |
|:---|:---|:---:|
| `b6b2ff3` | `lab01: init - add lab00 and lab01 documents` | Khởi tạo |
| `ad2e7fc` | `feat: add DeviceLog-MenuItem-OrderTicket-TicketLine entity` | YC1 |
| `0c4d108` | `feat: initialize data access layer with Entity Framework Core` | YC1 |
| `aa7ade1` | `feat: initialize database schema and seed initial menu items` | YC1 |
| `2b028b2` | `docs: update README.md and Done YC1` | YC1 |
| `b01a561` | `feat: add FCanteen.KitchenServer project and configuration` | YC2 |
| `03695d7` | `lab01: update README - YC2 KitchenServer completed` | YC2 |
| `445a75f` | `lab01: YC3 - PosClient completed + add README for all projects` | YC3 |
| `adf9f4c` | `lab01: add menu_prices.json for HttpClient sync` | YC4 |
| `96fadae` | `lab01: YC4 - UDP soldout broadcast + HttpClient price sync with Uri/Dns` | YC4 |
| `ca54210` | `chore: test update Com ga price and reformat menu_prices.json` | YC4 |
