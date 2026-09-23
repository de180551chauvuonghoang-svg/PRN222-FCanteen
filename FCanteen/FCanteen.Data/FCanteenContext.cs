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

        // Lab 04 DbSets
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<MenuItemIngredient> MenuItemIngredients => Set<MenuItemIngredient>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Cau hinh toan bo thuoc tinh decimal ve chuan decimal(18, 2)
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Khoa chinh phuc hop cho quan he Nhieu-Nhieu MenuItemIngredient
            modelBuilder.Entity<MenuItemIngredient>()
                .HasKey(mi => new { mi.MenuItemId, mi.IngredientId });

            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.MenuItem)
                .WithMany(m => m.MenuItemIngredients)
                .HasForeignKey(mi => mi.MenuItemId);

            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.Ingredient)
                .WithMany(i => i.MenuItemIngredients)
                .HasForeignKey(mi => mi.IngredientId);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "MÃ³n chÃ­nh", Description = "CÆ¡m, bÃºn, phá»Ÿ, mÃ¬ cÃ¡c loáº¡i" },
                new Category { CategoryId = 2, Name = "MÃ³n phá»¥", Description = "BÃ¡nh mÃ¬, xÃ´i, bÃ¡nh bao Äƒn nháº¹" },
                new Category { CategoryId = 3, Name = "Äá»“ uá»‘ng", Description = "TrÃ  sá»¯a, nÆ°á»›c Ã©p, sinh tá»‘" },
                new Category { CategoryId = 4, Name = "TrÃ¡ng miá»‡ng", Description = "ChÃ¨, sá»¯a chua, hoa quáº£" }
            );

            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { SupplierId = 1, Name = "NPP Thuc Pham Sach FPT", Phone = "0905123456", Email = "thucpham@fpt.vn", Address = "Ngu Hanh Son, Da Nang" },
                new Supplier { SupplierId = 2, Name = "NPP Do Uong & Nhat Giai Khat", Phone = "0905987654", Email = "douong@fpt.vn", Address = "Hai Chau, Da Nang" }
            );

            // Seed 15 mon an kem ItemCode va CategoryId
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { MenuItemId = 1,  ItemCode = "MON-0001", CategoryId = 1, Name = "Com ga",           Price = 35000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 2,  ItemCode = "MON-0002", CategoryId = 1, Name = "Com suon",         Price = 35000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 3,  ItemCode = "MON-0003", CategoryId = 1, Name = "Com tam",          Price = 30000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 4,  ItemCode = "MON-0004", CategoryId = 1, Name = "Bun bo",           Price = 40000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 5,  ItemCode = "MON-0005", CategoryId = 1, Name = "Pho bo",           Price = 45000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 6,  ItemCode = "MON-0006", CategoryId = 1, Name = "Mi quang",         Price = 35000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 7,  ItemCode = "MON-0007", CategoryId = 2, Name = "Banh mi thit",     Price = 20000, Unit = "o",    IsAvailable = true },
                new MenuItem { MenuItemId = 8,  ItemCode = "MON-0008", CategoryId = 2, Name = "Xoi ga",           Price = 25000, Unit = "phan", IsAvailable = true },
                new MenuItem { MenuItemId = 9,  ItemCode = "MON-0009", CategoryId = 4, Name = "Che dau xanh",     Price = 15000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 10, ItemCode = "MON-0010", CategoryId = 3, Name = "Tra sua tran chau", Price = 30000, Unit = "ly",  IsAvailable = true },
                new MenuItem { MenuItemId = 11, ItemCode = "MON-0011", CategoryId = 3, Name = "Nuoc cam ep",      Price = 20000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 12, ItemCode = "MON-0012", CategoryId = 3, Name = "Sinh to bo",       Price = 25000, Unit = "ly",   IsAvailable = true },
                new MenuItem { MenuItemId = 13, ItemCode = "MON-0013", CategoryId = 2, Name = "Banh bao nhan thit", Price = 15000, Unit = "cai", IsAvailable = true },
                new MenuItem { MenuItemId = 14, ItemCode = "MON-0014", CategoryId = 1, Name = "Hu tieu nam vang", Price = 40000, Unit = "to",   IsAvailable = true },
                new MenuItem { MenuItemId = 15, ItemCode = "MON-0015", CategoryId = 1, Name = "Chao ga",          Price = 30000, Unit = "to",   IsAvailable = true }
            );

            // Seed nguyen lieu kem CostPrice va SupplierId
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { IngredientId = 1, SupplierId = 1, Name = "Gao Thom",       Unit = "kg",   StockQuantity = 500, WarningThreshold = 50, CostPrice = 18000 },
                new Ingredient { IngredientId = 2, SupplierId = 1, Name = "Thit Ga",        Unit = "kg",   StockQuantity = 200, WarningThreshold = 30, CostPrice = 65000 },
                new Ingredient { IngredientId = 3, SupplierId = 1, Name = "Suon Heo",       Unit = "kg",   StockQuantity = 150, WarningThreshold = 25, CostPrice = 90000 },
                new Ingredient { IngredientId = 4, SupplierId = 1, Name = "Thit Bo",        Unit = "kg",   StockQuantity = 100, WarningThreshold = 20, CostPrice = 180000 },
                new Ingredient { IngredientId = 5, SupplierId = 2, Name = "Duong Cat",      Unit = "kg",   StockQuantity = 300, WarningThreshold = 40, CostPrice = 22000 },
                new Ingredient { IngredientId = 6, SupplierId = 2, Name = "Sua Dac",        Unit = "lon",  StockQuantity = 400, WarningThreshold = 50, CostPrice = 24000 },
                new Ingredient { IngredientId = 7, SupplierId = 1, Name = "Banh Pho",       Unit = "kg",   StockQuantity = 80,  WarningThreshold = 15, CostPrice = 20000 },
                new Ingredient { IngredientId = 8, SupplierId = 2, Name = "Tra Den",        Unit = "kg",   StockQuantity = 50,  WarningThreshold = 10, CostPrice = 120000 }
            );

            // Seed MenuItemIngredients (Dinh luong nguyen lieu)
            // Com ga: 0.15kg Gao (2.7k) + 0.2kg Thit ga (13k) = 15.7k gia von
            modelBuilder.Entity<MenuItemIngredient>().HasData(
                new MenuItemIngredient { MenuItemId = 1, IngredientId = 1, Quantity = 0.15m },
                new MenuItemIngredient { MenuItemId = 1, IngredientId = 2, Quantity = 0.20m },
                new MenuItemIngredient { MenuItemId = 2, IngredientId = 1, Quantity = 0.15m },
                new MenuItemIngredient { MenuItemId = 2, IngredientId = 3, Quantity = 0.18m },
                new MenuItemIngredient { MenuItemId = 4, IngredientId = 4, Quantity = 0.10m },
                new MenuItemIngredient { MenuItemId = 5, IngredientId = 7, Quantity = 0.25m },
                new MenuItemIngredient { MenuItemId = 5, IngredientId = 4, Quantity = 0.12m },
                new MenuItemIngredient { MenuItemId = 10, IngredientId = 8, Quantity = 0.03m },
                new MenuItemIngredient { MenuItemId = 10, IngredientId = 6, Quantity = 0.10m }
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