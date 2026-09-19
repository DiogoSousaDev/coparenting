using CoParenting.BuildingBlocks.Core.Identity;
using CoParenting.Services.Calendar;
using CoParenting.Services.Chat;
using CoParenting.Services.Expenses;
using CoParenting.Services.Families;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Api.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IFamiliesDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<FamilyInvite> FamilyInvites => Set<FamilyInvite>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Family>(entity =>
        {
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(f => f.Members).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId);
            entity.HasMany(f => f.Children).WithOne(c => c.Family).HasForeignKey(c => c.FamilyId);
            entity.HasMany(f => f.Invites).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId);
        });

        // CalendarEvent/ChatMessage/Expense vivem em projetos Services separados de Families,
        // por isso não têm navegação para Family — a relação fica só pelo FamilyId (shadow FK).
        builder.Entity<CalendarEvent>().HasOne<Family>().WithMany().HasForeignKey(e => e.FamilyId);
        builder.Entity<ChatMessage>().HasOne<Family>().WithMany().HasForeignKey(m => m.FamilyId);
        builder.Entity<Expense>().HasOne<Family>().WithMany().HasForeignKey(e => e.FamilyId);

        builder.Entity<FamilyMember>().HasIndex(m => new { m.UserId, m.FamilyId }).IsUnique();

        builder.Entity<FamilyInvite>(entity =>
        {
            entity.Property(i => i.InvitedEmail).IsRequired().HasMaxLength(256);
            entity.Property(i => i.TokenHash).IsRequired().HasMaxLength(128);
            entity.HasIndex(i => i.TokenHash).IsUnique();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        });

        builder.Entity<Expense>().Property(e => e.Amount).HasPrecision(10, 2);
    }
}
