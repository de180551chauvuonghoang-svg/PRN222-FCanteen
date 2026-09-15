using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Services
{
    public class EfAsyncReportService
    {
        private readonly Func<FCanteenContext> _contextFactory;

        public EfAsyncReportService(Func<FCanteenContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task RunAsyncReportsAsync()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("    YC4: BAT DONG BO VOI EF CORE (Task.WhenAll, DbContext, IAsyncEnumerable)");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            // 1. Khoi tao CancellationToken voi timeout 30 giay theo de bai
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var ct = cts.Token;

            try
            {
                Console.WriteLine("[*] Khoi tao CancellationToken (Timeout = 30 giay)...");

                // =====================================================================
                // PHAN 1: SO SANH AWAIT TUAN TU VS TASK.WHENALL SONG SONG
                // =====================================================================
                Console.WriteLine("\n--- [1] SO SANH TOC DO TRUY VAN EF CORE ---");

                var sw = new Stopwatch();

                // 1A. Phien ban await noi tiep (Sequential Async)
                Console.Write("[A] Dang chay: 4 truy van await NOI TIEP (Sequential)... ");
                sw.Restart();
                var resSeq1 = await Query1_TotalAndRevenueAsync(ct);
                var resSeq2 = await Query2_BranchRevenueAsync(ct);
                var resSeq3 = await Query3_StatusCountsAsync(ct);
                var resSeq4 = await Query4_TopTicketsAsync(ct);
                sw.Stop();
                long seqMs = sw.ElapsedMilliseconds;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Xong! Thoi gian: {seqMs} ms");
                Console.ResetColor();

                // 1B. Phien ban Task.WhenAll dong thoi (Moi task 1 DbContext rieng)
                Console.Write("[B] Dang chay: 4 truy van Task.WhenAll DONG THOI (Moi task 1 DbContext)... ");
                sw.Restart();
                var task1 = Query1_TotalAndRevenueAsync(ct);
                var task2 = Query2_BranchRevenueAsync(ct);
                var task3 = Query3_StatusCountsAsync(ct);
                var task4 = Query4_TopTicketsAsync(ct);

                await Task.WhenAll(task1, task2, task3, task4);
                sw.Stop();
                long whenAllMs = sw.ElapsedMilliseconds;

                var res1 = await task1;
                var res2 = await task2;
                var res3 = await task3;
                var res4 = await task4;

                double speedup = whenAllMs > 0 ? (double)seqMs / whenAllMs : 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Xong! Thoi gian: {whenAllMs} ms (Speedup: {speedup:F2}x)");
                Console.ResetColor();

                // In bang so sanh
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n| Phuong phap thuc thi               | Thoi gian (ms) | Toc do cai thien |");
                Console.WriteLine("| :--------------------------------- | :------------: | :--------------: |");
                Console.WriteLine($"| 1. await Noi tiep (Sequential)     | {seqMs,14} | {"1.00x",16} |");
                Console.WriteLine($"| 2. Task.WhenAll (Dong thoi)        | {whenAllMs,14} | {$"{speedup:F2}x",16} |");
                Console.ResetColor();

                // In ket qua 4 truy van
                Console.WriteLine("\n[*] KET QUA 4 TRUY VAN THONG KE DONG THOI:");
                Console.WriteLine($"  - Truy van 1 (Tong quan): {res1.TotalTickets:N0} phieu | Doanh thu: {res1.TotalRevenue:N0} VND");
                Console.WriteLine($"  - Truy van 2 (Theo co so): {string.Join(" | ", res2.Select(x => $"{x.Branch}: {x.Revenue:N0} VND ({x.Count:N0} phieu)"))}");
                Console.WriteLine($"  - Truy van 3 (Trang thai): {string.Join(" | ", res3.Select(x => $"{x.Status}: {x.Count:N0}"))}");
                Console.WriteLine($"  - Truy van 4 (Top 1 gia tri cao nhat): Phieu #{res4.First().Id} ({res4.First().Amount:N0} VND)");

                // =====================================================================
                // PHAN 2: DUYET DU LIEU LON BANG IAsyncEnumerable (STREAMING)
                // =====================================================================
                Console.WriteLine("\n--- [2] DUYET PHIEU GIA TRI LON BANG IAsyncEnumerable (STREAMING) ---");
                Console.WriteLine("[*] Muc tieu: Stream du lieu tu CSDL tung dong ma KHONG nap toan bo vao bo nho (Memory Optimization).");

                decimal highValueThreshold = 100000m; // Phieu >= 100.000 VND
                int streamedCount = 0;
                decimal streamedTotal = 0;

                sw.Restart();
                using (var db = _contextFactory())
                {
                    // Su dung AsAsyncEnumerable de streaming records
                    var highValueQuery = db.OrderTickets
                        .AsNoTracking()
                        .Where(t => t.TotalAmount >= highValueThreshold)
                        .OrderByDescending(t => t.CreatedAt)
                        .AsAsyncEnumerable();

                    await foreach (var ticket in highValueQuery.WithCancellation(ct))
                    {
                        streamedCount++;
                        streamedTotal += ticket.TotalAmount;

                        // In mau 3 phieu dau tien
                        if (streamedCount <= 3)
                        {
                            Console.WriteLine($"  >> Streamed Phieu #{ticket.OrderTicketId} | Co so: {ticket.BranchCode} | {ticket.TotalAmount:N0} VND ({ticket.CreatedAt:dd/MM/yyyy HH:mm})");
                        }
                    }
                }
                sw.Stop();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[THANH CONG] Da stream thanh cong {streamedCount:N0} phieu lon (>= {highValueThreshold:N0} VND).");
                Console.WriteLine($"[TONG DOANH THU STREAM]: {streamedTotal:N0} VND | Thoi gian stream: {sw.ElapsedMilliseconds} ms");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("    GIAI THICH KIEN TRUC BAT DONG BO TRONG YC4 (DUNG CHO BAO CAO)");
                Console.WriteLine("==========================================================================");
                Console.ResetColor();
                Console.WriteLine("1. Vi sao moi truy van can mot DbContext rieng trong Task.WhenAll?");
                Console.WriteLine("   - DbContext cua EF Core KHONG ho tro Thread-Safety. Neu 2 truy van async chay dong");
                Console.WriteLine("     thoi tren cung 1 instance DbContext se bi loi: InvalidOperationException.");
                Console.WriteLine("   - Do do, moi Task phai khoi tao mot DbContext doc lap (thong qua factory).");
                Console.WriteLine("\n2. Loi ich cua IAsyncEnumerable so voi ToListAsync():");
                Console.WriteLine("   - ToListAsync() bat buoc SQL Server phai tra ve toan bo ket qua va RAM phai chua");
                Console.WriteLine("     hang chuc nghin object cung luc (de gay Out-Of-Memory hoac GC Pause).");
                Console.WriteLine("   - IAsyncEnumerable su dung IDataReader stream tung dong du lieu len RAM de xu ly");
                Console.WriteLine("     ngay, giu muc su dung bo nho RAM o muc cuc ky thap va on dinh.");
                Console.WriteLine("==========================================================================\n");
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[HUY TAC VU] Qua trinh thuc thi da bi huy do vuot qua nguong Timeout 30 giay (OperationCanceledException).");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[LOI]: {ex.Message}");
                Console.ResetColor();
            }
        }

        // Truy van 1: Tong phieu va tong doanh thu
        private async Task<(int TotalTickets, decimal TotalRevenue)> Query1_TotalAndRevenueAsync(CancellationToken ct)
        {
            using var db = _contextFactory();
            int count = await db.OrderTickets.CountAsync(ct);
            decimal sum = await db.OrderTickets.SumAsync(t => t.TotalAmount, ct);
            return (count, sum);
        }

        // Truy van 2: Doanh thu theo co so
        private async Task<List<(string Branch, decimal Revenue, int Count)>> Query2_BranchRevenueAsync(CancellationToken ct)
        {
            using var db = _contextFactory();
            var data = await db.OrderTickets
                .GroupBy(t => t.BranchCode)
                .Select(g => new
                {
                    Branch = g.Key,
                    Revenue = g.Sum(t => t.TotalAmount),
                    Count = g.Count()
                })
                .ToListAsync(ct);

            return data.Select(x => (x.Branch, x.Revenue, x.Count)).ToList();
        }

        // Truy van 3: So luong phieu theo trang thai
        private async Task<List<(string Status, int Count)>> Query3_StatusCountsAsync(CancellationToken ct)
        {
            using var db = _contextFactory();
            var data = await db.OrderTickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.Status, x.Count)).ToList();
        }

        // Truy van 4: Top 5 phieu gia tri cao nhat
        private async Task<List<(int Id, decimal Amount)>> Query4_TopTicketsAsync(CancellationToken ct)
        {
            using var db = _contextFactory();
            var data = await db.OrderTickets
                .OrderByDescending(t => t.TotalAmount)
                .Take(5)
                .Select(t => new { Id = t.OrderTicketId, Amount = t.TotalAmount })
                .ToListAsync(ct);

            return data.Select(x => (x.Id, x.Amount)).ToList();
        }
    }
}