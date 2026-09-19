namespace CoParenting.Services.Families;

public record CreateFamilyResult(bool Succeeded, Guid? FamilyId, IReadOnlyList<string> Errors)
{
    public static CreateFamilyResult Success(Guid familyId) => new(true, familyId, []);
    public static CreateFamilyResult Failure(params string[] errors) => new(false, null, errors);
}

public record FamilySummary(Guid FamilyId, string Name, FamilyRole Role);

public record InviteResult(bool Succeeded, DateTime? ExpiresAtUtc, string? RawToken, string? Warning, IReadOnlyList<string> Errors)
{
    public static InviteResult Success(DateTime expiresAtUtc, string rawToken, string? warning = null) => new(true, expiresAtUtc, rawToken, warning, []);
    public static InviteResult Failure(params string[] errors) => new(false, null, null, null, errors);
}

public record InviteDetailsResult(bool Found, string? FamilyName, string? InvitedEmail, bool Expired, bool AlreadyAccepted);

public record AcceptInviteResult(bool Succeeded, Guid? FamilyId, IReadOnlyList<string> Errors)
{
    public static AcceptInviteResult Success(Guid familyId) => new(true, familyId, []);
    public static AcceptInviteResult Failure(params string[] errors) => new(false, null, errors);
}
