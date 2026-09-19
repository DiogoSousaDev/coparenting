namespace CoParenting.Services.Authentication;

public record RegisterResult(bool Succeeded, Guid? UserId, string? Warning, IReadOnlyList<string> Errors)
{
    public static RegisterResult Success(Guid userId, string? warning = null) => new(true, userId, warning, []);
    public static RegisterResult Failure(params string[] errors) => new(false, null, null, errors);
}

public record ConfirmEmailResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static ConfirmEmailResult Success() => new(true, []);
    public static ConfirmEmailResult Failure(params string[] errors) => new(false, errors);
}

public record LoginResult(bool Succeeded, string? Token, DateTime? ExpiresAtUtc, string? Email, IReadOnlyList<string> Errors)
{
    public static LoginResult Success(string token, DateTime expiresAtUtc, string email) => new(true, token, expiresAtUtc, email, []);
    public static LoginResult Failure(params string[] errors) => new(false, null, null, null, errors);
}
