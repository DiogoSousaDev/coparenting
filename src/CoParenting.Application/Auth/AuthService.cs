using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoParenting.Application.Auth;

public class AuthService(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailSender emailSender,
    IGoogleTokenValidator googleTokenValidator,
    IOptions<AppOptions> appOptions,
    ILogger<AuthService> logger) : IAuthService
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

        string? warning = null;
        try
        {
            await emailSender.SendAsync(
                email,
                "Confirme o seu email — CoParenting",
                $"Olá {firstName},<br/>Confirme o seu email clicando <a href=\"{confirmationLink}\">aqui</a>.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            // A conta já foi criada mesmo que o envio do email falhe — uma falha temporária
            // do provedor de email não deve bloquear o registo.
            logger.LogWarning(ex, "Falha ao enviar email de confirmação para {Email}", email);
            warning = "Conta criada, mas não foi possível enviar o email de confirmação. Tenta novamente mais tarde.";
        }

        return RegisterResult.Success(userId, warning);
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
        return LoginResult.Success(token.Value, token.ExpiresAtUtc, email);
    }

    public async Task<LoginResult> LoginWithGoogleAsync(string googleIdToken, CancellationToken cancellationToken = default)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(googleIdToken, cancellationToken);
        if (googleUser is null)
        {
            return LoginResult.Failure("Token do Google inválido.");
        }

        if (!googleUser.EmailVerified)
        {
            return LoginResult.Failure("O email da conta Google não está verificado.");
        }

        var userId = await identityService.FindOrCreateExternalUserAsync(googleUser.Email, googleUser.FirstName, googleUser.LastName);

        var token = jwtTokenGenerator.CreateToken(userId, googleUser.Email);
        return LoginResult.Success(token.Value, token.ExpiresAtUtc, googleUser.Email);
    }
}
