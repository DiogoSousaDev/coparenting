using CoParenting.BuildingBlocks.Core.Common.Models;
using CoParenting.Services.Calendar;
using CoParenting.Services.Families;
using CoParenting.UnitTests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoParenting.UnitTests.Calendar;

public class CustodyServiceTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory = new();
    private static readonly IOptions<AppOptions> AppOptions = Options.Create(new AppOptions { ClientBaseUrl = "http://localhost:5173" });

    private static readonly List<CustodySegmentDto> AlternatingWeekSegments =
    [
        new CustodySegmentDto(CustodyRole.Pai, 168),
        new CustodySegmentDto(CustodyRole.Mae, 168),
    ];

    private CustodyService CreateSut(
        CoParenting.Api.Persistence.ApplicationDbContext context,
        FakeIdentityService? identity = null,
        FakeEmailSender? emailSender = null) =>
        new(context, new FamilyAccessService(context), identity ?? new FakeIdentityService(), emailSender ?? new FakeEmailSender(), AppOptions, NullLogger<CustodyService>.Instance);

    private async Task<(Guid FamilyId, Guid CreatorId, Guid OtherMemberId)> SeedFamilyWithTwoMembersAsync(FakeIdentityService identity)
    {
        var familyId = Guid.NewGuid();

        var (_, creatorId) = await identity.CreateUserAsync("criador@teste.com", "Password123!", "Pai", "Um");
        var (_, otherId) = await identity.CreateUserAsync("outro@teste.com", "Password123!", "Mae", "Um");

        using var context = _factory.CreateContext();
        context.Families.Add(new Family { Id = familyId, Name = "Família Teste", CreatedAt = DateTime.UtcNow });
        context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = creatorId, FamilyId = familyId, Role = FamilyRole.Pai });
        context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = otherId, FamilyId = familyId, Role = FamilyRole.Mae });
        await context.SaveChangesAsync();

        return (familyId, creatorId, otherId);
    }

    [Fact]
    public async Task UpsertScheduleAsync_Fails_WhenUserIsNotFamilyMember()
    {
        using var context = _factory.CreateContext();
        context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Família Alheia", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var familyId = context.Families.Single().Id;
        var sut = CreateSut(context);

        var result = await sut.UpsertScheduleAsync(Guid.NewGuid(), familyId, DateTime.UtcNow, "#3b82f6", "#ec4899", AlternatingWeekSegments);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpsertScheduleAsync_Fails_WhenNoSegments()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var result = await sut.UpsertScheduleAsync(creatorId, familyId, DateTime.UtcNow, "#3b82f6", "#ec4899", []);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpsertScheduleAsync_Fails_WhenSegmentDurationIsNotPositive()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var result = await sut.UpsertScheduleAsync(
            creatorId, familyId, DateTime.UtcNow, "#3b82f6", "#ec4899",
            [new CustodySegmentDto(CustodyRole.Pai, 0)]);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpsertScheduleAsync_Fails_WithInvalidColorFormat()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var result = await sut.UpsertScheduleAsync(creatorId, familyId, DateTime.UtcNow, "azul", "#ec4899", AlternatingWeekSegments);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpsertScheduleAsync_Succeeds_AndNotifiesOnlyOtherMember()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);
        var emailSender = new FakeEmailSender();

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity, emailSender);

        var result = await sut.UpsertScheduleAsync(creatorId, familyId, DateTime.UtcNow, "#3b82f6", "#ec4899", AlternatingWeekSegments);

        Assert.True(result.Succeeded);
        Assert.Single(emailSender.SentEmails);
        Assert.Equal("outro@teste.com", emailSender.SentEmails[0].ToEmail);
    }

    [Fact]
    public async Task UpsertScheduleAsync_ReplacesSegments_InsteadOfAccumulating()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);
        var anchor = new DateTime(2026, 9, 25, 17, 0, 0, DateTimeKind.Utc);

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context, identity);
            await sut.UpsertScheduleAsync(creatorId, familyId, anchor, "#3b82f6", "#ec4899", AlternatingWeekSegments);
        }

        var newSegments = new List<CustodySegmentDto>
        {
            new(CustodyRole.Mae, 48),
            new(CustodyRole.Pai, 48),
            new(CustodyRole.Mae, 72),
        };

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context, identity);
            var result = await sut.UpsertScheduleAsync(creatorId, familyId, anchor, "#3b82f6", "#ec4899", newSegments);
            Assert.True(result.Succeeded);
        }

        using (var context = _factory.CreateContext())
        {
            var schedule = await CreateSut(context, identity).GetScheduleAsync(creatorId, familyId);
            Assert.NotNull(schedule);
            Assert.Equal(3, schedule.Segments.Count);
            Assert.Equal(CustodyRole.Mae, schedule.Segments[0].Role);
            Assert.Equal(48, schedule.Segments[0].DurationHours);
        }
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsNull_WhenNotConfigured()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var schedule = await sut.GetScheduleAsync(creatorId, familyId);

        Assert.Null(schedule);
    }

    [Fact]
    public async Task GetPeriodsAsync_ReturnsEmpty_WhenNotConfigured()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var periods = await sut.GetPeriodsAsync(creatorId, familyId, DateTime.UtcNow, DateTime.UtcNow.AddDays(30));

        Assert.Empty(periods);
    }

    [Fact]
    public async Task GetPeriodsAsync_ReturnsAlternatingPeriods_WithCorrectColors()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);
        var anchor = new DateTime(2026, 9, 25, 17, 0, 0, DateTimeKind.Utc);

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context, identity);
            await sut.UpsertScheduleAsync(creatorId, familyId, anchor, "#3b82f6", "#ec4899", AlternatingWeekSegments);
        }

        using var readContext = _factory.CreateContext();
        var readSut = CreateSut(readContext, identity);
        var periods = await readSut.GetPeriodsAsync(creatorId, familyId, anchor, anchor.AddDays(14));

        Assert.Equal(2, periods.Count);
        Assert.Equal(CustodyRole.Pai, periods[0].Role);
        Assert.Equal("#3b82f6", periods[0].Color);
        Assert.Equal(CustodyRole.Mae, periods[1].Role);
        Assert.Equal("#ec4899", periods[1].Color);
    }

    public void Dispose() => _factory.Dispose();
}
