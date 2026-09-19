namespace CoParenting.Services.Calendar;

public class CalendarEvent
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurring { get; set; }

    // Formato iCalendar (RRULE) para facilitar sincronização futura com Google Calendar/Outlook.
    public string? RecurrenceRule { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
