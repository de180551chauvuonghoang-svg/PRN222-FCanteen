using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Audit;
using FCanteen.Services.Context;
using FCanteen.Services.Discounts;
using FCanteen.Services.Formatters;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Lifetimes;
using FCanteen.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// =========================================================================
// YC1: KHOI TAO HOST BUILDER VA DANG KY DI CONTAINER
// =========================================================================
var builder = Host.CreateApplicationBuilder(args);

// 1. Doc chuoi ket noi tu appsettings.json
var connectionString = builder.Configuration.GetConnectionString("FCanteenDb")
    ?? "Server=DESKTOP-IJV2BTH\\SQLEXPRESS;Database=PRN222_Lab_FCanteen;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<FCanteenContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. Dang ky Repositories (Scoped)
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// 3. Dang ky Services (Scoped)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IAuditLogger, ConsoleAuditLogger>();

// 4. YC2: Dang ky cac Chinh sach giam gia theo Open/Closed (OCP)
// Khi muon them chinh sach moi (VD: HappyHour), chi can add them 1 dong o day!
builder.Services.AddSingleton<IDiscountPolicy, StudentDiscountPolicy>();
builder.Services.AddSingleton<IDiscountPolicy, TeacherStaffDiscountPolicy>();
builder.Services.AddSingleton<IDiscountPolicy, ComboDiscountPolicy>();
// builder.Services.AddSingleton<IDiscountPolicy, HappyHourDiscountPolicy>(); // Bat len khi giang vien hoi!

// 5. YC3: Dang ky Kenh thong bao theo Dependency Inversion (DIP)
// Doi dung 1 dong nay de chuyen kenh giua Console, File, va Email!
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
// builder.Services.AddScoped<INotificationService, FileNotificationService>();
// builder.Services.AddScoped<INotificationService, EmailNotificationService>();

// 6. YC4: Dang ky Service Lifetimes (Transient, Scoped, Singleton)
builder.Services.AddTransient<ITransientService, TransientService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddSingleton<ISingletonService, SingletonService>();

var app = builder.Build();

// =========================================================================
// MENU DIEU KHIEN CHUONG TRINH CLI (LAB 03)
// =========================================================================
while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n==========================================================================");
    Console.WriteLine("    FCANTEEN â€” LAB 03: DEPENDENCY INJECTION & LAYERED ARCHITECTURE");
    Console.WriteLine("==========================================================================");
    Console.ResetColor();
    Console.WriteLine("  1. [YC1 & YC2] Tao don hang & Ap dung Chinh sach giam gia Open/Closed (OCP)");
    Console.WriteLine("  2. [YC3] Kiem tra Doi kenh thong bao Dependency Inversion (Console/File/Email)");
    Console.WriteLine("  3. [YC4] Kiem tra Vong doi Service (Transient vs Scoped vs Singleton)");
    Console.WriteLine("  4. [YC4] Thu nghiem Co tinh gay loi Captive Dependency & Cach khac phuc");
    Console.WriteLine("  5. [YC5] Kiem tra 4 kieu Injection trong OrderService & Ambient Context");
    Console.WriteLine("  0. Thoat");
    Console.WriteLine("==========================================================================");
    Console.Write("Chon chuc nang (0-5): ");

    var choice = Console.ReadLine()?.Trim();
    if (choice == "0") break;

    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    switch (choice)
    {
        case "1":
            await RunOrderWithDiscountsAsync(sp);
            break;
        case "2":
            await RunNotificationTestAsync(sp);
            break;
        case "3":
            RunLifetimesTest(app.Services);
            break;
        case "4":
            RunCaptiveDependencyDemo(connectionString);
            break;
        case "5":
            await RunFourInjectionsDemoAsync(sp);
            break;
        default:
            Console.WriteLine("[!] Lua chon khong hop le.");
            break;
    }
}

