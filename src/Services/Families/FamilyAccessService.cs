using CoParenting.BuildingBlocks.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Services.Families;

public class FamilyAccessService(IFamiliesDbContext dbContext) : IFamilyAccessService
{
    public Task<bool> IsMemberAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default) =>
        dbContext.FamilyMembers.AnyAsync(m => m.UserId == userId && m.FamilyId == familyId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetMemberUserIdsAsync(Guid familyId, CancellationToken cancellationToken = default) =>
        await dbContext.FamilyMembers.Where(m => m.FamilyId == familyId).Select(m => m.UserId).ToListAsync(cancellationToken);
}
