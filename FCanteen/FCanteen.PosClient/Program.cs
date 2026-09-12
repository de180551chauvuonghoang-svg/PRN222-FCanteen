using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.PosClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ============================================================
// CẤU HÌNH
// ============================================================
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("FCanteenDb")!;
var kitchenHost = config["KitchenServer:Host"] ?? "127.0.0.1";
var kitchenPort = int.Parse(config["KitchenServer:Port"] ?? "9500");

// Lấy tên quầy từ tham số dòng lệnh: dotnet run -- QUAY01
var stationName = args.Length > 0 ? args[0] : "QUAY01";

var dbOptions = new DbContextOptionsBuilder<FCanteenContext>()
    .UseSqlServer(connectionString)
    .Options;

// ============================================================
// HIỂN THỊ TIÊU ĐỀ
// ============================================================
void PrintHeader()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("╔══════════════════════════════════════════╗");
    Console.WriteLine($"║     FCANTEEN — QUAY THU NGAN             ║");
    Console.WriteLine($"║     Ten quay: {stationName,-28}║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.ResetColor();
}

// ============================================================
// HIỂN THỊ MENU TỪ DATABASE
// ============================================================
async Task<List<FCanteen.Data.Entities.MenuItem>> LoadAndDisplayMenuAsync()
{
    using var db = new FCanteenContext(dbOptions);
    var items = await db.MenuItems
        .Where(m => m.IsAvailable)
        .OrderBy(m => m.MenuItemId)
        .ToListAsync();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n  THUC DON");
    Console.WriteLine($"  {"STT",-5} {"Ten mon",-25} {"Don gia",-12} {"Don vi"}");
    Console.WriteLine("  " + new string('-', 55));
    Console.ResetColor();

    for (int i = 0; i < items.Count; i++)
    {
        var item = items[i];
        Console.WriteLine($"  {i + 1,-5} {item.Name,-25} {item.Price,10:N0}  {item.Unit}");
    }
    Console.WriteLine("  " + new string('-', 55));
    return items;
}

// ============================================================
// VÒNG LẶP CHÍNH
// ============================================================
while (true)
{
    PrintHeader();
    var menuItems = await LoadAndDisplayMenuAsync();

    var order = new OrderDto { StationName = stationName };
    decimal tamTinh = 0;

    Console.WriteLine("\n  Nhap order (go 'xong' de gui, 'thoat' de thoat):");

    while (true)
    {
        Console.WriteLine($"\n  [Tam tinh: {tamTinh:N0} VND]");
        Console.Write("  > So thu tu mon (hoac 'xong'/'thoat'): ");
        var input = Console.ReadLine()?.Trim().ToLower();

        if (input == "thoat") goto END;
        if (input == "xong") break;

        if (!int.TryParse(input, out int idx) || idx < 1 || idx > menuItems.Count)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!] So thu tu khong hop le!");
            Console.ResetColor();
            continue;
        }

        var selected = menuItems[idx - 1];
        Console.Write($"  > So luong [{selected.Name}]: ");
        if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!] So luong khong hop le!");
            Console.ResetColor();
            continue;
        }

        Console.Write($"  > Ghi chu (Enter de bo qua): ");
        var note = Console.ReadLine() ?? "";

        order.Lines.Add(new OrderLineDto
        {
            MenuItemId = selected.MenuItemId,
            Quantity = qty,
            Note = note
        });

        tamTinh += selected.Price * qty;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [+] Da them: {selected.Name} x{qty} = {selected.Price * qty:N0} VND");
        Console.ResetColor();
    }

    if (order.Lines.Count == 0)
    {
        Console.WriteLine("  [!] Chua co mon nao. Thu lai.");
        Console.ReadKey();
        continue;
    }

    // --------------------------------------------------------
    // HIỂN THỊ TÓM TẮT TRƯỚC KHI GỬI
    // --------------------------------------------------------
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  ===== TOM TAT PHIEU ORDER =====");
    foreach (var line in order.Lines)
    {
        var item = menuItems.First(m => m.MenuItemId == line.MenuItemId);
        Console.WriteLine($"  - {item.Name,-25} x{line.Quantity}  {item.Price * line.Quantity,10:N0} VND");
        if (!string.IsNullOrEmpty(line.Note))
            Console.WriteLine($"    Ghi chu: {line.Note}");
    }
    Console.WriteLine($"  {"TAM TINH:",-30} {tamTinh,10:N0} VND");
    Console.WriteLine("  ================================");
    Console.ResetColor();

    Console.Write("  Xac nhan gui len bep? (Y/n): ");
    var confirm = Console.ReadLine()?.Trim().ToUpper();
    if (confirm == "N") continue;

    // --------------------------------------------------------
    // GỬI PHIẾU LÊN KITCHENSERVER QUA TCP
    // --------------------------------------------------------
    try
    {
        Console.Write("\n  Dang gui len bep...");
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(kitchenHost, kitchenPort);

        var json = JsonSerializer.Serialize(order);
        var data = Encoding.UTF8.GetBytes(json);

        using var stream = tcp.GetStream();
        await stream.WriteAsync(data);

        // Nhận xác nhận từ server
        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer);
        var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        var response = JsonSerializer.Deserialize<OrderConfirmDto>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  ===== XAC NHAN TU BEP =====");
        Console.WriteLine($"  Ma phieu : #{response?.OrderTicketId}");
        Console.WriteLine($"  Tong tien: {response?.TotalAmount:N0} VND");
        Console.WriteLine($"  Thong bao: {response?.Message}");
        Console.WriteLine("  ============================");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  [LOI] Khong the ket noi den bep: {ex.Message}");
        Console.ResetColor();
    }

    Console.Write("\n  Nhan phim bat ky de tiep tuc...");
    Console.ReadKey();
}

END:
Console.WriteLine("\n  Tam biet!");
