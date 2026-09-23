using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Services
{
    public class ItemEfficiencyResult
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalSold { get; set; }
        public double PeakHourRatio { get; set; }     // Ti le ban trong gio cao diem (11h-13h)
        public double EfficiencyIndex { get; set; }     // Chi so hieu qua tinh toan
    }

    public class BenchmarkReport
    {
        public int LogicalCores { get; set; }
        public long SequentialMs { get; set; }
        public long ParallelForEachMs { get; set; }
        public long Parallel2CoresMs { get; set; }
        public long Parallel4CoresMs { get; set; }
        public long ParallelMaxCoresMs { get; set; }

        public double SpeedupParallelForEach => SequentialMs > 0 ? (double)SequentialMs / ParallelForEachMs : 0;
        public double Speedup2Cores => SequentialMs > 0 ? (double)SequentialMs / Parallel2CoresMs : 0;
        public double Speedup4Cores => SequentialMs > 0 ? (double)SequentialMs / Parallel4CoresMs : 0;
        public double SpeedupMaxCores => SequentialMs > 0 ? (double)SequentialMs / ParallelMaxCoresMs : 0;
    }

    public class ItemEfficiencyService
    {
        private readonly FCanteenContext _context;

        public ItemEfficiencyService(FCanteenContext context)
        {
            _context = context;
        }

        // Ham tinh toan nang mo phong CPU-bound de danh gia hieu qua
        private ItemEfficiencyResult CalculateMetric(MenuItem item, List<TicketLine> lines)
        {
            int totalSold = lines.Sum(l => l.Quantity);
            decimal totalRev = lines.Sum(l => l.Quantity * l.UnitPrice);

            int peakCount = lines
                .Where(l => l.OrderTicket != null && l.OrderTicket.CreatedAt.Hour >= 11 && l.OrderTicket.CreatedAt.Hour <= 13)
                .Sum(l => l.Quantity);

            double peakRatio = totalSold > 0 ? (double)peakCount / totalSold : 0;

            // Phep tinh CPU-bound mo phong phan tich phuc tap (Monte Carlo / Matrix math)
            // de thuc su gay tai CPU ro ret giua tuan tu va song song
            double dummyWork = 0;
            for (int i = 0; i < 4000000; i++)
            {
                dummyWork += Math.Sin(i) * Math.Cos(i);
            }

            double efficiency = (double)totalRev * peakRatio * 0.0001 + (dummyWork % 1.0);

            return new ItemEfficiencyResult
            {
                MenuItemId = item.MenuItemId,
                ItemName = item.Name,
                TotalRevenue = totalRev,
                TotalSold = totalSold,
                PeakHourRatio = peakRatio,
                EfficiencyIndex = efficiency
            };
        }

        public async Task<BenchmarkReport> RunBenchmarkAsync()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("    YC2: SO SANH XU LY TUAN TU VA SONG SONG (CPU-BOUND BENCHMARK)");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            Console.WriteLine("[*] Dang nap du lieu MenuItem va TicketLine tu CSDL...");
            var menuItems = await _context.MenuItems.AsNoTracking().ToListAsync();

            // Nap TicketLine kem theo gio cua OrderTicket de tinh toan
            var ticketLines = await _context.TicketLines
                .AsNoTracking()
                .Include(l => l.OrderTicket)
                .ToListAsync();

            Console.WriteLine($"[*] Da nap {menuItems.Count} mon an va {ticketLines.Count:N0} chi tiet phieu vao bo nho.");
            Console.WriteLine($"[*] So loi CPU logic cua may: {Environment.ProcessorCount} cores\n");

            // Gom nhom san du lieu theo MenuItemId de san sang tinh toan
            var groupedData = menuItems.ToDictionary(
                m => m,
                m => ticketLines.Where(l => l.MenuItemId == m.MenuItemId).ToList()
            );

            var report = new BenchmarkReport
            {
                LogicalCores = Environment.ProcessorCount
            };

            var sw = new Stopwatch();

            // 1. Phien ban Tuáº§n tá»± (Sequential foreach)
            Console.Write("[1/5] Dang chay: foreach Tuan tu (Sequential)... ");
            var seqResults = new List<ItemEfficiencyResult>();
            sw.Restart();
            foreach (var kv in groupedData)
            {
                seqResults.Add(CalculateMetric(kv.Key, kv.Value));
            }
            sw.Stop();
            report.SequentialMs = sw.ElapsedMilliseconds;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Xong! Thoi gian: {report.SequentialMs} ms");
            Console.ResetColor();

            // 2. Phien ban Parallel.ForEach (Mac dinh)
            Console.Write("[2/5] Dang chay: Parallel.ForEach (Default)... ");
            var parResults = new ConcurrentBag<ItemEfficiencyResult>();
            sw.Restart();
            Parallel.ForEach(groupedData, kv =>
            {
                parResults.Add(CalculateMetric(kv.Key, kv.Value));
            });
            sw.Stop();
            report.ParallelForEachMs = sw.ElapsedMilliseconds;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! Thoi gian: {report.ParallelForEachMs} ms (Speedup: {report.SpeedupParallelForEach:F2}x)");
            Console.ResetColor();

            // 3. Parallel.ForEach voi MaxDegreeOfParallelism = 2
            Console.Write("[3/5] Dang chay: Parallel voi MaxDegree = 2... ");
            var par2Results = new ConcurrentBag<ItemEfficiencyResult>();
            sw.Restart();
            Parallel.ForEach(groupedData, new ParallelOptions { MaxDegreeOfParallelism = 2 }, kv =>
            {
                par2Results.Add(CalculateMetric(kv.Key, kv.Value));
            });
            sw.Stop();
            report.Parallel2CoresMs = sw.ElapsedMilliseconds;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! Thoi gian: {report.Parallel2CoresMs} ms (Speedup: {report.Speedup2Cores:F2}x)");
            Console.ResetColor();

            // 4. Parallel.ForEach voi MaxDegreeOfParallelism = 4
            Console.Write("[4/5] Dang chay: Parallel voi MaxDegree = 4... ");
            var par4Results = new ConcurrentBag<ItemEfficiencyResult>();
            sw.Restart();
            Parallel.ForEach(groupedData, new ParallelOptions { MaxDegreeOfParallelism = 4 }, kv =>
            {
                par4Results.Add(CalculateMetric(kv.Key, kv.Value));
            });
            sw.Stop();
            report.Parallel4CoresMs = sw.ElapsedMilliseconds;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! Thoi gian: {report.Parallel4CoresMs} ms (Speedup: {report.Speedup4Cores:F2}x)");
            Console.ResetColor();

            // 5. Parallel.ForEach voi MaxDegreeOfParallelism = So loi cua may
            Console.Write($"[5/5] Dang chay: Parallel voi MaxDegree = {report.LogicalCores} (So loi may)... ");
            var parMaxResults = new ConcurrentBag<ItemEfficiencyResult>();
            sw.Restart();
            Parallel.ForEach(groupedData, new ParallelOptions { MaxDegreeOfParallelism = report.LogicalCores }, kv =>
            {
                parMaxResults.Add(CalculateMetric(kv.Key, kv.Value));
            });
            sw.Stop();
            report.ParallelMaxCoresMs = sw.ElapsedMilliseconds;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! Thoi gian: {report.ParallelMaxCoresMs} ms (Speedup: {report.SpeedupMaxCores:F2}x)");
            Console.ResetColor();

            // Ghi ket qua vao DailySettlement theo yeu cau de bai
            var today = DateTime.Today;
            var branches = new[] { "CS1", "CS2", "CS3" };
            foreach (var br in branches)
            {
                var branchTickets = await _context.OrderTickets
                    .Where(t => t.BranchCode == br && t.CreatedAt.Date == today)
                    .ToListAsync();

                var settlement = new DailySettlement
                {
                    SettlementDate = today,
                    BranchCode = br,
                    TotalTickets = branchTickets.Count,
                    TotalRevenue = branchTickets.Sum(t => t.TotalAmount),
                    CalculationTimeMs = report.ParallelMaxCoresMs
                };
                _context.DailySettlements.Add(settlement);
            }
            await _context.SaveChangesAsync();

            // In bang ket qua cho bao cao Word
            PrintMarkdownTable(report);

            return report;
        }

        private void PrintMarkdownTable(BenchmarkReport r)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("        BANG KET QUA SO SANH HIEU NANG (DUNG CHO BAO CAO WORD)");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            Console.WriteLine($"So loi CPU logic (Logical Cores): {r.LogicalCores}\n");
            Console.WriteLine("| Phien ban thuc thi                   | Thoi gian (ms) | He so tang toc (Speedup) |");
            Console.WriteLine("| :----------------------------------- | :------------: | :----------------------: |");
            Console.WriteLine($"| 1. foreach Tuan tu (Sequential)      | {r.SequentialMs,14} | {"1.00x",24} |");
            Console.WriteLine($"| 2. Parallel.ForEach (Mac dinh)       | {r.ParallelForEachMs,14} | {$"{r.SpeedupParallelForEach:F2}x",24} |");
            Console.WriteLine($"| 3. Parallel (MaxDegree = 2)          | {r.Parallel2CoresMs,14} | {$"{r.Speedup2Cores:F2}x",24} |");
            Console.WriteLine($"| 4. Parallel (MaxDegree = 4)          | {r.Parallel4CoresMs,14} | {$"{r.Speedup4Cores:F2}x",24} |");
            Console.WriteLine($"| 5. Parallel (MaxDegree = {r.LogicalCores} loi)      | {r.ParallelMaxCoresMs,14} | {$"{r.SpeedupMaxCores:F2}x",24} |");
            Console.WriteLine("==========================================================================\n");
        }
    }
}