using Microsoft.EntityFrameworkCore;

namespace CoParenting.Services.Calendar;

public interface ICalendarDbContext
{
    DbSet<CalendarEvent> CalendarEvents { get; }
    DbSet<CustodySchedule> CustodySchedules { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
