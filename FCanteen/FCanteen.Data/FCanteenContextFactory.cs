using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FCanteen.Data
{
    public class FCanteenContextFactory : IDesignTimeDbContextFactory<FCanteenContext>
    {
        public FCanteenContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("FCanteenDb"));
            return new FCanteenContext(optionsBuilder.Options);
        }
    }
}