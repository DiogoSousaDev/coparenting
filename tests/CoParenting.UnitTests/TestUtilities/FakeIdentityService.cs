using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;

namespace CoParenting.UnitTests.TestUtilities;

public class FakeIdentityService : IIdentityService
{
    private record UserRecord(Guid Id, string Email, string Password, bool Confirmed);

    private readonly List<UserRecord> _users = [];
    public const string ConfirmationToken = "fake-confirmation-token";

    public Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(string email, string password, string firstName, string lastName)
    {
        if (_users.Any(u => u.Email == email))
        {
            return Task.FromResult((IdentityOperationResult.Failure("Email já registado."), Guid.Empty));
        }

        var id = Guid.NewGuid();
        _users.Add(new UserRecord(id, email, password, Confirmed: false));
        return Task.FromResult((IdentityOperationResult.Success(), id));
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(Guid userId) => Task.FromResult(ConfirmationToken);

    public Task<IdentityOperationResult> ConfirmEmailAsync(Guid userId, string token)
    {
        var index = _users.FindIndex(u => u.Id == userId);
        if (index < 0)
        {
            return Task.FromResult(IdentityOperationResult.Failure("Utilizador não encontrado."));
        }

        if (token != ConfirmationToken)
        {
            return Task.FromResult(IdentityOperationResult.Failure("Token inválido."));
        }

        _users[index] = _users[index] with { Confirmed = true };
        return Task.FromResult(IdentityOperationResult.Success());
    }

    public Task<bool> IsEmailConfirmedAsync(Guid userId) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == userId)?.Confirmed ?? false);

    public Task<bool> CheckPasswordAsync(Guid userId, string password) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == userId)?.Password == password);

    public Task<Guid?> FindUserIdByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email)?.Id);

    public Task<string?> FindEmailByUserIdAsync(Guid userId) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == userId)?.Email);

    public Task<Guid> FindOrCreateExternalUserAsync(string email, string firstName, string lastName)
    {
        var existing = _users.FirstOrDefault(u => u.Email == email);
        if (existing is not null)
        {
            return Task.FromResult(existing.Id);
        }

        var id = Guid.NewGuid();
        _users.Add(new UserRecord(id, email, Password: string.Empty, Confirmed: true));
        return Task.FromResult(id);
    }
}
