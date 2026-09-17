using CoParenting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<Child> Children { get; }
    DbSet<CalendarEvent> CalendarEvents { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<Expense> Expenses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