// -------------------------------------------------------------------------
// [YC1 & YC2] TAO DON HANG VA AP DUNG GIAM GIA OPEN/CLOSED (OCP)
// -------------------------------------------------------------------------
async Task RunOrderWithDiscountsAsync(IServiceProvider sp)
{
    Console.WriteLine("\n--- [YC1 & YC2] CHECKOUT & AP DUNG CHINH SACH GIAM GIA ---");
    var staffRepo = sp.GetRequiredService<IStaffRepository>();
    var orderService = sp.GetRequiredService<IOrderService>();
    var auditLogger = sp.GetRequiredService<IAuditLogger>();

    // Set Property Injection cho logger
    orderService.AuditLogger = auditLogger;

    // Chon nhan vien thu ngan truc ca (Ambient Context)
    var staffList = await staffRepo.GetAllAsync();
    var staff = staffList.Count > 0 ? staffList[0] : new Staff { StaffCode = "NV01", FullName = "Nguyen Van An", Role = "Cashier", BranchCode = "CS1" };
    StaffContext.Current = staff;
    Console.WriteLine($"[*] Nhan vien dang truc (Ambient Context): {staff.FullName} ({staff.Role}) tai {staff.BranchCode}");

    // Dat combo: Com ga (1) + Nuoc cam ep (11)
    Console.WriteLine("[*] Khach dat: 1 Com ga (35k) + 1 Nuoc cam ep (20k) => Tong goc: 55.000 VND");
    var items = new List<(int, int)> { (1, 1), (11, 1) };

    var ticket = await orderService.CheckoutAsync("QUAY_01", items);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[THANH CONG] Don hang #{ticket.OrderTicketId} da hoan tat!");
    Console.WriteLine($"  - Tong tien sau tat ca cac giam gia: {ticket.TotalAmount:N0} VND");
    Console.ResetColor();
}

// -------------------------------------------------------------------------
// [YC3] KIEM TRA KENH THONG BAO (DIP)
// -------------------------------------------------------------------------
async Task RunNotificationTestAsync(IServiceProvider sp)
{
    Console.WriteLine("\n--- [YC3] KIEM TRA KENH THONG BAO DEPENDENCY INVERSION ---");
    var notif = sp.GetRequiredService<INotificationService>();
    Console.WriteLine($"[*] Service dang duoc inject trong container: {notif.GetType().Name}");
    await notif.SendNotificationAsync("Kiem tra he thong", "Day la tin nhan test qua interface INotificationService!");

    Console.WriteLine("\n[HUONG DAN NGHIEM THU]:");
    Console.WriteLine("  De doi sang FileNotificationService hoac EmailNotificationService,");
    Console.WriteLine("  ban chi can mo Program.cs va sua dong 'builder.Services.AddScoped<INotificationService, ...>'");
}

// -------------------------------------------------------------------------
// [YC4] KIEM TRA VONG DOI SERVICE (TRANSIENT, SCOPED, SINGLETON)
// -------------------------------------------------------------------------
void RunLifetimesTest(IServiceProvider rootSp)
{
    Console.WriteLine("\n--- [YC4] KIEM TRA SERVICE LIFETIMES QUA 2 SCOPE ---");

    Console.WriteLine("\n[SCOPE 1]:");
    using (var scope1 = rootSp.CreateScope())
    {
        var sp1 = scope1.ServiceProvider;
        var t1_a = sp1.GetRequiredService<ITransientService>();
        var t1_b = sp1.GetRequiredService<ITransientService>();
        var s1_a = sp1.GetRequiredService<IScopedService>();
        var s1_b = sp1.GetRequiredService<IScopedService>();
        var g1_a = sp1.GetRequiredService<ISingletonService>();
        var g1_b = sp1.GetRequiredService<ISingletonService>();

        Console.WriteLine($"  Transient Lan 1 : {t1_a.InstanceId}");
        Console.WriteLine($"  Transient Lan 2 : {t1_b.InstanceId}  (==> Khac nhau vi Transient luon tao moi)");
        Console.WriteLine($"  Scoped    Lan 1 : {s1_a.InstanceId}");
        Console.WriteLine($"  Scoped    Lan 2 : {s1_b.InstanceId}  (==> Giong nhau vi cung 1 Scope)");
        Console.WriteLine($"  Singleton Lan 1 : {g1_a.InstanceId}");
        Console.WriteLine($"  Singleton Lan 2 : {g1_b.InstanceId}  (==> Giong nhau vi Singleton toan cuc)");
    }

    Console.WriteLine("\n[SCOPE 2]:");
    using (var scope2 = rootSp.CreateScope())
    {
        var sp2 = scope2.ServiceProvider;
        var t2_a = sp2.GetRequiredService<ITransientService>();
        var s2_a = sp2.GetRequiredService<IScopedService>();
        var g2_a = sp2.GetRequiredService<ISingletonService>();

        Console.WriteLine($"  Transient Lan 1 : {t2_a.InstanceId}  (==> Tao moi hoan toan)");
        Console.WriteLine($"  Scoped    Lan 1 : {s2_a.InstanceId}  (==> DOI ID MOI vi da sang Scope 2)");
        Console.WriteLine($"  Singleton Lan 1 : {g2_a.InstanceId}  (==> GIU NGUYEN ID cu tu Scope 1)");
    }
}

