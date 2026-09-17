namespace CoParenting.Application.Auth;

public record RegisterResult(bool Succeeded, Guid? UserId, IReadOnlyList<string> Errors)
{
    public static RegisterResult Success(Guid userId) => new(true, userId, []);
    public static RegisterResult Failure(params string[] errors) => new(false, null, errors);
}

public record ConfirmEmailResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static ConfirmEmailResult Success() => new(true, []);
    public static ConfirmEmailResult Failure(params string[] errors) => new(false, errors);
}

public record LoginResult(bool Succeeded, string? Token, DateTime? ExpiresAtUtc, IReadOnlyList<string> Errors)
{
    public static LoginResult Success(string token, DateTime expiresAtUtc) => new(true, token, expiresAtUtc, []);
    public static LoginResult Failure(params string[] errors) => new(false, null, null, errors);
}
