using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data.Services
{
    public class DataSeeder
    {
        public static async Task SeedLargeDataAsync(FCanteenContext context)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n========================================================");
            Console.WriteLine("    YC1: SINH DU LIEU LON (50.000 TICKETS & 200.000 LINES)");
            Console.WriteLine("========================================================");
            Console.ResetColor();

            // Kiem tra du lieu da ton tai chua
            int currentTicketCount = await context.OrderTickets.CountAsync();
            if (currentTicketCount >= 50000)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] Da co {currentTicketCount:N0} OrderTickets trong CSDL. Bo qua buoc seed.");
                Console.ResetColor();
                return;
            }

            var menuItems = await context.MenuItems.ToListAsync();
            if (menuItems.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Khong tim thay MenuItem nao. Vui long chay Migration seed MenuItem truoc.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("[*] Bat dau sinh 50.000 OrderTickets va ~200.000 TicketLines...");
            Console.WriteLine("[*] Cau hinh: BatchSize = 2.000, AutoDetectChangesEnabled = false");

            var sw = Stopwatch.StartNew();

            // Bat buoc theo yeu cau: Tat AutoDetectChangesEnabled de toi uu toc do
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            int targetTickets = 50000;
            int batchSize = 2000;
            var branches = new[] { "CS1", "CS2", "CS3" };
            var stations = new[] { "QUAY01", "QUAY02", "QUAY03" };
            var random = new Random(42); // Seed co dinh de du lieu thong nhat
            DateTime now = DateTime.Now;

            int totalTicketsInserted = 0;
            int totalLinesInserted = 0;

            for (int b = 0; b < targetTickets; b += batchSize)
            {
                int currentBatch = Math.Min(batchSize, targetTickets - b);
                var tickets = new List<OrderTicket>(currentBatch);

                for (int i = 0; i < currentBatch; i++)
                {
                    // 1. Phan bo 6 thang gan nhat (0 den 180 ngay truoc)
                    int daysAgo = random.Next(0, 180);
                    DateTime ticketDate = now.AddDays(-daysAgo).Date;

                    // 2. Trong so gio cao diem 11h - 13h (chiem 65%)
                    int hour;
                    int peakProb = random.Next(100);
                    if (peakProb < 65)
                    {
                        hour = random.Next(11, 14); // 11h, 12h, 13h
                    }
                    else if (peakProb < 85)
                    {
                        // Gio sang hoac toi
                        hour = random.Next(0, 2) == 0 ? random.Next(7, 10) : random.Next(17, 20);
                    }
                    else
                    {
                        // Cac gio con lai trong ngay
                        hour = random.Next(6, 21);
                    }

                    int minute = random.Next(0, 60);
                    int second = random.Next(0, 60);
                    DateTime createdAt = ticketDate.AddHours(hour).AddMinutes(minute).AddSeconds(second);

                    string branch = branches[random.Next(branches.Length)];
                    string station = stations[random.Next(stations.Length)];

                    // 3. Moi ticket co tu 3 den 5 mon de dat tong >= 200.000 lines
                    int lineCount = random.Next(3, 6);
                    decimal totalAmount = 0;
                    var lines = new List<TicketLine>(lineCount);

                    // Chon ngau nhien cac mon khong trung trong 1 ticket
                    var pickedItems = menuItems.OrderBy(_ => random.Next()).Take(lineCount).ToList();

                    foreach (var m in pickedItems)
                    {
                        int qty = random.Next(1, 4); // So luong 1 - 3
                        decimal unitPrice = m.Price; // Gia tai thoi diem ban
                        totalAmount += qty * unitPrice;

                        lines.Add(new TicketLine
                        {
                            MenuItemId = m.MenuItemId,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            Note = ""
                        });
                        totalLinesInserted++;
                    }

                    tickets.Add(new OrderTicket
                    {
                        BranchCode = branch,
                        StationName = station,
                        TotalAmount = totalAmount,
                        CreatedAt = createdAt,
                        Status = "Completed",
                        TicketLines = lines
                    });

                    totalTicketsInserted++;
                }

                // Ghi xuong CSDL theo lo (Batch)
                await context.OrderTickets.AddRangeAsync(tickets);
                await context.SaveChangesAsync();

                // Giai phong ChangeTracker tranh tran RAM
                context.ChangeTracker.Clear();

                Console.WriteLine($"[+] Tien do: {totalTicketsInserted:N0} / {targetTickets:N0} tickets | {totalLinesInserted:N0} lines ({(totalTicketsInserted * 100.0 / targetTickets):F1}%) - {sw.ElapsedMilliseconds:N0} ms");
            }

            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[THANH CONG] Da seed {totalTicketsInserted:N0} OrderTickets va {totalLinesInserted:N0} TicketLines.");
            Console.WriteLine($"[THOI GIAN] Tong thoi gian sinh: {sw.Elapsed.TotalSeconds:F2} giay ({sw.ElapsedMilliseconds:N0} ms).");
            Console.ResetColor();
        }
    }
}