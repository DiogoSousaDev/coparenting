using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;
using CoParenting.Services.Authentication;
using CoParenting.UnitTests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoParenting.UnitTests.Auth;

public class AuthServiceTests
{
    private static readonly IOptions<AppOptions> AppOptions = Options.Create(new AppOptions { ClientBaseUrl = "http://localhost:5173" });

    private static (AuthService Sut, FakeIdentityService Identity, FakeEmailSender Email, FakeGoogleTokenValidator Google) CreateSut()
    {
        var identity = new FakeIdentityService();
        var email = new FakeEmailSender();
        var google = new FakeGoogleTokenValidator();
        var sut = new AuthService(identity, new FakeJwtTokenGenerator(), email, google, AppOptions, NullLogger<AuthService>.Instance);
        return (sut, identity, email, google);
    }

    [Fact]
    public async Task RegisterAsync_Succeeds_AndSendsConfirmationEmail()
    {
        var (sut, _, email, _) = CreateSut();

        var result = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UserId);
        Assert.Single(email.SentEmails);
        Assert.Equal("pai@teste.com", email.SentEmails[0].ToEmail);
    }

    [Fact]
    public async Task RegisterAsync_Fails_OnDuplicateEmail()
    {
        var (sut, _, _, _) = CreateSut();
        await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        var result = await sut.RegisterAsync("pai@teste.com", "OutraPassword123!", "Pai", "Dois");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_Fails_BeforeEmailConfirmed()
    {
        var (sut, _, _, _) = CreateSut();
        await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        var result = await sut.LoginAsync("pai@teste.com", "Password123!");

        Assert.False(result.Succeeded);
        Assert.Contains("Confirme o seu email", result.Errors[0]);
    }

    [Fact]
    public async Task LoginAsync_Fails_WithWrongPassword()
    {
        var (sut, identity, _, _) = CreateSut();
        var register = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");
        await identity.ConfirmEmailAsync(register.UserId!.Value, FakeIdentityService.ConfirmationToken);

        var result = await sut.LoginAsync("pai@teste.com", "PasswordErrada!");

        Assert.False(result.Succeeded);
        Assert.Contains("Credenciais inválidas.", result.Errors[0]);
    }

    [Fact]
    public async Task LoginAsync_Succeeds_AfterEmailConfirmed_AndReturnsToken()
    {
        var (sut, identity, _, _) = CreateSut();
        var register = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");
        await identity.ConfirmEmailAsync(register.UserId!.Value, FakeIdentityService.ConfirmationToken);

        var result = await sut.LoginAsync("pai@teste.com", "Password123!");

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("pai@teste.com", result.Email);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_Fails_WithInvalidToken()
    {
        var (sut, _, _, _) = CreateSut();

        var result = await sut.LoginWithGoogleAsync("token-invalido");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_Fails_WhenGoogleEmailNotVerified()
    {
        var (sut, _, _, google) = CreateSut();
        google.SetValidToken("token-valido", new GoogleUserInfo("mae@teste.com", "Mae", "Um", EmailVerified: false));

        var result = await sut.LoginWithGoogleAsync("token-valido");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_Succeeds_AndCreatesUserAlreadyConfirmed()
    {
        var (sut, identity, _, google) = CreateSut();
        google.SetValidToken("token-valido", new GoogleUserInfo("mae@teste.com", "Mae", "Um", EmailVerified: true));

        var result = await sut.LoginWithGoogleAsync("token-valido");

        Assert.True(result.Succeeded);
        Assert.Equal("mae@teste.com", result.Email);
        var userId = await identity.FindUserIdByEmailAsync("mae@teste.com");
        Assert.NotNull(userId);
        Assert.True(await identity.IsEmailConfirmedAsync(userId!.Value));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ReusesExistingAccount_ForSameEmail()
    {
        var (sut, identity, _, google) = CreateSut();
        var register = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");
        await identity.ConfirmEmailAsync(register.UserId!.Value, FakeIdentityService.ConfirmationToken);
        google.SetValidToken("token-valido", new GoogleUserInfo("pai@teste.com", "Pai", "Um", EmailVerified: true));

        var result = await sut.LoginWithGoogleAsync("token-valido");

        Assert.True(result.Succeeded);
        var userId = await identity.FindUserIdByEmailAsync("pai@teste.com");
        Assert.Equal(register.UserId, userId);
    }
}
