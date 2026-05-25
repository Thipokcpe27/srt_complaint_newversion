using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SRT.Complaint.Data;

public class CorruptionDbContextFactory : IDesignTimeDbContextFactory<CorruptionDbContext>
{
    public CorruptionDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CorruptionDbContext>();
        optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        return new CorruptionDbContext(optionsBuilder.Options);
    }
}
