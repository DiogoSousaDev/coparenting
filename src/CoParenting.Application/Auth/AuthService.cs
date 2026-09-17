using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CoParenting.Application.Auth;

public class AuthService(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var (result, userId) = await identityService.CreateUserAsync(email, password, firstName, lastName);
        if (!result.Succeeded)
        {
            return RegisterResult.Failure([.. result.Errors]);
        }

        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);
        var confirmationLink = $"{appOptions.Value.ClientBaseUrl}/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendAsync(
            email,
            "Confirme o seu email — CoParenting",
            $"Olá {firstName},<br/>Confirme o seu email clicando <a href=\"{confirmationLink}\">aqui</a>.",
            cancellationToken);

        return RegisterResult.Success(userId);
    }

    public async Task<ConfirmEmailResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var result = await identityService.ConfirmEmailAsync(userId, token);
        return result.Succeeded
            ? ConfirmEmailResult.Success()
            : ConfirmEmailResult.Failure([.. result.Errors]);
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var userId = await identityService.FindUserIdByEmailAsync(email);
        if (userId is null)
        {
            return LoginResult.Failure("Credenciais inválidas.");
        }

        // Verificar a confirmação do email ANTES da password: CheckPasswordAsync não passa
        // pelo SignInManager, por isso não bloqueia sozinho o login de contas não confirmadas.
        if (!await identityService.IsEmailConfirmedAsync(userId.Value))
        {
            return LoginResult.Failure("Confirme o seu email antes de iniciar sessão.");
        }

        if (!await identityService.CheckPasswordAsync(userId.Value, password))
        {
            return LoginResult.Failure("Credenciais inválidas.");
        }

        var token = jwtTokenGenerator.CreateToken(userId.Value, email);
        return LoginResult.Success(token.Value, token.ExpiresAtUtc);
    }
}
