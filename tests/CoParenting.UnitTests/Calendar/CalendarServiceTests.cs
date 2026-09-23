using CoParenting.BuildingBlocks.Core.Common.Models;
using CoParenting.Services.Calendar;
using CoParenting.Services.Families;
using CoParenting.UnitTests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoParenting.UnitTests.Calendar;

public class CalendarServiceTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory = new();
    private static readonly IOptions<AppOptions> AppOptions = Options.Create(new AppOptions { ClientBaseUrl = "http://localhost:5173" });

    private CalendarService CreateSut(
        CoParenting.Api.Persistence.ApplicationDbContext context,
        FakeIdentityService? identity = null,
        FakeEmailSender? emailSender = null) =>
        new(context, new FamilyAccessService(context), identity ?? new FakeIdentityService(), emailSender ?? new FakeEmailSender(), AppOptions, NullLogger<CalendarService>.Instance);

    private async Task<(Guid FamilyId, Guid CreatorId, Guid OtherMemberId)> SeedFamilyWithTwoMembersAsync(FakeIdentityService identity)
    {
        var familyId = Guid.NewGuid();

        var (creatorResult, creatorId) = await identity.CreateUserAsync("criador@teste.com", "Password123!", "Pai", "Um");
        var (_, otherId) = await identity.CreateUserAsync("outro@teste.com", "Password123!", "Mae", "Um");

        using var context = _factory.CreateContext();
        context.Families.Add(new Family { Id = familyId, Name = "Família Teste", CreatedAt = DateTime.UtcNow });
        context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = creatorId, FamilyId = familyId, Role = FamilyRole.Pai });
        context.FamilyMembers.Add(new FamilyMember { Id = Guid.NewGuid(), UserId = otherId, FamilyId = familyId, Role = FamilyRole.Mae });
        await context.SaveChangesAsync();

        return (familyId, creatorId, otherId);
    }

    [Fact]
    public async Task CreateEventAsync_Fails_WhenUserIsNotFamilyMember()
    {
        using var context = _factory.CreateContext();
        context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Família Alheia", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var familyId = context.Families.Single().Id;
        var sut = CreateSut(context);

        var result = await sut.CreateEventAsync(Guid.NewGuid(), familyId, "Consulta", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateEventAsync_Fails_WhenEndBeforeStart()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var start = DateTime.UtcNow;
        var result = await sut.CreateEventAsync(creatorId, familyId, "Consulta", start, start.AddHours(-1), null);

        Assert.False(result.Succeeded);
        Assert.Contains("fim", result.Errors[0]);
    }

    [Fact]
    public async Task CreateEventAsync_Fails_WithInvalidRecurrenceRule()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var start = DateTime.UtcNow;
        var result = await sut.CreateEventAsync(creatorId, familyId, "Consulta", start, start.AddHours(1), "isto-nao-e-uma-rrule");

        Assert.False(result.Succeeded);
        Assert.Contains("recorrência", result.Errors[0]);
    }

    [Fact]
    public async Task CreateEventAsync_Succeeds_AndNotifiesOnlyOtherMember()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, otherId) = await SeedFamilyWithTwoMembersAsync(identity);
        var emailSender = new FakeEmailSender();

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity, emailSender);

        var start = DateTime.UtcNow;
        var result = await sut.CreateEventAsync(creatorId, familyId, "Consulta pediatra", start, start.AddHours(1), null);

        Assert.True(result.Succeeded);
        Assert.Single(emailSender.SentEmails);
        Assert.Equal("outro@teste.com", emailSender.SentEmails[0].ToEmail);
    }

    [Fact]
    public async Task GetEventsAsync_ExpandsWeeklyRecurringEvent_WithinRange()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var start = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc); // segunda-feira
        var create = await sut.CreateEventAsync(creatorId, familyId, "Semana com o pai", start, start.AddHours(2), "FREQ=WEEKLY;INTERVAL=1");
        Assert.True(create.Succeeded);

        var occurrences = await sut.GetEventsAsync(creatorId, familyId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(4, occurrences.Count);
        Assert.All(occurrences, o => Assert.True(o.IsRecurring));
    }

    [Fact]
    public async Task GetEventsAsync_ExcludesNonRecurringEvent_OutsideRange()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        using var context = _factory.CreateContext();
        var sut = CreateSut(context, identity);

        var farFuture = new DateTime(2027, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        await sut.CreateEventAsync(creatorId, familyId, "Evento distante", farFuture, farFuture.AddHours(1), null);

        var occurrences = await sut.GetEventsAsync(creatorId, familyId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.Empty(occurrences);
    }

    [Fact]
    public async Task UpdateEventAsync_Fails_WhenEventNotFound()
    {
        using var context = _factory.CreateContext();
        var sut = CreateSut(context);

        var result = await sut.UpdateEventAsync(Guid.NewGuid(), Guid.NewGuid(), "Título", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEvent()
    {
        var identity = new FakeIdentityService();
        var (familyId, creatorId, _) = await SeedFamilyWithTwoMembersAsync(identity);

        Guid eventId;
        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context, identity);
            var start = DateTime.UtcNow;
            var create = await sut.CreateEventAsync(creatorId, familyId, "A apagar", start, start.AddHours(1), null);
            eventId = create.EventId!.Value;
        }

        using (var context = _factory.CreateContext())
        {
            var sut = CreateSut(context, identity);
            var delete = await sut.DeleteEventAsync(creatorId, eventId);
            Assert.True(delete.Succeeded);
        }

        using (var context = _factory.CreateContext())
        {
            Assert.Empty(context.CalendarEvents);
        }
    }

    public void Dispose() => _factory.Dispose();
}
