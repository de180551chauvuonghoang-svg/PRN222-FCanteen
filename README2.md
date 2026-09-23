# FCanteen — Lab 02: Asynchronous & Parallel Programming
> **Môn học:** PRN222 — Lập trình .NET nâng cao  
> **Trường:** FPT University Đà Nẵng  
> **Case Study:** Hệ thống quản lý căng tin FCanteen (Mở rộng 3 cơ sở & xử lý dữ liệu lớn)  
> **Tài liệu:** Hướng dẫn chi tiết từng bước thực hiện Lab 02

---

## 📋 Mục lục & Thang điểm

| Yêu cầu | Nội dung chính | Điểm | Trạng thái |
| :--- | :--- | :---: | :---: |
| **YC1** | Mở rộng Schema (`Ingredient`, `DailySettlement`, `BranchCode`) & Seeder 50.000 Tickets | 1.5đ | ⏳ Chưa làm |
| **YC2** | So sánh CPU-bound: Tuần tự vs `Parallel.ForEach` vs `Parallel.For` (MaxDegreeOfParallelism) | 2.5đ | ⏳ Chưa làm |
| **YC3** | Thống kê dữ liệu lớn bằng PLINQ (4 báo cáo, `AsOrdered()`, đo thời gian) | 2.0đ | ⏳ Chưa làm |
| **YC4** | Bất đồng bộ với EF Core (`Task.WhenAll`, `IDbContextFactory`, `CancellationToken`, `IAsyncEnumerable`) | 2.0đ | ⏳ Chưa làm |
| **YC5** | Demo Race Condition tồn kho nguyên liệu (Unsafe vs `Interlocked` / `lock`) | 1.0đ | ⏳ Chưa làm |
| **Lý thuyết** | Giải thích Amdahl, Overhead đa luồng, Phân biệt CPU-bound vs I/O-bound | 1.0đ | ⏳ Chưa làm |
| **TỔNG** | **Toàn bộ Lab 02** | **10.0đ** | **Chuẩn bị triển khai** |

---

## 🎯 Bối cảnh nghiệp vụ Lab 02
FCanteen sau 3 tháng hoạt động đã mở rộng ra 3 cơ sở (`CS1`, `CS2`, `CS3`). Lượng dữ liệu tích luỹ đạt trên **50.000 phiếu (`OrderTicket`)** và hơn **200.000 chi tiết phiếu (`TicketLine`)**.
* Kế toán phản ánh: Báo cáo cuối ngày chạy quá chậm (mất gần 4 phút).
* Nhà bếp phản ánh: Không nhận diện kịp thời nguyên liệu nào sắp hết để nhập hàng.
* Mục tiêu: Ứng dụng **Lập trình bất đồng bộ (Asynchronous)** và **Lập trình song song (Parallel / PLINQ)** để tối ưu hoá hiệu năng toàn diện.

---

## 🛠️ Hướng dẫn chi tiết từng yêu cầu (Step-by-Step)

### YC1. Mở rộng Schema & Sinh dữ liệu lớn (1.5 điểm)

#### 1.1. Bổ sung các Entity vào Domain / Models
1. **Entity `Ingredient` (Nguyên liệu):**
   ```csharp
   public class Ingredient
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
       public string Unit { get; set; } = string.Empty; // kg, lít, gói, quả...
       public int StockQuantity { get; set; }           // Tồn kho hiện tại
       public int WarningThreshold { get; set; }        // Ngưỡng cảnh báo sắp hết
   }
   ```
2. **Entity `DailySettlement` (Quyết toán ngày):**
   ```csharp
   public class DailySettlement
   {
       public int Id { get; set; }
       public DateTime SettlementDate { get; set; }
       public string BranchCode { get; set; } = string.Empty; // CS1, CS2, CS3
       public int TotalTickets { get; set; }
       public decimal TotalRevenue { get; set; }
       public long CalculationTimeMs { get; set; }     // Thời gian tính toán (ms)
   }
   ```
3. **Cập nhật `OrderTicket`:** Thêm thuộc tính `BranchCode`:
   ```csharp
   public string BranchCode { get; set; } = "CS1"; // Giá trị mặc định hoặc CS1/CS2/CS3
   ```

