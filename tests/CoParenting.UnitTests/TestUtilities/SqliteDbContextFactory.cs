using CoParenting.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.UnitTests.TestUtilities;

// Sqlite in-memory (não o provider InMemory do EF) porque este último não valida
// índices únicos — mascararia bugs nas regras que estes testes verificam.
public sealed class SqliteDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
