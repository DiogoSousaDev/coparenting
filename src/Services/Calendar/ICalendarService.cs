namespace CoParenting.Services.Calendar;

public interface ICalendarService
{
    Task<CreateEventResult> CreateEventAsync(
        Guid userId,
        Guid familyId,
        string title,
        DateTime start,
        DateTime end,
        string? recurrenceRule,
        CancellationToken cancellationToken = default);

    Task<UpdateEventResult> UpdateEventAsync(
        Guid userId,
        Guid eventId,
        string title,
        DateTime start,
        DateTime end,
        string? recurrenceRule,
        CancellationToken cancellationToken = default);

    Task<DeleteEventResult> DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventOccurrence>> GetEventsAsync(
        Guid userId,
        Guid familyId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken = default);
}
