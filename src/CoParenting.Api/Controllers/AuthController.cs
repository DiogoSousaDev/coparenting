using System.ComponentModel.DataAnnotations;
using CoParenting.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required] string FirstName,
        [Required] string LastName);

    public record RegisterResponse(Guid UserId, string Message);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Created(string.Empty, new RegisterResponse(result.UserId!.Value, "Verifique o seu email para confirmar a conta."));
    }

    public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

    public record LoginResponse(string Token, DateTime ExpiresAtUtc);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Ok(new LoginResponse(result.Token!, result.ExpiresAtUtc!.Value));
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await authService.ConfirmEmailAsync(userId, token, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors });
        }

        return Ok(new { Message = "Email confirmado com sucesso." });
    }
}
