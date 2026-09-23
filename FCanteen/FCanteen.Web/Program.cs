using System;
using FCanteen.Data;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Audit;
using FCanteen.Services.Discounts;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Notifications;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
var connectionString = builder.Configuration.GetConnectionString("FCanteenDb")
    ?? "Server=DESKTOP-IJV2BTH\\SQLEXPRESS;Database=PRN222_Lab_FCanteen;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<FCanteenContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. Repositories (Scoped)
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// 3. Services & Lab 03 DI dependencies
builder.Services.AddSingleton<IAuditLogger, ConsoleAuditLogger>();
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
builder.Services.AddSingleton<IDiscountPolicy, StudentDiscountPolicy>();
builder.Services.AddSingleton<IDiscountPolicy, TeacherStaffDiscountPolicy>();
builder.Services.AddSingleton<IDiscountPolicy, ComboDiscountPolicy>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 4. MVC, Session & HttpContextAccessor
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session Middleware before Authorization & Controller execution
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MenuItems}/{action=Index}/{id?}");

app.Run();
