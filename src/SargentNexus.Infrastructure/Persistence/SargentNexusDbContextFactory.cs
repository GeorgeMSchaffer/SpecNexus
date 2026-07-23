using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SargentNexus.Infrastructure;

public sealed class SargentNexusDbContextFactory : IDesignTimeDbContextFactory<SargentNexusDbContext>
{
    public SargentNexusDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SargentNexusDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=SargentNexus;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True");

        return new SargentNexusDbContext(optionsBuilder.Options);
    }
}