namespace CoParenting.Domain.Entities;

// UserId aponta para o Id do ApplicationUser (Identity), que vive na camada Infrastructure.
public class FamilyMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }
    public FamilyRole Role { get; set; }

    public Family Family { get; set; } = null!;
}
