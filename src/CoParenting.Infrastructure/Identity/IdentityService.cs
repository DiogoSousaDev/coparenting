using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace CoParenting.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        return result.Succeeded
            ? (IdentityOperationResult.Success(), user.Id)
            : (IdentityOperationResult.Failure([.. result.Errors.Select(e => e.Description)]), Guid.Empty);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityOperationResult> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityOperationResult.Failure("Utilizador não encontrado.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);

        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure([.. result.Errors.Select(e => e.Description)]);
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<Guid?> FindUserIdByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }
}
