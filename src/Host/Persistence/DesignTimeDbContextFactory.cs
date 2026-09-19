using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoParenting.Api.Persistence;

// Usada apenas pelas ferramentas `dotnet ef` (migrations) para construir o DbContext
// sem depender do container de DI completo da Api (que exige serviços de Identity/DataProtection
// não resolvíveis em tempo de design).
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=coparenting_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
