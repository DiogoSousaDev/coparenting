using CoParenting.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Application.Families;

public class FamilyAccessService(IApplicationDbContext dbContext) : IFamilyAccessService
{
    public Task<bool> IsMemberAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default) =>
        dbContext.FamilyMembers.AnyAsync(m => m.UserId == userId && m.FamilyId == familyId, cancellationToken);
}
