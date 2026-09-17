using CoParenting.Application.Common.Interfaces;
using CoParenting.Domain.Entities;
using CoParenting.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<FamilyInvite> FamilyInvites => Set<FamilyInvite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Family>(entity =>
        {
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(f => f.Members).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId);
            entity.HasMany(f => f.Children).WithOne(c => c.Family).HasForeignKey(c => c.FamilyId);
            entity.HasMany(f => f.CalendarEvents).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId);
            entity.HasMany(f => f.ChatMessages).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId);
            entity.HasMany(f => f.Expenses).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId);
            entity.HasMany(f => f.Invites).WithOne(i => i.Family).HasForeignKey(i => i.FamilyId);
        });

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
