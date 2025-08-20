using Microsoft.EntityFrameworkCore;
using Flowganized.Models;

namespace Flowganized.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    public DbSet<ResetToken> ResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<UserOrganization> UserOrganizations { get; set; }
    public DbSet<InviteLink> InviteLinks { get; set; }
    public DbSet<Hierarchy> Hierarchies { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<ProjectLevel> ProjectLevels { get; set; }
    public DbSet<ProjectScale> ProjectScales { get; set; }
    public DbSet<ProjectType> ProjectTypes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserOrganization>()
            .Property(u => u.Position)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Page>()
            .Property(p => p.Props)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Role>()
            .Property(r => r.AccessPage)
            .HasColumnType("jsonb");
    }
}