#### 1.2. Tạo Migration và Update Database
```bash
dotnet ef migrations add AddLab02Schema --project FCanteen.Data
dotnet ef database update --project FCanteen.Data
```

#### 1.3. Viết Seeder dữ liệu lớn (Big Data Seeder)
* **Quy mô:** $\ge 50.000$ `OrderTicket` và $\ge 200.000$ `TicketLine`.
* **Phân bố:** 
  * Trải đều trong 6 tháng gần nhất.
  * Phân bổ ngẫu nhiên theo 3 cơ sở: `CS1`, `CS2`, `CS3`.
  * **Trọng số giờ cao điểm:** $60 - 70\%$ số đơn tập trung vào khung giờ **11h00 – 13h00**.
* **Kỹ thuật tối ưu bắt buộc (tránh sập RAM / tràn bộ nhớ):**
  * Tắt kiểm tra thay đổi: `context.ChangeTracker.AutoDetectChangesEnabled = false;`
  * Chạy theo từng lô (Batching): `SaveChanges()` sau mỗi **1.000 - 2.000 đơn hàng**.
  * Giải phóng bộ nhớ sau mỗi batch: `context.ChangeTracker.Clear();`
  * Đo thời gian sinh bằng `Stopwatch` và in ra màn hình Console.

---

### YC2. So sánh xử lý tuần tự và song song (2.5 điểm)

#### 2.1. Nghiệp vụ: Tính "Chỉ số hiệu quả món ăn" (Item Efficiency Index)
* Với mỗi `MenuItem`:
  * Duyệt toàn bộ các `TicketLine` của món đó trong lịch sử.
  * Thực hiện phép tính toán đủ nặng (CPU-bound) mô phỏng phân tích: tỉ suất bán theo khung giờ, hệ số biến động theo ngày, tỷ trọng đóng góp doanh thu.
* Đảm bảo công thức tiêu tốn CPU đủ để thấy rõ sự khác biệt giữa tuần tự và song song.

#### 2.2. Triển khai 3 phiên bản & đo bằng `Stopwatch`:
1. **Phiên bản 1 (Sequential):** `foreach (var item in menuItems)`
2. **Phiên bản 2 (Parallel.ForEach):** `Parallel.ForEach(menuItems, item => { ... })`
3. **Phiên bản 3 (Parallel.For / ParallelOptions):**
   * Chạy với `ParallelOptions.MaxDegreeOfParallelism = 2`
   * Chạy với `ParallelOptions.MaxDegreeOfParallelism = 4`
   * Chạy với `ParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount` (Số lõi logic của máy)

#### 2.3. Báo cáo kết quả:
* Lập bảng so sánh vào file nộp bài:
  $$\text{Speedup (Hệ số tăng tốc)} = \frac{T_{\text{sequential}}}{T_{\text{parallel}}}$$
* Lưu kết quả tổng hợp ngày xuống bảng `DailySettlement`.

---

### YC3. Báo cáo bằng PLINQ (Parallel LINQ) (2.0 điểm)

Triển khai 4 báo cáo thống kê trên tập dữ liệu $\ge 50.000$ phiếu, mỗi báo cáo so sánh thời gian thực thi giữa **LINQ tuần tự** và **PLINQ (`.AsParallel()`)**:

1. **Báo cáo 1: Top 10 món bán chạy nhất theo doanh thu**
   * Nhóm theo `MenuItemId`, tính tổng doanh thu, sắp xếp giảm dần, lấy 10 món đầu.
   * *Yêu cầu:* Sử dụng `.AsOrdered()` để đảm bảo thứ tự chính xác khi phân luồng song song.
2. **Báo cáo 2: Doanh thu theo từng khung giờ trong ngày (0h - 23h)**
   * Nhóm theo `TicketDate.Hour`, tính tổng doanh thu từng giờ.
3. **Báo cáo 3: Cơ sở có doanh thu cao nhất từng tháng**
   * Nhóm theo `Year`, `Month`, `BranchCode`, tìm cơ sở dẫn đầu mỗi tháng.