// -------------------------------------------------------------------------
// [YC4] CO TINH GAY LOI CAPTIVE DEPENDENCY
// -------------------------------------------------------------------------
void RunCaptiveDependencyDemo(string connStr)
{
    Console.WriteLine("\n--- [YC4] CO TINH GAY LOI CAPTIVE DEPENDENCY ---");
    Console.WriteLine("[*] Kich ban: Dang ky CaptiveServiceDemo la SINGLETON, nhung no chua DbContext (SCOPED)!");

    try
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddDbContext<FCanteenContext>(opt => opt.UseSqlServer(connStr));

        // Lá»–I CAPTIVE DEPENDENCY: Singleton phu thuoc Scoped
        builder.Services.AddSingleton<CaptiveServiceDemo>();

        // Bat tinh nang ValidateScopes cua .NET
        var host = Host.CreateDefaultBuilder()
            .UseDefaultServiceProvider(o =>
            {
                o.ValidateScopes = true;
                o.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.AddDbContext<FCanteenContext>(opt => opt.UseSqlServer(connStr));
                services.AddSingleton<CaptiveServiceDemo>();
            })
            .Build();

        Console.WriteLine("[!] Khong phat hien duoc loi (Co the ValidateOnBuild chua bat).");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[BAT DUOC NGOAI LE DUNG KIEU!]:\n  {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"  Chi tiet: {ex.InnerException.Message}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[CACH KHAC PHUC]:");
        Console.WriteLine("  1. Chuyen CaptiveServiceDemo thanh Scoped.");
        Console.WriteLine("  2. Hoac inject IServiceScopeFactory vao Singleton de tao scope cuc bo khi can dung DbContext.");
        Console.ResetColor();
    }
}

// -------------------------------------------------------------------------
// [YC5] KIEM TRA 4 KIEU INJECTION TRONG OrderService
// -------------------------------------------------------------------------
async Task RunFourInjectionsDemoAsync(IServiceProvider sp)
{
    Console.WriteLine("\n--- [YC5] KIEM TRA 4 KIEU INJECTION TRONG OrderService ---");

    var orderService = sp.GetRequiredService<IOrderService>();

    // 1. Constructor Injection: Da duoc DI container tu dong inject (Repos, Discounts, Notification)
    Console.WriteLine("[1. Constructor Injection]: Da tu dong inject IOrderRepository, IMenuItemRepository, IEnumerable<IDiscountPolicy>, INotificationService.");

    // 2. Property Injection: Inject logger tuy chon
    var auditLogger = sp.GetRequiredService<IAuditLogger>();
    orderService.AuditLogger = auditLogger;
    Console.WriteLine("[2. Property Injection]: Da gan orderService.AuditLogger = ConsoleAuditLogger.");

    // 4. Ambient Context: Thiet lap nhan vien truc bang AsyncLocal
    StaffContext.Current = new Staff { StaffCode = "NV99", FullName = "Le Thi Demo", Role = "Manager", BranchCode = "CS3" };
    Console.WriteLine($"[4. Ambient Context]: StaffContext.Current = {StaffContext.Current.FullName} ({StaffContext.Current.Role})");

    // Tao don hang thu nghiem
    var ticket = await orderService.CheckoutAsync("QUAY_VIP", new List<(int, int)> { (2, 2), (10, 1) });

    // 3. Method Injection: Truyen formatter vao ham ExportReceipt
    Console.WriteLine("\n[3. Method Injection]: Truyen PlainTextReceiptFormatter vao phuong thuc ExportReceipt:");
    orderService.ExportReceipt(ticket, new PlainTextReceiptFormatter());

    Console.WriteLine("\n[3. Method Injection]: Truyen JsonReceiptFormatter vao phuong thuc ExportReceipt:");
    orderService.ExportReceipt(ticket, new JsonReceiptFormatter());
}