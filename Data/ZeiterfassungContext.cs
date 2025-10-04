// Data/ZeiterfassungContext.cs

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

        // Bestehende Konfiguration für TimeEntry
        builder.Entity<TimeEntry>()
            .HasOne(te => te.User)
            .WithMany()
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Bestehende Konfiguration für die ProjectUser-Join-Tabelle
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

        // NEU: Wir unterbrechen die Kaskade vom User (Owner) zum Projekt.
        // Das verhindert das automatische Löschen von Projekten, wenn der Owner gelöscht wird.
        builder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany() // Ein User kann Owner von vielen Projekten sein
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // Oder .NoAction. Verhindert das Löschen des Users.




    }



}