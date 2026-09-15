using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FCanteen.Data;
using FCanteen.PosClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("FCanteenDb")!;
var kitchenHost = config["KitchenServer:Host"] ?? "127.0.0.1";
var kitchenPort = int.Parse(config["KitchenServer:Port"] ?? "9500");
var stationName = args.Length > 0 ? args[0] : "QUAY01";

const int UDP_PORT = 9501;
const string PRICE_SYNC_URL =
    "https://raw.githubusercontent.com/de180551chauvuonghoang-svg/PRN222-FCanteen/lab01/menu_prices.json";

var dbOptions = new DbContextOptionsBuilder<FCanteenContext>()
    .UseSqlServer(connectionString)
    .Options;

// YC4-A: LANG NGHE UDP BROADCAST HET HANG (chay ngam)

var soldOutIds = new HashSet<int>();

_ = Task.Run(async () =>
{
    try
    {
        var udp = new UdpClient();
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, UDP_PORT));
        udp.EnableBroadcast = true;
        while (true)
        {
            var result = await udp.ReceiveAsync();
            var msg = Encoding.UTF8.GetString(result.Buffer);
            if (msg.StartsWith("SOLD_OUT:"))
            {
                var parts = msg.Split(':');
                if (parts.Length >= 3 && int.TryParse(parts[1], out int id))
                {
                    lock (soldOutIds) { soldOutIds.Add(id); }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[!!!] Mon '{parts[2]}' da HET HANG! Vui long chon mon khac.");
                    Console.ResetColor();
                }
            }
        }
    }
    catch { }
});


// YC4-B: DONG BO BANG GIA HTTPCLIENT + URI + DNS

