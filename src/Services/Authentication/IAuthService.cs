namespace CoParenting.Services.Authentication;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<ConfirmEmailResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginWithGoogleAsync(string googleIdToken, CancellationToken cancellationToken = default);
}
