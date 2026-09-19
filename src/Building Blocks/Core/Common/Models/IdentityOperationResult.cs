namespace CoParenting.BuildingBlocks.Core.Common.Models;

public record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, []);
    public static IdentityOperationResult Failure(params string[] errors) => new(false, errors);
}
