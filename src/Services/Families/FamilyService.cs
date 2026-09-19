using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using CoParenting.BuildingBlocks.Core.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoParenting.Services.Families;

public class FamilyService(
    IFamiliesDbContext dbContext,
    IFamilyAccessService familyAccessService,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions,
    ILogger<FamilyService> logger) : IFamilyService
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);
    private const int MaxFamilyMembers = 2;

    public async Task<CreateFamilyResult> CreateFamilyAsync(Guid userId, string familyName, FamilyRole creatorRole, CancellationToken cancellationToken = default)
    {
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = familyName,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Families.Add(family);
        dbContext.FamilyMembers.Add(new FamilyMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = family.Id,
            Role = creatorRole
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateFamilyResult.Success(family.Id);
    }

    public async Task<IReadOnlyList<FamilySummary>> GetMyFamiliesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.FamilyMembers
            .Where(m => m.UserId == userId)
            .Select(m => new FamilySummary(m.FamilyId, m.Family.Name, m.Role))
            .ToListAsync(cancellationToken);

    public async Task<InviteResult> CreateInviteAsync(Guid inviterUserId, Guid familyId, string inviteeEmail, FamilyRole inviteeRole, CancellationToken cancellationToken = default)
    {
        if (!await familyAccessService.IsMemberAsync(inviterUserId, familyId, cancellationToken))
        {
            return InviteResult.Failure("Não tens acesso a esta família.");
        }

        var memberCount = await dbContext.FamilyMembers.CountAsync(m => m.FamilyId == familyId, cancellationToken);
        if (memberCount >= MaxFamilyMembers)
        {
            return InviteResult.Failure("Esta família já tem o número máximo de membros.");
        }

        var family = await dbContext.Families.FirstAsync(f => f.Id == familyId, cancellationToken);

        var rawToken = InviteTokenHasher.GenerateRawToken();
        var expiresAtUtc = DateTime.UtcNow.Add(InviteLifetime);

        dbContext.FamilyInvites.Add(new FamilyInvite
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            InvitedEmail = inviteeEmail,
            Role = inviteeRole,
            TokenHash = InviteTokenHasher.Hash(rawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var acceptLink = $"{appOptions.Value.ClientBaseUrl}/accept-invite/{rawToken}";
        string? warning = null;
        try
        {
            await emailSender.SendAsync(
                inviteeEmail,
                $"Convite para a família {family.Name} — CoParenting",
                $"Foste convidado(a) para a unidade familiar \"{family.Name}\" no CoParenting. Aceita o convite <a href=\"{acceptLink}\">aqui</a>.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            // O convite já foi criado (e é válido) mesmo que o envio do email falhe —
            // uma falha temporária do provedor de email não deve bloquear o fluxo de convite.
            logger.LogWarning(ex, "Falha ao enviar email de convite para {Email}", inviteeEmail);
            warning = "Convite criado, mas não foi possível enviar o email. Partilha o link manualmente.";
        }

        return InviteResult.Success(expiresAtUtc, rawToken, warning);
    }

    public async Task<InviteDetailsResult> GetInviteDetailsAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokenHasher.Hash(rawToken);
        var invite = await dbContext.FamilyInvites
            .Include(i => i.Family)
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);

        if (invite is null)
        {
            return new InviteDetailsResult(false, null, null, false, false);
        }

        return new InviteDetailsResult(
            Found: true,
            FamilyName: invite.Family.Name,
            InvitedEmail: invite.InvitedEmail,
            Expired: invite.ExpiresAt < DateTime.UtcNow,
            AlreadyAccepted: invite.AcceptedAt is not null);
    }

    public async Task<AcceptInviteResult> AcceptInviteAsync(Guid userId, string userEmail, string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokenHasher.Hash(rawToken);
        var invite = await dbContext.FamilyInvites.FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);

        if (invite is null)
        {
            return AcceptInviteResult.Failure("Convite não encontrado.");
        }

        if (invite.AcceptedAt is not null)
        {
            return AcceptInviteResult.Failure("Este convite já foi aceite.");
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            return AcceptInviteResult.Failure("Este convite expirou.");
        }

        if (!string.Equals(invite.InvitedEmail, userEmail, StringComparison.OrdinalIgnoreCase))
        {
            return AcceptInviteResult.Failure("Este convite foi enviado para outro email.");
        }

        if (await familyAccessService.IsMemberAsync(userId, invite.FamilyId, cancellationToken))
        {
            return AcceptInviteResult.Failure("Já és membro desta família.");
        }

        dbContext.FamilyMembers.Add(new FamilyMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = invite.FamilyId,
            Role = invite.Role
        });
        invite.AcceptedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return AcceptInviteResult.Success(invite.FamilyId);
    }
}
