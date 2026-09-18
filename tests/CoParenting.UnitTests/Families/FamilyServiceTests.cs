using CoParenting.Application.Common.Models;
using CoParenting.Application.Families;
using CoParenting.Domain.Entities;
using CoParenting.UnitTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoParenting.UnitTests.Families;

public class FamilyServiceTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory = new();
    private static readonly IOptions<AppOptions> AppOptions = Options.Create(new AppOptions { ClientBaseUrl = "http://localhost:5173" });

    private FamilyService CreateSut(CoParenting.Infrastructure.Persistence.ApplicationDbContext context, FakeEmailSender? emailSender = null) =>
        new(context, new FamilyAccessService(context), emailSender ?? new FakeEmailSender(), AppOptions, NullLogger<FamilyService>.Instance);

    [Fact]
    public async Task CreateFamilyAsync_AllowsUserToCreateMultipleFamilies()
    {
        var userId = Guid.NewGuid();

        using var context = _factory.CreateContext();
        var sut = CreateSut(context);

        var first = await sut.CreateFamilyAsync(userId, "Família A", FamilyRole.Pai);
        var second = await sut.CreateFamilyAsync(userId, "Família B", FamilyRole.Pai);

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);

        var families = await sut.GetMyFamiliesAsync(userId);
        Assert.Equal(2, families.Count);
    }

    [Fact]
    public async Task CreateInviteAsync_Fails_WhenFamilyAlreadyHasTwoMembers()
    {
        var familyId = Guid.NewGuid();
        var inviter = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.Families.Add(new Family { Id = familyId, Name = "Família Cheia", CreatedAt = DateTime.UtcNow });
            context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = inviter, FamilyId = familyId, Role = FamilyRole.Pai });
            context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FamilyId = familyId, Role = FamilyRole.Mae });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = CreateSut(readContext);

        var result = await sut.CreateInviteAsync(inviter, familyId, "novo@teste.com", FamilyRole.Mae);

        Assert.False(result.Succeeded);
        Assert.Contains("número máximo", result.Errors[0]);
    }

    [Fact]
    public async Task CreateInviteAsync_Fails_WhenInviterIsNotAMember()
    {
        var familyId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.Families.Add(new Family { Id = familyId, Name = "Família Alheia", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = CreateSut(readContext);

        var result = await sut.CreateInviteAsync(Guid.NewGuid(), familyId, "novo@teste.com", FamilyRole.Mae);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AcceptInviteAsync_Fails_WhenEmailDoesNotMatchInvite()
    {
        var (familyId, rawToken) = await SeedFamilyWithInviteAsync("convidado@teste.com");

        using var context = _factory.CreateContext();
        var sut = CreateSut(context);

        var result = await sut.AcceptInviteAsync(Guid.NewGuid(), "outro-email@teste.com", rawToken);

        Assert.False(result.Succeeded);
        Assert.Contains("outro email", result.Errors[0]);
    }

    [Fact]
    public async Task AcceptInviteAsync_Fails_WhenInviteExpired()
    {
        var (familyId, rawToken) = await SeedFamilyWithInviteAsync("convidado@teste.com", expiresAt: DateTime.UtcNow.AddDays(-1));

        using var context = _factory.CreateContext();
        var sut = CreateSut(context);

        var result = await sut.AcceptInviteAsync(Guid.NewGuid(), "convidado@teste.com", rawToken);

        Assert.False(result.Succeeded);
        Assert.Contains("expirou", result.Errors[0]);
    }

    [Fact]
    public async Task AcceptInviteAsync_Fails_OnSecondAcceptAttempt()
    {
        var (familyId, rawToken) = await SeedFamilyWithInviteAsync("convidado@teste.com");
        var userId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context);
            var first = await sut.AcceptInviteAsync(userId, "convidado@teste.com", rawToken);
            Assert.True(first.Succeeded);
        }

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context);
            var second = await sut.AcceptInviteAsync(Guid.NewGuid(), "convidado@teste.com", rawToken);

            Assert.False(second.Succeeded);
            Assert.Contains("já foi aceite", second.Errors[0]);
        }
    }

    [Fact]
    public async Task AcceptInviteAsync_Fails_WhenUserAlreadyMemberOfThatFamily()
    {
        var (familyId, rawToken) = await SeedFamilyWithInviteAsync("convidado@teste.com");
        var userId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = userId, FamilyId = familyId, Role = FamilyRole.Mae });
            await context.SaveChangesAsync();
        }

        using var readContext = _factory.CreateContext();
        var sut = CreateSut(readContext);

        var result = await sut.AcceptInviteAsync(userId, "convidado@teste.com", rawToken);

        Assert.False(result.Succeeded);
        Assert.Contains("Já és membro", result.Errors[0]);
    }

    private async Task<(Guid FamilyId, string RawToken)> SeedFamilyWithInviteAsync(string invitedEmail, DateTime? expiresAt = null)
    {
        var familyId = Guid.NewGuid();
        var inviterId = Guid.NewGuid();

        using var context = _factory.CreateContext();
        context.Families.Add(new Family { Id = familyId, Name = "Família Teste", CreatedAt = DateTime.UtcNow });
        context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = inviterId, FamilyId = familyId, Role = FamilyRole.Pai });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);
        var inviteResult = await sut.CreateInviteAsync(inviterId, familyId, invitedEmail, FamilyRole.Mae);
        Assert.True(inviteResult.Succeeded);

        if (expiresAt is not null)
        {
            var invite = await context.FamilyInvites.FirstAsync(i => i.FamilyId == familyId);
            invite.ExpiresAt = expiresAt.Value;
            await context.SaveChangesAsync();
        }

        return (familyId, inviteResult.RawToken!);
    }

    public void Dispose() => _factory.Dispose();
}
