namespace CoParenting.Services.Calendar;

public record EventOccurrence(Guid EventId, Guid FamilyId, string Title, DateTime Start, DateTime End, bool IsRecurring, string? RecurrenceRule);

public record CreateEventResult(bool Succeeded, Guid? EventId, IReadOnlyList<string> Errors)
{
    public static CreateEventResult Success(Guid eventId) => new(true, eventId, []);
    public static CreateEventResult Failure(params string[] errors) => new(false, null, errors);
}

public record UpdateEventResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static UpdateEventResult Success() => new(true, []);
    public static UpdateEventResult Failure(params string[] errors) => new(false, errors);
}

public record DeleteEventResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static DeleteEventResult Success() => new(true, []);
    public static DeleteEventResult Failure(params string[] errors) => new(false, errors);
}
