using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Services
{
    public class InventoryRaceConditionService
    {
        private readonly FCanteenContext _context;
        private readonly object _lockObj = new object();

        public InventoryRaceConditionService(FCanteenContext context)
        {
            _context = context;
        }

        public async Task RunSimulationAsync()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("    YC5: MO PHONG RACE CONDITION KHI TRU TON KHO NGUYEN LIEU");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            // 1. Chon nguyen lieu mo phong tu Database hoac khoi tao
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync();
            string ingredientName = ingredient != null ? ingredient.Name : "Thit Ga (kg)";

            const int initialStock = 20000;    // Ton kho ban dau: 20.000 don vi
            const int totalOrders = 15000;     // 15.000 don hang dong thoi
            const int deductPerOrder = 1;      // Moi don tru 1 don vi
            int expectedStock = initialStock - (totalOrders * deductPerOrder); // 5.000

            Console.WriteLine($"[*] Nguyen lieu mo phong: {ingredientName}");
            Console.WriteLine($"[*] Ton kho ban dau    : {initialStock:N0}");
            Console.WriteLine($"[*] So don hang dong thoi: {totalOrders:N0} don (Moi don tru {deductPerOrder} don vi)");
            Console.WriteLine($"[*] Ton kho ky vong dung : {expectedStock:N0}\n");

            var sw = new Stopwatch();

            // =====================================================================
            // THI NGHIEM 1: PHIEN BAN KHONG DONG BO (UNSAFE - NO SYNCHRONIZATION)
            // =====================================================================
            Console.Write("[1/3] Dang chay phien ban: KHONG DONG BO (Unsafe - Gay Race Condition)... ");
            int unsafeStock = initialStock;
            sw.Restart();
            Parallel.For(0, totalOrders, i =>
            {
                // Lá»–I: Thao tac tru nay khong phai Atomic (Read -> Modify -> Write)
                // Cac luong tranh chap gay ra loi Lost Update
                unsafeStock -= deductPerOrder;
            });
            sw.Stop();
            long unsafeMs = sw.ElapsedMilliseconds;
            int unsafeLostUpdates = unsafeStock - expectedStock;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Xong! ({unsafeMs} ms)");
            Console.WriteLine($"      ==> Ton kho thuc te: {unsafeStock:N0} (SAI LECH: mat {unsafeLostUpdates:N0} lan tru!)");
            Console.ResetColor();

            // =====================================================================
            // THI NGHIEM 2: PHIEN BAN DONG BO BANG Interlocked (ATOMIC INSTRUCTION)
            // =====================================================================
            Console.Write("[2/3] Dang chay phien ban: CO DONG BO BANG Interlocked (Thread-safe)... ");
            int interlockedStock = initialStock;
            sw.Restart();
            Parallel.For(0, totalOrders, i =>
            {
                // CHUAN: Su dung CPU Atomic Instruction (Hardware-level lock-free)
                Interlocked.Add(ref interlockedStock, -deductPerOrder);
            });
            sw.Stop();
            long interlockedMs = sw.ElapsedMilliseconds;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! ({interlockedMs} ms)");
            Console.WriteLine($"      ==> Ton kho thuc te: {interlockedStock:N0} (CHINH XAC 100%)");
            Console.ResetColor();

            // =====================================================================
            // THI NGHIEM 3: PHIEN BAN DONG BO BANG lock (CRITICAL SECTION)
            // =====================================================================
            Console.Write("[3/3] Dang chay phien ban: CO DONG BO BANG lock statement... ");
            int lockedStock = initialStock;
            sw.Restart();
            Parallel.For(0, totalOrders, i =>
            {
                // CHUAN: Chi duy nhat 1 luong duoc phep vao Critical Section tai 1 thoi diem
                lock (_lockObj)
                {
                    lockedStock -= deductPerOrder;
                }
            });
            sw.Stop();
            long lockMs = sw.ElapsedMilliseconds;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Xong! ({lockMs} ms)");
            Console.WriteLine($"      ==> Ton kho thuc te: {lockedStock:N0} (CHINH XAC 100%)");
            Console.ResetColor();

            // In bang ket qua so sanh
            PrintComparisonTable(initialStock, totalOrders, expectedStock,
                unsafeStock, unsafeLostUpdates, unsafeMs,
                interlockedStock, interlockedMs,
                lockedStock, lockMs);

            // In phan tich nguyen nhan cho bao cao Word
            PrintReportExplanation();
        }

        private void PrintComparisonTable(int initial, int orders, int expected,
            int unsafeStock, int lost, long unsafeMs,
            int interlockedStock, long interlockedMs,
            int lockedStock, long lockMs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n==========================================================================================");
            Console.WriteLine("        BANG KET QUA SO SANH RACE CONDITION (DUNG CHUP ANH DUA VAO BAO CAO WORD)");
            Console.WriteLine("==========================================================================================");
            Console.ResetColor();

            Console.WriteLine($"Ton kho ban dau : {initial:N0}");
            Console.WriteLine($"So order tru kho: {orders:N0}");
            Console.WriteLine($"Ton kho ky vong : {expected:N0}\n");

            Console.WriteLine("| Phien ban giai thuat           | Ton kho cuoi cung | Thoi gian (ms) | Chenh lech (Loi) | Ket luan |");
            Console.WriteLine("| :----------------------------- | :---------------: | :------------: | :--------------: | :------: |");
            Console.WriteLine($"| 1. Khong dong bo (Unsafe)      | {unsafeStock,17:N0} | {unsafeMs,14} | {$"+{lost:N0}",16} | {"âŒ BI SAI",8} |");
            Console.WriteLine($"| 2. Dung Interlocked (Atomic)   | {interlockedStock,17:N0} | {interlockedMs,14} | {"0",16} | {"âœ… DUNG 100%",8} |");
            Console.WriteLine($"| 3. Dung lock (Monitor)         | {lockedStock,17:N0} | {lockMs,14} | {"0",16} | {"âœ… DUNG 100%",8} |");
            Console.WriteLine("==========================================================================================\n");
        }

        private void PrintReportExplanation()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("==========================================================================================");
            Console.WriteLine("      GIAI THICH BAN CHAT RACE CONDITION & GIAI PHAP DONG BO (CHO BAO CAO)");
            Console.WriteLine("==========================================================================================");
            Console.ResetColor();
            Console.WriteLine("1. Vi sao phien ban khong dong bo bi sai (Lost Update)?");
            Console.WriteLine("   - Phep tinh 'stock -= 1' thuc chat gom 3 buoc o cap do CPU:");
            Console.WriteLine("     Buoc 1: READ gia tri stock tu RAM vao thanh ghi CPU.");
            Console.WriteLine("     Buoc 2: MODIFY giam gia tri di 1 trong thanh ghi.");
            Console.WriteLine("     Buoc 3: WRITE ghi nguoc gia tri moi tu thanh ghi ve RAM.");
            Console.WriteLine("   - Khi hang nghin luong chay song song tren cac loi CPU khac nhau, nhieu luong cung doc");
            Console.WriteLine("     chung 1 gia tri cu truoc khi luong kia kip ghi lai. Ket qua la nhieu lan tru bi de len");
            Console.WriteLine("     nhau (Lost Update), dan den ton kho thuc te cao hon rat nhieu so voi so sach.");
            Console.WriteLine("\n2. So sanh giua Interlocked va lock:");
            Console.WriteLine("   - Interlocked: Su dung chi lenh nguyen tu truc tiep o cap phan cung CPU (Atomic Instruction),");
            Console.WriteLine("     khong gay block luong (Lock-free) nen toc do cuc nhanh va tieu ton it tai nguyen nhat.");
            Console.WriteLine("   - lock: Tao ra mot vung Critical Section ngan cac luong khac vao cung luc. Thich hop khi");
            Console.WriteLine("     khoi ma can bao ve phuc tap gom nhieu dong code chu khong chi la 1 bien so don le.");
            Console.WriteLine("==========================================================================================\n");
        }
    }
}