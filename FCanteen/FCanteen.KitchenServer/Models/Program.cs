using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.KitchenServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ============================================================
// CAU HINH
// ============================================================
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("FCanteenDb")!;

DbContextOptions<FCanteenContext> BuildDbOptions() =>
    new DbContextOptionsBuilder<FCanteenContext>()
        .UseSqlServer(connectionString)
        .Options;

// ============================================================
// HANG SO
// ============================================================
const int TCP_PORT = 9500;
const int UDP_PORT = 9501;

// ============================================================
// DANH SACH PHIEU DANG CHO
// ============================================================
var pendingTickets = new List<string>();
var lockObj = new object();

void PrintPending()
{
    lock (lockObj)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("     KITCHEN SERVER -- CONG 9500");
        Console.WriteLine("  [Go 'soldout <ID>' de danh dau het hang]");
        Console.WriteLine("============================================");
        Console.ResetColor();
        if (pendingTickets.Count == 0)
            Console.WriteLine("  [Chua co phieu nao dang cho]");
        else
            foreach (var t in pendingTickets)
                Console.WriteLine("  >> " + t);
        Console.WriteLine("============================================");
    }
}

// ============================================================
// YC4-A: PHAT UDP BROADCAST KHI MON HET HANG
// ============================================================
async Task BroadcastSoldOutAsync(int menuItemId, string itemName)
{
    using var udpClient = new UdpClient();
    udpClient.EnableBroadcast = true;

    var message = $"SOLD_OUT:{menuItemId}:{itemName}";
    var data = Encoding.UTF8.GetBytes(message);
    var broadcastEp = new IPEndPoint(IPAddress.Broadcast, UDP_PORT);
    await udpClient.SendAsync(data, data.Length, broadcastEp);

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[UDP] Broadcast: Mon '{itemName}' (ID={menuItemId}) HET HANG!");
    Console.ResetColor();

    // Ghi DeviceLog
    using var db = new FCanteenContext(BuildDbOptions());
    db.DeviceLogs.Add(new DeviceLog
    {
        Protocol = "UDP",
        SourceAddress = $"BROADCAST:{UDP_PORT}",
        Content = $"Sold out broadcast: {itemName} (ID={menuItemId})",
        LoggedAt = DateTime.Now
    });
    await db.SaveChangesAsync();
}

// ============================================================
// XU LY MOI KET NOI QUAY (chay tren Task rieng)
// ============================================================
async Task HandleClientAsync(TcpClient client)
{
    var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
    Console.WriteLine($"\n[+] Ket noi tu: {endpoint}");

    using var db = new FCanteenContext(BuildDbOptions());

    // Ghi DeviceLog: ket noi VAO
    db.DeviceLogs.Add(new DeviceLog
    {
        Protocol = "TCP",
        SourceAddress = endpoint,
        Content = "Client connected",
        LoggedAt = DateTime.Now
    });
    await db.SaveChangesAsync();

    try
    {
        using var stream = client.GetStream();
        var buffer = new byte[8192];
        var bytesRead = await stream.ReadAsync(buffer);
        var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Console.WriteLine($"[<] Nhan du lieu tu {endpoint}");

        var order = JsonSerializer.Deserialize<OrderDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order == null || order.Lines.Count == 0)
        {
            var errConfirm = new OrderConfirmDto { OrderTicketId = 0, TotalAmount = 0, Message = "LOI: Du lieu khong hop le" };
            var errBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errConfirm));
            await stream.WriteAsync(errBytes);
            return;
        }

        // ---- LUU VAO DB TRONG TRANSACTION ----
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            decimal totalAmount = 0;
            var ticketLines = new List<TicketLine>();

            foreach (var line in order.Lines)
            {
                var menuItem = await db.MenuItems.FindAsync(line.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable) continue;

                // Server TU TINH lai tong tien
                totalAmount += menuItem.Price * line.Quantity;
                ticketLines.Add(new TicketLine
                {
                    MenuItemId = line.MenuItemId,
                    Quantity = line.Quantity,
                    UnitPrice = menuItem.Price, // gia tai thoi diem ban
                    Note = line.Note
                });
            }

            var ticket = new OrderTicket
            {
                StationName = order.StationName,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                Status = "Pending",
                TicketLines = ticketLines
            };

            db.OrderTickets.Add(ticket);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            lock (lockObj)
            {
                pendingTickets.Add($"Phieu #{ticket.OrderTicketId} | {ticket.StationName} | {totalAmount:N0} VND");
            }
            PrintPending();

            // Gui xac nhan ve quay
            var confirm = new OrderConfirmDto
            {
                OrderTicketId = ticket.OrderTicketId,
                TotalAmount = totalAmount,
                Message = $"Phieu #{ticket.OrderTicketId} tiep nhan. Tong: {totalAmount:N0} VND"
            };
            var confirmBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(confirm));
            await stream.WriteAsync(confirmBytes);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] Luu phieu #{ticket.OrderTicketId} | {order.StationName} | {totalAmount:N0} VND");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[LOI] Transaction: {ex.Message}");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[LOI] Client: {ex.Message}");
    }
    finally
    {
        // Ghi DeviceLog: ket noi RA
        db.DeviceLogs.Add(new DeviceLog
        {
            Protocol = "TCP",
            SourceAddress = endpoint,
            Content = "Client disconnected",
            LoggedAt = DateTime.Now
        });
        await db.SaveChangesAsync();
        client.Close();
    }
}

// ============================================================
// KHOI DONG SERVER
// ============================================================
var listener = new TcpListener(IPAddress.Any, TCP_PORT);
listener.Start();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"[SERVER] Kitchen Server lang nghe TCP:{TCP_PORT} | UDP broadcast: {UDP_PORT}");
Console.ResetColor();
PrintPending();

// Chap nhan ket noi TCP song song
_ = Task.Run(async () =>
{
    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleClientAsync(client));
    }
});

// ============================================================
// GIAO DIEN CONSOLE: lenh 'soldout <ID>'
// ============================================================
Console.WriteLine("\n[GO LENH] Danh sach lenh:");
Console.WriteLine("  soldout <MenuItemId>  - Danh dau mon het hang va broadcast UDP");
Console.WriteLine("  list                  - Xem danh sach phieu dang cho");
Console.WriteLine("  exit                  - Thoat\n");

while (true)
{
    Console.Write("Server> ");
    var cmd = Console.ReadLine()?.Trim().ToLower() ?? "";

    if (cmd == "exit") break;

    if (cmd == "list")
    {
        PrintPending();
        continue;
    }

    if (cmd.StartsWith("soldout "))
    {
        var parts = cmd.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int itemId))
        {
            using var db = new FCanteenContext(BuildDbOptions());
            var item = await db.MenuItems.FindAsync(itemId);
            if (item == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Khong tim thay mon ID={itemId}");
                Console.ResetColor();
            }
            else
            {
                item.IsAvailable = false;
                await db.SaveChangesAsync();
                await BroadcastSoldOutAsync(item.MenuItemId, item.Name);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[OK] Da danh dau '{item.Name}' het hang va broadcast UDP!");
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("[!] Cu phap: soldout <MenuItemId>  Vi du: soldout 3");
        }
        continue;
    }

    Console.WriteLine("[?] Lenh khong hop le. Cac lenh: soldout <ID> | list | exit");
}