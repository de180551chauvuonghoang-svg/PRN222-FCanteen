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
        }
    }
}