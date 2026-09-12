using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.KitchenServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// cấu hình
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("FCanteenDb")!;

DbContextOptions<FCanteenContext> BuildDbOptions() =>
    new DbContextOptionsBuilder<FCanteenContext>()
        .UseSqlServer(connectionString)
        .Options;

// danh sách phiếu chờ 
var pendingTickets = new List<string>();
var lockObj = new object();

void PrintPending()
{
    lock (lockObj)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("     KITCHEN SERVER — CONG 9500");
        Console.WriteLine("============================================");
        Console.ResetColor();
        if (pendingTickets.Count == 0)
        {
            Console.WriteLine("  [Chua co phieu nao dang cho]");
        }
        else
        {
            foreach (var t in pendingTickets)
                Console.WriteLine("  >> " + t);
        }
        Console.WriteLine("============================================");
    }
}

// xử lý kết nối quầy (task được xử lý riêng biệt)
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
        Content = $"Client connected",
        LoggedAt = DateTime.Now
    });
    await db.SaveChangesAsync();

    try
    {
        using var stream = client.GetStream();
        var buffer = new byte[8192];
        var bytesRead = await stream.ReadAsync(buffer);
        var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Console.WriteLine($"[<] Nhan du lieu: {json}");

        // Giai ma JSON
        var order = JsonSerializer.Deserialize<OrderDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order == null || order.Lines.Count == 0)
        {
            await SendResponseAsync(stream, new OrderConfirmDto
            {
                OrderTicketId = 0,
                TotalAmount = 0,
                Message = "LOI: Du lieu phieu khong hop le"
            });
            return;
        }


        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Server TỰ TÍNH lại tổng tiền — không tin client
            decimal totalAmount = 0;
            var ticketLines = new List<TicketLine>();

            foreach (var line in order.Lines)
            {
                var menuItem = await db.MenuItems.FindAsync(line.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable) continue;

                var unitPrice = menuItem.Price; // giá hiện tại từ DB
                totalAmount += unitPrice * line.Quantity;

                ticketLines.Add(new TicketLine
                {
                    MenuItemId = line.MenuItemId,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice,  // lưu giá tại thời điểm bán
                    Note = line.Note
                });
            }

            // Tạo OrderTicket
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

            // Thêm vào danh sách chờ
            lock (lockObj)
            {
                pendingTickets.Add($"Phieu #{ticket.OrderTicketId} | {ticket.StationName} | {totalAmount:N0} VND");
            }
            PrintPending();

            // Gửi xác nhận về quầy
            var confirm = new OrderConfirmDto
            {
                OrderTicketId = ticket.OrderTicketId,
                TotalAmount = totalAmount,
                Message = $"Phieu #{ticket.OrderTicketId} da duoc tiep nhan. Tong: {totalAmount:N0} VND"
            };
            await SendResponseAsync(stream, confirm);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] Luu phieu #{ticket.OrderTicketId} thanh cong. Tong: {totalAmount:N0} VND");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[LOI] Transaction that bai: {ex.Message}");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[LOI] Xu ly client: {ex.Message}");
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
        Console.WriteLine($"[-] Dong ket noi: {endpoint}");
    }
}

// Helper: gửi JSON response
async Task SendResponseAsync(NetworkStream stream, OrderConfirmDto confirm)
{
    var json = JsonSerializer.Serialize(confirm);
    var bytes = Encoding.UTF8.GetBytes(json);
    await stream.WriteAsync(bytes);
    Console.WriteLine($"[>] Gui xac nhan: {json}");
}


const int PORT = 9500;
var listener = new TcpListener(IPAddress.Any, PORT);
listener.Start();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"[SERVER] Kitchen Server dang lang nghe tren cong {PORT}...");
Console.ResetColor();
PrintPending();

// Chấp nhận kết nối liên tục, mỗi client một Task riêng
while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    // Không await — chạy song song
    _ = Task.Run(() => HandleClientAsync(client));
}
