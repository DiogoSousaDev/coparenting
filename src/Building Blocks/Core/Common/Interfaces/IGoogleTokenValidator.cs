namespace CoParenting.BuildingBlocks.Core.Common.Interfaces;

public record GoogleUserInfo(string Email, string FirstName, string LastName, bool EmailVerified);

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
