namespace CoParenting.Services.Families;

public class FamilyInvite
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public FamilyRole Role { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public Family Family { get; set; } = null!;
}
