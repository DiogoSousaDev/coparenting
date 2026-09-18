using CoParenting.Application.Common.Models;

namespace CoParenting.Application.Common.Interfaces;

// Abstrai o UserManager<ApplicationUser> (Identity vive em Infrastructure;
// Application não pode referenciar ApplicationUser diretamente).
public interface IIdentityService
{
    Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(string email, string password, string firstName, string lastName);
    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);
    Task<IdentityOperationResult> ConfirmEmailAsync(Guid userId, string token);
    Task<bool> IsEmailConfirmedAsync(Guid userId);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<Guid?> FindUserIdByEmailAsync(string email);

    // Login externo (Google, etc.): encontra o utilizador pelo email ou cria um novo,
    // já com o email confirmado (o provedor externo já garantiu a posse do email).
    Task<Guid> FindOrCreateExternalUserAsync(string email, string firstName, string lastName);
}