4. **Báo cáo 4: Danh sách món có doanh thu dưới 1% tổng doanh thu**
   * Lọc những món đóng góp $< 1\%$ doanh thu $\rightarrow$ Đưa ra đề xuất loại bỏ khỏi thực đơn.

> **Điểm mấu chốt trong báo cáo:**  
> Giải thích vì sao **Báo cáo 1 (Top 10)** bắt buộc cần `.AsOrdered()` (để giữ đúng thứ tự xếp hạng doanh thu sau khi chia nhỏ luồng), còn các báo cáo nhóm (GroupBy theo Hour, theo Branch) thì không cần thiết vì phép tổng hợp/gom nhóm không phụ thuộc vào thứ tự ban đầu của từng dòng.

---

### YC4. Bất đồng bộ với EF Core (2.0 điểm)

#### 4.1. Chạy đa truy vấn thống kê đồng thời (`Task.WhenAll`)
* **Lưu ý sống còn:** `DbContext` trong EF Core **KHÔNG thread-safe**. Không được dùng chung 1 instance `DbContext` cho nhiều `Task` chạy đồng thời!
* **Giải pháp:** Sử dụng `IDbContextFactory<FCanteenDbContext>` hoặc tạo `IServiceScope` riêng để mỗi truy vấn chạy trên một `DbContext` độc lập:
  ```csharp
  var task1 = Task.Run(async () => {
      using var ctx = _contextFactory.CreateDbContext();
      return await ctx.OrderTickets.CountAsync();
  });
  var task2 = Task.Run(async () => {
      using var ctx = _contextFactory.CreateDbContext();
      return await ctx.OrderTickets.SumAsync(t => t.TotalAmount);
  });
  // Chạy đồng thời cả 4 task
  await Task.WhenAll(task1, task2, task3, task4);
  ```

#### 4.2. So sánh với phiên bản `await` nối tiếp
* Chạy 4 truy vấn tuần tự: `await Query1(); await Query2(); await Query3(); await Query4();`
* Đo `Stopwatch` so sánh giữa nối tiếp và đồng thời (`Task.WhenAll`).

#### 4.3. Quản lý Timeout với `CancellationToken`
* Khởi tạo: `using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));`
* Truyền `cts.Token` vào tất cả các phương thức async (`ToListAsync(cts.Token)`, `CountAsync(cts.Token)`...).
* Bắt `OperationCanceledException` và in thông báo huỷ tác vụ an toàn.

#### 4.4. Đọc dữ liệu lớn bằng `IAsyncEnumerable<T>`
* Sử dụng `_context.OrderTickets.Where(t => t.TotalAmount > 200000).AsAsyncEnumerable()`
* Duyệt bằng `await foreach (var ticket in ...)` để stream dữ liệu từng dòng, không load hàng chục nghìn record cùng lúc vào RAM.

---

### YC5. Race Condition khi trừ tồn kho nguyên liệu (1.0 điểm)

Mô phỏng tình huống nhiều order được thanh toán đồng thời và cùng trừ số lượng nguyên liệu (ví dụ: món "Trà sữa" cần trừ nguyên liệu "Đường" và "Sữa"):

#### 5.1. Phiên bản lỗi (Unsafe - Không đồng bộ)
* Chạy nhiều Task hoặc `Parallel.ForEach` cùng đọc và trừ trực tiếp:
  ```csharp
  // LỖI: Race condition do thao tác trừ không nguyên tử (Atomic)
  ingredient.StockQuantity -= quantity; 
  ```
* **Hiện tượng:** Xuất hiện lỗi **Lost Update** (Cập nhật bị mất). Tồn kho thực tế cuối cùng lớn hơn nhiều so với con số chuẩn xác.

#### 5.2. Phiên bản an toàn (Thread-safe)
* Cách 1: Sử dụng `Interlocked`:
  ```csharp
  Interlocked.Add(ref stockQuantity, -quantity);
  ```
* Cách 2: Sử dụng `lock`:
  ```csharp
  lock (_lockObj)
  {
      ingredient.StockQuantity -= quantity;
  }
  ```
