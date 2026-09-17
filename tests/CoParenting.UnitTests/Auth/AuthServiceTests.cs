using CoParenting.Application.Auth;
using CoParenting.Application.Common.Models;
using CoParenting.UnitTests.TestUtilities;
using Microsoft.Extensions.Options;

namespace CoParenting.UnitTests.Auth;

public class AuthServiceTests
{
    private static readonly IOptions<AppOptions> AppOptions = Options.Create(new AppOptions { ClientBaseUrl = "http://localhost:5173" });

    private static (AuthService Sut, FakeIdentityService Identity, FakeEmailSender Email) CreateSut()
    {
        var identity = new FakeIdentityService();
        var email = new FakeEmailSender();
        var sut = new AuthService(identity, new FakeJwtTokenGenerator(), email, AppOptions);
        return (sut, identity, email);
    }

    [Fact]
    public async Task RegisterAsync_Succeeds_AndSendsConfirmationEmail()
    {
        var (sut, _, email) = CreateSut();

        var result = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.UserId);
        Assert.Single(email.SentEmails);
        Assert.Equal("pai@teste.com", email.SentEmails[0].ToEmail);
    }

    [Fact]
    public async Task RegisterAsync_Fails_OnDuplicateEmail()
    {
        var (sut, _, _) = CreateSut();
        await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        var result = await sut.RegisterAsync("pai@teste.com", "OutraPassword123!", "Pai", "Dois");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_Fails_BeforeEmailConfirmed()
    {
        var (sut, _, _) = CreateSut();
        await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");

        var result = await sut.LoginAsync("pai@teste.com", "Password123!");

        Assert.False(result.Succeeded);
        Assert.Contains("Confirme o seu email", result.Errors[0]);
    }

    [Fact]
    public async Task LoginAsync_Fails_WithWrongPassword()
    {
        var (sut, identity, _) = CreateSut();
        var register = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");
        await identity.ConfirmEmailAsync(register.UserId!.Value, FakeIdentityService.ConfirmationToken);

        var result = await sut.LoginAsync("pai@teste.com", "PasswordErrada!");

        Assert.False(result.Succeeded);
        Assert.Contains("Credenciais inválidas.", result.Errors[0]);
    }

    [Fact]
    public async Task LoginAsync_Succeeds_AfterEmailConfirmed_AndReturnsToken()
    {
        var (sut, identity, _) = CreateSut();
        var register = await sut.RegisterAsync("pai@teste.com", "Password123!", "Pai", "Um");
        await identity.ConfirmEmailAsync(register.UserId!.Value, FakeIdentityService.ConfirmationToken);

        var result = await sut.LoginAsync("pai@teste.com", "Password123!");

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }
}