async Task SyncPricesAsync()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[SYNC] Bat dau dong bo bang gia...");
    Console.ResetColor();

    var uri = new Uri(PRICE_SYNC_URL);
    Console.WriteLine($"  [URI] Scheme={uri.Scheme} | Host={uri.Host} | Port={uri.Port}");
    Console.WriteLine($"  [URI] Path={uri.AbsolutePath}");

    Console.Write($"  [DNS] Phan giai {uri.Host}: ");
    try
    {
        var ips = await Dns.GetHostAddressesAsync(uri.Host);
        Console.WriteLine(string.Join(", ", ips.Select(ip => ip.ToString())));
    }
    catch { Console.WriteLine("Khong phan giai duoc"); }

    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "FCanteen-PosClient/1.0");
        var response = await http.GetAsync(PRICE_SYNC_URL);
        sw.Stop();
        int status = (int)response.StatusCode;
        Console.WriteLine($"  [HTTP] Status={status} | Time={sw.ElapsedMilliseconds}ms");

        if (response.IsSuccessStatusCode)
        {
            var jsonStr = await response.Content.ReadAsStringAsync();
            var prices = JsonSerializer.Deserialize<List<PriceItem>>(jsonStr,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            int updated = 0;
            if (prices != null)
            {
                using var db = new FCanteenContext(dbOptions);
                foreach (var p in prices)
                {
                    var item = await db.MenuItems.FindAsync(p.MenuItemId);
                    if (item != null && item.Price != p.Price)
                    {
                        Console.WriteLine($"  [CAP NHAT] {item.Name}: {item.Price:N0} -> {p.Price:N0} VND");
                        item.Price = p.Price;
                        updated++;
                    }
                }
                await db.SaveChangesAsync();

                db.DeviceLogs.Add(new FCanteen.Data.Entities.DeviceLog
                {
                    Protocol = "HTTP",
                    SourceAddress = PRICE_SYNC_URL,
                    Content = $"Sync OK | Status:{status} | {sw.ElapsedMilliseconds}ms | Updated:{updated}",
                    LoggedAt = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [SYNC] Cap nhat {updated} mon thanh cong!");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        sw.Stop();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [LOI] {ex.Message}");
        Console.ResetColor();
        using var db = new FCanteenContext(dbOptions);
        db.DeviceLogs.Add(new FCanteen.Data.Entities.DeviceLog
        {
            Protocol = "HTTP",
            SourceAddress = PRICE_SYNC_URL,
            Content = $"Sync FAILED | {sw.ElapsedMilliseconds}ms | {ex.Message}",
            LoggedAt = DateTime.Now
        });
        await db.SaveChangesAsync();
    }
}


// HIEN THI MENU

async Task<List<FCanteen.Data.Entities.MenuItem>> ShowMenuAsync()
{
    using var db = new FCanteenContext(dbOptions);
    var all = await db.MenuItems.OrderBy(m => m.MenuItemId).ToListAsync();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n  {"STT",-5} {"Ten mon",-25} {"Don gia",-12} {"Trang thai"}");
    Console.WriteLine("  " + new string('-', 60));
    Console.ResetColor();

    for (int i = 0; i < all.Count; i++)
    {
        var m = all[i];
        bool het = soldOutIds.Contains(m.MenuItemId) || !m.IsAvailable;
        if (het)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  {i + 1,-5} {m.Name,-25} {"---",-12} [HET HANG]");
            Console.ResetColor();
        }
        else
            Console.WriteLine($"  {i + 1,-5} {m.Name,-25} {m.Price,10:N0}");
    }
    Console.WriteLine("  " + new string('-', 60));
    return all;
}


// KHOI DONG: SYNC GIA TRUOC

Console.WriteLine($"[{stationName}] Khoi dong - dong bo bang gia...");
await SyncPricesAsync();
Console.Write("Nhan phim de tiep tuc... ");
Console.ReadKey();

// VONG LAP CHINH

bool running = true;
while (running)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"  === FCANTEEN | {stationName} ===  [sync=dong bo gia | thoat=thoat]");
    Console.ResetColor();

    var allItems = await ShowMenuAsync();
    var order = new OrderDto { StationName = stationName };
    decimal tamTinh = 0;
    bool orderDone = false;

    while (!orderDone)
    {
        Console.WriteLine($"\n  [Tam tinh: {tamTinh:N0} VND]");
        Console.Write("  > STT mon / 'xong' / 'sync' / 'thoat': ");
        var input = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (input == "thoat") { running = false; orderDone = true; break; }
        if (input == "xong") { orderDone = true; break; }
        if (input == "sync") { await SyncPricesAsync(); Console.ReadKey(); break; }

        if (!int.TryParse(input, out int idx) || idx < 1 || idx > allItems.Count)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!] Khong hop le!");
            Console.ResetColor();
            continue;
        }

        var sel = allItems[idx - 1];
        if (soldOutIds.Contains(sel.MenuItemId) || !sel.IsAvailable)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [!] '{sel.Name}' da HET HANG!");
            Console.ResetColor();
            continue;
        }

        Console.Write($"  > So luong [{sel.Name}]: ");
        if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [!] So luong khong hop le!");
            Console.ResetColor();
            continue;
        }

        Console.Write("  > Ghi chu: ");
        var note = Console.ReadLine() ?? "";

        order.Lines.Add(new OrderLineDto { MenuItemId = sel.MenuItemId, Quantity = qty, Note = note });
        tamTinh += sel.Price * qty;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [+] {sel.Name} x{qty} = {sel.Price * qty:N0} VND");
        Console.ResetColor();
    }

    if (!running || order.Lines.Count == 0) continue;

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  === TOM TAT ===");
    foreach (var line in order.Lines)
    {
        var it = allItems.First(m => m.MenuItemId == line.MenuItemId);
        Console.WriteLine($"  {it.Name} x{line.Quantity} = {it.Price * line.Quantity:N0} VND");
    }
    Console.WriteLine($"  Tong: {tamTinh:N0} VND");
    Console.ResetColor();

    Console.Write("  Gui len bep? (Y/n): ");
    if (Console.ReadLine()?.Trim().ToUpper() == "N") continue;

    try
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(kitchenHost, kitchenPort);
        var jdata = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));
        using var stream = tcp.GetStream();
        await stream.WriteAsync(jdata);
        var buf = new byte[4096];
        var n = await stream.ReadAsync(buf);
        var res = JsonSerializer.Deserialize<OrderConfirmDto>(
            Encoding.UTF8.GetString(buf, 0, n),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [XAC NHAN] Phieu #{res?.OrderTicketId} | Tong: {res?.TotalAmount:N0} VND");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [LOI] {ex.Message}");
        Console.ResetColor();
    }

    Console.Write("  Nhan phim de tiep tuc... ");
    Console.ReadKey();
}

Console.WriteLine("Tam biet!");

public class PriceItem
{
    public int MenuItemId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}