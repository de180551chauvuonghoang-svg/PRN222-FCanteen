using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Services
{
    public class PlinqReportResult
    {
        public string ReportName { get; set; } = string.Empty;
        public long SequentialMs { get; set; }
        public long PlinqMs { get; set; }
        public double Speedup => PlinqMs > 0 ? (double)SequentialMs / PlinqMs : 0;
        public string SampleOutput { get; set; } = string.Empty;
    }

    public class PlinqReportService
    {
        private readonly FCanteenContext _context;

        public PlinqReportService(FCanteenContext context)
        {
            _context = context;
        }

        public async Task<List<PlinqReportResult>> RunReportsAsync()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("        YC3: BAO CAO THONG KE DU LIEU LON BANG PLINQ (PARALLEL LINQ)");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            Console.WriteLine("[*] Dang nap du lieu TicketLine va OrderTicket vao bo nho (In-Memory)...");
            var ticketLines = await _context.TicketLines
                .AsNoTracking()
                .Include(l => l.MenuItem)
                .Include(l => l.OrderTicket)
                .ToListAsync();

            if (ticketLines.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Khong co du lieu TicketLine trong CSDL. Vui long chay lenh 'seed' truoc!");
                Console.ResetColor();
                return new List<PlinqReportResult>();
            }

            Console.WriteLine($"[*] Da nap thanh cong {ticketLines.Count:N0} records. Bat dau thuc thi 4 bao cao...\n");

            var results = new List<PlinqReportResult>();
            var sw = new Stopwatch();

            // -------------------------------------------------------------------------
            // BAO CAO 1: Top 10 mon ban chay nhat theo doanh thu (BAT BUOC DUNG AsOrdered())
            // -------------------------------------------------------------------------
            Console.Write("[1/4] Dang tinh: BÃ¡o cÃ¡o 1 - Top 10 mÃ³n bÃ¡n cháº¡y nháº¥t... ");
            // 1a. LINQ Tuan tu
            sw.Restart();
            var rep1Seq = ticketLines
                .GroupBy(l => new { l.MenuItemId, ItemName = l.MenuItem != null ? l.MenuItem.Name : $"Mon #{l.MenuItemId}" })
                .Select(g => new
                {
                    Name = g.Key.ItemName,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    TotalQty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();
            sw.Stop();
            long rep1SeqMs = sw.ElapsedMilliseconds;

            // 1b. PLINQ voi AsOrdered()
            sw.Restart();
            var rep1Plinq = ticketLines
                .AsParallel()
                .AsOrdered() // DUNG CU PHAP: AsOrdered phai goi ngay sau AsParallel de bao toan thu tu nguon
                .GroupBy(l => new { l.MenuItemId, ItemName = l.MenuItem != null ? l.MenuItem.Name : $"Mon #{l.MenuItemId}" })
                .Select(g => new
                {
                    Name = g.Key.ItemName,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    TotalQty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();
            sw.Stop();
            long rep1PlinqMs = sw.ElapsedMilliseconds;

            string rep1Summary = string.Join(", ", rep1Plinq.Take(3).Select(x => $"{x.Name} ({x.Revenue:N0}d)"));
            results.Add(new PlinqReportResult
            {
                ReportName = "1. Top 10 mon ban chay nhat (dung AsOrdered)",
                SequentialMs = rep1SeqMs,
                PlinqMs = rep1PlinqMs,
                SampleOutput = $"Top 3: {rep1Summary}"
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! (LINQ: {rep1SeqMs}ms | PLINQ: {rep1PlinqMs}ms | Speedup: {results[0].Speedup:F2}x)");
            Console.ResetColor();

            // -------------------------------------------------------------------------
            // BAO CAO 2: Doanh thu theo tung khung gio trong ngay (0h - 23h)
            // -------------------------------------------------------------------------
            Console.Write("[2/4] Dang tinh: BÃ¡o cÃ¡o 2 - Doanh thu theo tá»«ng khung giá»... ");
            // 2a. LINQ Tuan tu
            sw.Restart();
            var rep2Seq = ticketLines
                .Where(l => l.OrderTicket != null)
                .GroupBy(l => l.OrderTicket.CreatedAt.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    OrderCount = g.Select(x => x.OrderTicketId).Distinct().Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();
            sw.Stop();
            long rep2SeqMs = sw.ElapsedMilliseconds;

            // 2b. PLINQ
            sw.Restart();
            var rep2Plinq = ticketLines
                .AsParallel()
                .Where(l => l.OrderTicket != null)
                .GroupBy(l => l.OrderTicket.CreatedAt.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    OrderCount = g.Select(x => x.OrderTicketId).Distinct().Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();
            sw.Stop();
            long rep2PlinqMs = sw.ElapsedMilliseconds;

            var peakHour = rep2Plinq.OrderByDescending(x => x.Revenue).FirstOrDefault();
            results.Add(new PlinqReportResult
            {
                ReportName = "2. Doanh thu theo tung khung gio trong ngay",
                SequentialMs = rep2SeqMs,
                PlinqMs = rep2PlinqMs,
                SampleOutput = peakHour != null ? $"Gio cao diem nhat: {peakHour.Hour}h ({peakHour.Revenue:N0}d)" : ""
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! (LINQ: {rep2SeqMs}ms | PLINQ: {rep2PlinqMs}ms | Speedup: {results[1].Speedup:F2}x)");
            Console.ResetColor();

            // -------------------------------------------------------------------------
            // BAO CAO 3: Co so co doanh thu cao nhat tung thang
            // -------------------------------------------------------------------------
            Console.Write("[3/4] Dang tinh: BÃ¡o cÃ¡o 3 - CÆ¡ sá»Ÿ doanh thu cao nháº¥t tá»«ng thÃ¡ng... ");
            // 3a. LINQ Tuan tu
            sw.Restart();
            var rep3Seq = ticketLines
                .Where(l => l.OrderTicket != null)
                .GroupBy(l => new
                {
                    l.OrderTicket.CreatedAt.Year,
                    l.OrderTicket.CreatedAt.Month,
                    l.OrderTicket.BranchCode
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Branch = g.Key.BranchCode,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => g.OrderByDescending(x => x.Revenue).First())
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();
            sw.Stop();
            long rep3SeqMs = sw.ElapsedMilliseconds;

            // 3b. PLINQ
            sw.Restart();
            var rep3Plinq = ticketLines
                .AsParallel()
                .Where(l => l.OrderTicket != null)
                .GroupBy(l => new
                {
                    l.OrderTicket.CreatedAt.Year,
                    l.OrderTicket.CreatedAt.Month,
                    l.OrderTicket.BranchCode
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Branch = g.Key.BranchCode,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => g.OrderByDescending(x => x.Revenue).First())
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();
            sw.Stop();
            long rep3PlinqMs = sw.ElapsedMilliseconds;

            string rep3Sample = rep3Plinq.Count > 0 ? $"Thang {rep3Plinq.Last().Month}/{rep3Plinq.Last().Year}: {rep3Plinq.Last().Branch} ({rep3Plinq.Last().Revenue:N0}d)" : "";
            results.Add(new PlinqReportResult
            {
                ReportName = "3. Co so co doanh thu cao nhat tung thang",
                SequentialMs = rep3SeqMs,
                PlinqMs = rep3PlinqMs,
                SampleOutput = rep3Sample
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! (LINQ: {rep3SeqMs}ms | PLINQ: {rep3PlinqMs}ms | Speedup: {results[2].Speedup:F2}x)");
            Console.ResetColor();

            // -------------------------------------------------------------------------
            // BAO CAO 4: Danh sach mon duoi 1% tong doanh thu (De xuat loai bo)
            // -------------------------------------------------------------------------
            Console.Write("[4/4] Dang tinh: BÃ¡o cÃ¡o 4 - MÃ³n dÆ°á»›i 1% tá»•ng doanh thu... ");
            // 4a. LINQ Tuan tu
            sw.Restart();
            decimal totalRevenueSeq = ticketLines.Sum(x => x.Quantity * x.UnitPrice);
            decimal thresholdSeq = totalRevenueSeq * 0.01m;
            var rep4Seq = ticketLines
                .GroupBy(l => new { l.MenuItemId, ItemName = l.MenuItem != null ? l.MenuItem.Name : $"Mon #{l.MenuItemId}" })
                .Select(g => new
                {
                    Name = g.Key.ItemName,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    Percentage = totalRevenueSeq > 0 ? (double)(g.Sum(x => x.Quantity * x.UnitPrice) / totalRevenueSeq) * 100 : 0
                })
                .Where(x => x.Revenue < thresholdSeq)
                .OrderBy(x => x.Revenue)
                .ToList();
            sw.Stop();
            long rep4SeqMs = sw.ElapsedMilliseconds;

            // 4b. PLINQ
            sw.Restart();
            decimal totalRevenuePlinq = ticketLines.AsParallel().Sum(x => x.Quantity * x.UnitPrice);
            decimal thresholdPlinq = totalRevenuePlinq * 0.01m;
            var rep4Plinq = ticketLines
                .AsParallel()
                .GroupBy(l => new { l.MenuItemId, ItemName = l.MenuItem != null ? l.MenuItem.Name : $"Mon #{l.MenuItemId}" })
                .Select(g => new
                {
                    Name = g.Key.ItemName,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    Percentage = totalRevenuePlinq > 0 ? (double)(g.Sum(x => x.Quantity * x.UnitPrice) / totalRevenuePlinq) * 100 : 0
                })
                .Where(x => x.Revenue < thresholdPlinq)
                .OrderBy(x => x.Revenue)
                .ToList();
            sw.Stop();
            long rep4PlinqMs = sw.ElapsedMilliseconds;

            results.Add(new PlinqReportResult
            {
                ReportName = "4. Danh sach mon duoi 1% doanh thu (De xuat loai bo)",
                SequentialMs = rep4SeqMs,
                PlinqMs = rep4PlinqMs,
                SampleOutput = rep4Plinq.Count > 0 ? $"Phat hien {rep4Plinq.Count} mon: {string.Join(", ", rep4Plinq.Select(x => $"{x.Name} ({x.Percentage:F2}%)"))}" : "Khong co mon nao < 1%"
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! (LINQ: {rep4SeqMs}ms | PLINQ: {rep4PlinqMs}ms | Speedup: {results[3].Speedup:F2}x)");
            Console.ResetColor();

            // In bang ket qua Markdown
            PrintTable(results);

            // In phan giai thich AsOrdered()
            PrintAsOrderedExplanation();

            return results;
        }

        private void PrintTable(List<PlinqReportResult> results)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n==========================================================================================");
            Console.WriteLine("           BANG KET QUA SO SANH LINQ TUAN TU VS PLINQ (DUNG CHO BAO CAO WORD)");
            Console.WriteLine("==========================================================================================");
            Console.ResetColor();

            Console.WriteLine("| STT | Ten bao cao thong ke                   | LINQ Tuan tu (ms) | PLINQ (ms) | Speedup |");
            Console.WriteLine("| :-- | :------------------------------------- | :---------------: | :--------: | :-----: |");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Console.WriteLine($"| {i + 1,3} | {r.ReportName,-40} | {r.SequentialMs,17} | {r.PlinqMs,10} | {$"{r.Speedup:F2}x",7} |");
            }
            Console.WriteLine("==========================================================================================\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[*] CHI TIET KET QUA DAU RA CUA TUNG BAO CAO:");
            Console.ResetColor();
            foreach (var r in results)
            {
                Console.WriteLine($"  - {r.ReportName}:");
                Console.WriteLine($"    ==> {r.SampleOutput}");
            }
            Console.WriteLine();
        }

        private void PrintAsOrderedExplanation()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("==========================================================================================");
            Console.WriteLine("      GIAI THICH VAI TRO CUA AsOrdered() TRONG BAO CAO 1 (TOP 10 MON)");
            Console.WriteLine("==========================================================================================");
            Console.ResetColor();
            Console.WriteLine("1. Ly do Bao cao 1 BAT BUOC can AsOrdered():");
            Console.WriteLine("   - Bao cao 1 yeu cau lay Top 10 mon theo doanh thu giam dan (OrderByDescending).");
            Console.WriteLine("   - Trong PLINQ, khi phan chia du lieu thanh nhieu partition tren cac luong, viec Take(10)");
            Console.WriteLine("     neu khong co AsOrdered() co the gom cac phan tu khong dung thu tu xep hang da sort.");
            Console.WriteLine("   - AsOrdered() bat buoc PLINQ phai giu nguyen trat tu da sap xep khi gop ket qua lai.");
            Console.WriteLine("\n2. Ly do 3 bao cao con lai KHONG CAN AsOrdered():");
            Console.WriteLine("   - Bao cao 2, 3 va 4 chu yeu thuc hien phep gom nhom (GroupBy theo Gio, theo Co so,");
            Console.WriteLine("     hoac tong hop tong doanh thu). Phep toan GroupBy va Sum la cac phep toan giao hoan");
            Console.WriteLine("     (commutative), ket qua tinh tong khong he phu thuoc vao thu tu dong duoc xu ly.");
            Console.WriteLine("   - Khong dung AsOrdered() giup PLINQ bo qua chi phi quan ly thu tu (ordering overhead),");
            Console.WriteLine("     giup toc do chay song song dat hieu suat toi da.");
            Console.WriteLine("==========================================================================================\n");
        }
    }
}