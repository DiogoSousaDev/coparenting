using Microsoft.EntityFrameworkCore;

namespace CoParenting.Services.Families;

public class FamilyAccessService(IFamiliesDbContext dbContext) : IFamilyAccessService
{
    public Task<bool> IsMemberAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default) =>
        dbContext.FamilyMembers.AnyAsync(m => m.UserId == userId && m.FamilyId == familyId, cancellationToken);
}
