using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoParenting.Services.Calendar;

public class CalendarService(
    ICalendarDbContext dbContext,
    IFamilyAccessService familyAccessService,
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions,
    ILogger<CalendarService> logger) : ICalendarService
{
    public async Task<CreateEventResult> CreateEventAsync(
        Guid userId,
        Guid familyId,
        string title,
        DateTime start,
        DateTime end,
        string? recurrenceRule,
        CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(userId, familyId, cancellationToken))
        {
            return CreateEventResult.Failure("Não tens acesso a esta família.");
        }

        var validationError = ValidateEventInput(start, end, recurrenceRule);
        if (validationError is not null)
        {
            return CreateEventResult.Failure(validationError);
        }

        var isRecurring = !string.IsNullOrWhiteSpace(recurrenceRule);
        var calendarEvent = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Title = title,
            StartDate = start,
            EndDate = end,
            IsRecurring = isRecurring,
            RecurrenceRule = isRecurring ? recurrenceRule : null,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.CalendarEvents.Add(calendarEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        await NotifyOtherMembersAsync(familyId, userId, title, "criou", cancellationToken);

        return CreateEventResult.Success(calendarEvent.Id);
    }

    public async Task<UpdateEventResult> UpdateEventAsync(
        Guid userId,
        Guid eventId,
        string title,
        DateTime start,
        DateTime end,
        string? recurrenceRule,
        CancellationToken cancellationToken = default)
    {
        var calendarEvent = await dbContext.CalendarEvents.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (calendarEvent is null)
        {
            return UpdateEventResult.Failure("Evento não encontrado.");
        }

        if (!await familyAccessService.IsMemberAsync(userId, calendarEvent.FamilyId, cancellationToken))
        {
            return UpdateEventResult.Failure("Não tens acesso a esta família.");
        }

        var validationError = ValidateEventInput(start, end, recurrenceRule);
        if (validationError is not null)
        {
            return UpdateEventResult.Failure(validationError);
        }

        var isRecurring = !string.IsNullOrWhiteSpace(recurrenceRule);
        calendarEvent.Title = title;
        calendarEvent.StartDate = start;
        calendarEvent.EndDate = end;
        calendarEvent.IsRecurring = isRecurring;
        calendarEvent.RecurrenceRule = isRecurring ? recurrenceRule : null;

        await dbContext.SaveChangesAsync(cancellationToken);

        await NotifyOtherMembersAsync(calendarEvent.FamilyId, userId, title, "alterou", cancellationToken);

        return UpdateEventResult.Success();
    }

    public async Task<DeleteEventResult> DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var calendarEvent = await dbContext.CalendarEvents.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (calendarEvent is null)
        {
            return DeleteEventResult.Failure("Evento não encontrado.");
        }

        if (!await familyAccessService.IsMemberAsync(userId, calendarEvent.FamilyId, cancellationToken))
        {
            return DeleteEventResult.Failure("Não tens acesso a esta família.");
        }

        dbContext.CalendarEvents.Remove(calendarEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return DeleteEventResult.Success();
    }

    public async Task<IReadOnlyList<EventOccurrence>> GetEventsAsync(
        Guid userId,
        Guid familyId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(userId, familyId, cancellationToken))
        {
            return [];
        }

        // Trazemos para memória os eventos candidatos da família (não são muitos por família)
        // porque a expansão de RRULE não é traduzível para SQL.
        var events = await dbContext.CalendarEvents
            .Where(e => e.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var occurrences = new List<EventOccurrence>();
        foreach (var calendarEvent in events)
        {
            foreach (var (start, end) in RecurrenceExpander.GetOccurrences(calendarEvent, rangeStart, rangeEnd))
            {
                occurrences.Add(new EventOccurrence(calendarEvent.Id, familyId, calendarEvent.Title, start, end, calendarEvent.IsRecurring, calendarEvent.RecurrenceRule));
            }
        }

        return [.. occurrences.OrderBy(o => o.Start)];
    }

    private static string? ValidateEventInput(DateTime start, DateTime end, string? recurrenceRule)
    {
        if (end <= start)
        {
            return "A data de fim tem de ser depois da data de início.";
        }

        if (!string.IsNullOrWhiteSpace(recurrenceRule) && !RecurrenceExpander.IsValidRule(recurrenceRule))
        {
            return "Regra de recorrência inválida.";
        }

        return null;
    }

    private async Task NotifyOtherMembersAsync(Guid familyId, Guid actingUserId, string eventTitle, string action, CancellationToken cancellationToken)
    {
        var memberIds = await familyAccessService.GetMemberUserIdsAsync(familyId, cancellationToken);

        foreach (var memberId in memberIds.Where(id => id != actingUserId))
        {
            var email = await identityService.FindEmailByUserIdAsync(memberId);
            if (email is null)
            {
                continue;
            }

            try
            {
                var calendarLink = $"{appOptions.Value.ClientBaseUrl}/calendar";
                await emailSender.SendAsync(
                    email,
                    "Atualização no calendário — CoParenting",
                    $"O outro progenitor {action} o evento \"{eventTitle}\". <a href=\"{calendarLink}\">Ver calendário</a>.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Falha no envio não deve bloquear a criação/edição do evento — mesma
                // política já usada para os emails de confirmação/convite.
                logger.LogWarning(ex, "Falha ao enviar notificação de calendário para {Email}", email);
            }
        }
    }
}
