using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Data
{
    public class FCanteenContext : DbContext
    {
        public FCanteenContext(DbContextOptions<FCanteenContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();
        public DbSet<TicketLine> TicketLines => Set<TicketLine>();
        public DbSet<DeviceLog> DeviceLogs => Set<DeviceLog>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<DailySettlement> DailySettlements => Set<DailySettlement>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<DiscountPolicyLog> DiscountPolicyLogs => Set<DiscountPolicyLog>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Cau hinh toan bo thuoc tinh decimal ve chuan decimal(18, 2)
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed 15 mon an bang HasData
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { MenuItemId = 1,  Name = "Com ga",           Price = 35000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 2,  Name = "Com suon",         Price = 35000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 3,  Name = "Com tam",          Price = 30000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 4,  Name = "Bun bo",           Price = 40000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 5,  Name = "Pho bo",           Price = 45000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 6,  Name = "Mi quang",         Price = 35000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 7,  Name = "Banh mi thit",     Price = 20000, Unit = "o",    IsAvailable = true },
                new MenuItem { MenuItemId = 8,  Name = "Xoi ga",           Price = 25000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 9,  Name = "Che dau xanh",     Price = 15000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 10, Name = "Tra sua tran chau", Price = 30000, Unit = "ly",  IsAvailable = true },
                new MenuItem { MenuItemId = 11, Name = "Nuoc cam ep",      Price = 20000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 12, Name = "Sinh to bo",       Price = 25000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 13, Name = "Banh bao nhan thit", Price = 15000, Unit = "cai", IsAvailable = true },
                new MenuItem { MenuItemId = 14, Name = "Hu tieu nam vang", Price = 40000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 15, Name = "Chao ga",          Price = 30000, Unit = "to",   IsAvailable = true }
            );

            // Seed nguyen lieu
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { IngredientId = 1, Name = "Gao Thom",       Unit = "kg",   StockQuantity = 500, WarningThreshold = 50 },
                new Ingredient { IngredientId = 2, Name = "Thit Ga",        Unit = "kg",   StockQuantity = 200, WarningThreshold = 30 },
                new Ingredient { IngredientId = 3, Name = "Suon Heo",       Unit = "kg",   StockQuantity = 150, WarningThreshold = 25 },
                new Ingredient { IngredientId = 4, Name = "Thit Bo",        Unit = "kg",   StockQuantity = 100, WarningThreshold = 20 },
                new Ingredient { IngredientId = 5, Name = "Duong Cat",      Unit = "kg",   StockQuantity = 300, WarningThreshold = 40 },
                new Ingredient { IngredientId = 6, Name = "Sua Dac",        Unit = "lon",  StockQuantity = 400, WarningThreshold = 50 },
                new Ingredient { IngredientId = 7, Name = "Banh Pho",       Unit = "kg",   StockQuantity = 80,  WarningThreshold = 15 },
                new Ingredient { IngredientId = 8, Name = "Tra Den",        Unit = "kg",   StockQuantity = 50,  WarningThreshold = 10 }
            );

            // Seed Staff cho Lab 03
            modelBuilder.Entity<Staff>().HasData(
                new Staff { StaffId = 1, StaffCode = "NV01", FullName = "Nguyen Van An",  Role = "Cashier", BranchCode = "CS1" },
                new Staff { StaffId = 2, StaffCode = "NV02", FullName = "Tran Thi Binh",  Role = "Manager", BranchCode = "CS1" },
                new Staff { StaffId = 3, StaffCode = "NV03", FullName = "Le Van Cuong",   Role = "Chef",    BranchCode = "CS2" }
            );
        }
    }
}