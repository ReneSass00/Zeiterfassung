// Data/ZeiterfassungContext.cs

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

    // DIESE METHODE HINZUFÜGEN
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); 


        builder.Entity<TimeEntry>()
            .HasOne(te => te.User)
            .WithMany() 
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.NoAction); 
    }
}