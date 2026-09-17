namespace CoParenting.Domain.Entities;

public class Child
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public Family Family { get; set; } = null!;
}