* **Bằng chứng:** Chụp màn hình Console hiển thị số liệu tồn kho bị sai ở bản Unsafe và số liệu chuẩn xác $100\%$ ở bản Thread-safe để dán vào báo cáo.

---

### 📚 Câu hỏi lý thuyết (1.0 điểm)

#### Câu 1: Vì sao `Parallel.ForEach` không cho tốc độ nhanh gấp đúng số lõi của máy?
1. **Định luật Amdahl (Amdahl's Law):** Bất kỳ chương trình nào cũng có phần mã bắt buộc phải chạy tuần tự (khởi tạo, nạp dữ liệu, phân chia luồng, gộp kết quả). Tốc độ tối đa bị giới hạn bởi phần tuần tự này.
2. **Chi phí quản lý luồng (Overhead):** Việc tạo Task, chuyển đổi ngữ cảnh CPU (Context Switching), đồng bộ hóa luồng (Synchronization) và lập lịch (Task Scheduler) tiêu tốn thêm tài nguyên CPU.
3. **Nghẽn phần cứng (Hardware Contention):** Các lõi CPU phải chia sẻ chung băng thông bộ nhớ RAM (Memory Bandwidth), bộ nhớ đệm (L3 Cache), và gặp hiện tượng đụng độ cache line (False Sharing).

#### Câu 2: Ở YC4, CPU gần như không tăng tải khi chạy `Task.WhenAll` nhưng thời gian vẫn giảm. Giải thích và phân biệt CPU-bound vs I/O-bound?
1. **Phân biệt CPU-bound và I/O-bound:**
   * **CPU-bound:** Tác vụ tiêu tốn chu kỳ tính toán của chip (tính toán ma trận, nén file, mã hoá, xử lý ảnh...). Muốn tăng tốc phải chia việc cho nhiều lõi CPU (`Parallel`, `PLINQ`).
   * **I/O-bound:** Tác vụ chờ đợi phản hồi từ thiết bị bên ngoài (truy vấn SQL Server, đọc ghi file ổ cứng, gọi HTTP API...). CPU không cần làm việc gì trong lúc chờ.
2. **Giải thích hiện tượng ở YC4:**
   * Truy vấn cơ sở dữ liệu là tác vụ **I/O-bound**.
   * Khi gọi `await` các hàm async của EF Core, luồng của ứng dụng được trả về cho ThreadPool (nhờ cơ chế I/O Completion Ports - IOCP của HĐH). CPU của máy chủ web/app hầu như ở trạng thái nhàn rỗi (CPU 0–5%).
   * Khi dùng `Task.WhenAll`, cả 4 truy vấn được gửi đồng thời xuống SQL Server. SQL Server xử lý song song và trả về cùng lúc. Thời gian hoàn thành giảm từ **(T1 + T2 + T3 + T4)** xuống chỉ còn xấp xỉ **Max(T1, T2, T3, T4)** mà không tốn thêm tải CPU của ứng dụng.

---

## 🚀 Kế hoạch triển khai mã nguồn theo thứ tự đề xuất

```text
Bước 1: Domain & DbContext (Thêm Ingredient, DailySettlement, BranchCode)
   └── Migration & Update-Database
Bước 2: Data Seeder Console
   └── Sinh 50.000 Tickets + 200.000 Lines (Batching + AutoDetectChanges = false)
Bước 3: Service xử lý Parallel (YC2)
   └── ItemEfficiencyService: Sequential vs Parallel.ForEach vs Parallel.For
Bước 4: Service báo cáo PLINQ (YC3)
   └── PlinqReportService: 4 Báo cáo (Top 10 với AsOrdered, Theo giờ, Theo cơ sở, <1%)
Bước 5: Service bất đồng bộ EF Core (YC4)
   └── EfAsyncReportService: Task.WhenAll + IDbContextFactory + CancellationToken + IAsyncEnumerable
Bước 6: Demo Race Condition (YC5)
   └── InventorySimulation: Unsafe vs Interlocked/lock
Bước 7: Tổng hợp báo cáo Word & chụp ảnh kết quả
```
