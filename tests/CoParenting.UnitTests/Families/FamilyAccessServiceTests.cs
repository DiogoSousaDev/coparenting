using CoParenting.Application.Families;
using CoParenting.Domain.Entities;
using CoParenting.UnitTests.TestUtilities;

namespace CoParenting.UnitTests.Families;

public class FamilyAccessServiceTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory = new();

    [Fact]
    public async Task IsMemberAsync_ReturnsTrue_ForActualMember()
    {
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.Families.Add(new Family { Id = familyId, Name = "Família Teste", CreatedAt = DateTime.UtcNow });
            context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = userId, FamilyId = familyId, Role = FamilyRole.Pai });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = new FamilyAccessService(readContext);

        Assert.True(await sut.IsMemberAsync(userId, familyId));
    }

    [Fact]
    public async Task IsMemberAsync_ReturnsFalse_ForNonMember()
    {
        var familyId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.Families.Add(new Family { Id = familyId, Name = "Família Teste", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = new FamilyAccessService(readContext);

        Assert.False(await sut.IsMemberAsync(Guid.NewGuid(), familyId));
    }

    [Fact]
    public async Task IsMemberAsync_ReturnsFalse_ForMemberOfADifferentFamily()
    {
        var familyId = Guid.NewGuid();
        var otherFamilyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.Families.Add(new Family { Id = familyId, Name = "Família A", CreatedAt = DateTime.UtcNow });
            context.Families.Add(new Family { Id = otherFamilyId, Name = "Família B", CreatedAt = DateTime.UtcNow });
            context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = userId, FamilyId = otherFamilyId, Role = FamilyRole.Mae });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = new FamilyAccessService(readContext);

        Assert.False(await sut.IsMemberAsync(userId, familyId));
    }

    public void Dispose() => _factory.Dispose();
}
