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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Family>(entity =>
        {
            entity.HasMany(f => f.Members).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId);
            entity.HasMany(f => f.Children).WithOne(c => c.Family).HasForeignKey(c => c.FamilyId);
            entity.HasMany(f => f.CalendarEvents).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId);
            entity.HasMany(f => f.ChatMessages).WithOne(m => m.Family).HasForeignKey(m => m.FamilyId);
            entity.HasMany(f => f.Expenses).WithOne(e => e.Family).HasForeignKey(e => e.FamilyId);
        });

        builder.Entity<Expense>().Property(e => e.Amount).HasPrecision(10, 2);
    }
}
