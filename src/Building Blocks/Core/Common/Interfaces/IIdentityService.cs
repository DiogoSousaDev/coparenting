using CoParenting.BuildingBlocks.Core.Common.Models;

namespace CoParenting.BuildingBlocks.Core.Common.Interfaces;

// Abstrai o UserManager<ApplicationUser> para que os Services não precisem
// de referenciar diretamente os tipos concretos do Identity.
public interface IIdentityService
{
    Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(string email, string password, string firstName, string lastName);
    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);
    Task<IdentityOperationResult> ConfirmEmailAsync(Guid userId, string token);
    Task<bool> IsEmailConfirmedAsync(Guid userId);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<Guid?> FindUserIdByEmailAsync(string email);
    Task<string?> FindEmailByUserIdAsync(Guid userId);

    // Login externo (Google, etc.): encontra o utilizador pelo email ou cria um novo,
    // já com o email confirmado (o provedor externo já garantiu a posse do email).
    Task<Guid> FindOrCreateExternalUserAsync(string email, string firstName, string lastName);
}
