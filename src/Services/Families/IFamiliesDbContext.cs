using Microsoft.EntityFrameworkCore;

namespace CoParenting.Services.Families;

public interface IFamiliesDbContext
{
    DbSet<Family> Families { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<Child> Children { get; }
    DbSet<FamilyInvite> FamilyInvites { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
