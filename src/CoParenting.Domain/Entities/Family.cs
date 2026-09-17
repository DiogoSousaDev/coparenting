namespace CoParenting.Domain.Entities;

public class Family
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<FamilyInvite> Invites { get; set; } = new List<FamilyInvite>();
}
