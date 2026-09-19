namespace CoParenting.Services.Families;

public class Family
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<FamilyInvite> Invites { get; set; } = new List<FamilyInvite>();
}
