using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CommunityHub.API.Data
{
    public class CommunityHubDbContextFactory : IDesignTimeDbContextFactory<CommunityHubDbContext>
    {
        public CommunityHubDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var optionsBuilder = new DbContextOptionsBuilder<CommunityHubDbContext>();

            // This is to load the connection string from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Configure the DbContext with the connection string
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new CommunityHubDbContext(optionsBuilder.Options);
        }
    }
}
