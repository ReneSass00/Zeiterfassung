using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zeiterfassung.Data;

public class ZeiterfassungContext : IdentityDbContext<User>
{
    public ZeiterfassungContext(DbContextOptions<ZeiterfassungContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<QuickStartTemplate> QuickStartTemplates { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TimeEntry>()
            .HasOne(te => te.User)
            .WithMany()
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ProjectUser>()
            .HasKey(pu => new { pu.ProjectId, pu.UserId });

        builder.Entity<ProjectUser>()
            .HasOne(pu => pu.Project)
            .WithMany(p => p.ProjectUsers)
            .HasForeignKey(pu => pu.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectUser>()
            .HasOne(pu => pu.User)
            .WithMany(u => u.ProjectUsers)
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany() 
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); 



    }



}