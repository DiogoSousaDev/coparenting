using CoParenting.Domain.Entities;

namespace CoParenting.Application.Families;

public interface IFamilyService
{
    Task<CreateFamilyResult> CreateFamilyAsync(Guid userId, string familyName, FamilyRole creatorRole, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilySummary>> GetMyFamiliesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InviteResult> CreateInviteAsync(Guid inviterUserId, Guid familyId, string inviteeEmail, FamilyRole inviteeRole, CancellationToken cancellationToken = default);
    Task<InviteDetailsResult> GetInviteDetailsAsync(string rawToken, CancellationToken cancellationToken = default);
    Task<AcceptInviteResult> AcceptInviteAsync(Guid userId, string userEmail, string rawToken, CancellationToken cancellationToken = default);
}
