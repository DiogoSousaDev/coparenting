namespace CoParenting.Application.Common.Interfaces;

// Usado por todas as features que operam sobre dados de uma família (calendário, chat, despesas)
// para garantir que só membros dessa família lhes acedem.
public interface IFamilyAccessService
{
    Task<bool> IsMemberAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default);
}
