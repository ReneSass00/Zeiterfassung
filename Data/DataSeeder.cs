// Data/DataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zeiterfassung.Data;
using System.Security.Claims;

namespace Zeiterfassung.Data;

public static class DataSeeder
{
    public static async Task InitializeAsync(ZeiterfassungContext context, UserManager<User> userManager)
    {
        // Stellt sicher, dass die Datenbank erstellt ist.
        await context.Database.EnsureCreatedAsync();

        // Wenn bereits User in der DB sind, machen wir nichts.
        var users = await userManager.Users.ToListAsync();
        if (users.Select(x => x.Email?.Contains("example")).Any())
        {
            return;
        }

        // --- Gast-Benutzer erstellen ---
        var guestUser = new User { UserName = "guest@example.com", Email = "guest@example.com", EmailConfirmed = true };
        await userManager.CreateAsync(guestUser, "GuestPassword123!");

        // --- Weitere Muster-Benutzer erstellen ---
        var user1 = new User { UserName = "alice@example.com", Email = "alice@example.com", EmailConfirmed = true };
        var user2 = new User { UserName = "bob@example.com", Email = "bob@example.com", EmailConfirmed = true };
        await userManager.CreateAsync(user1, "Password123!");
        await userManager.CreateAsync(user2, "Password123!");

        // --- Projekte erstellen ---
        var project1 = new Project { Name = "Firmen-Website Relaunch", Description = "Neugestaltung der öffentlichen Website.", OwnerId = guestUser.Id };
        var project2 = new Project { Name = "Mobiles CRM-Tool", Description = "Entwicklung einer App für den Vertrieb.", OwnerId = user1.Id };
        var project3 = new Project { Name = "Cloud-Migration", Description = "Migration der internen Server zu Azure.", OwnerId = guestUser.Id };

        await context.Projects.AddRangeAsync(project1, project2, project3);
        await context.SaveChangesAsync();

        // --- Team-Mitgliedschaften zuweisen ---
        // Gast ist Owner von P1 & P3, und Mitglied in P2
        var membership1 = new ProjectUser { ProjectId = project2.Id, UserId = guestUser.Id };
        // Alice ist Owner von P2 und Mitglied in P3
        var membership2 = new ProjectUser { ProjectId = project3.Id, UserId = user1.Id };
        // Bob ist Mitglied in allen Projekten
        var membership3 = new ProjectUser { ProjectId = project1.Id, UserId = user2.Id };
        var membership4 = new ProjectUser { ProjectId = project2.Id, UserId = user2.Id };
        var membership5 = new ProjectUser { ProjectId = project3.Id, UserId = user2.Id };

        await context.ProjectUsers.AddRangeAsync(membership1, membership2, membership3, membership4, membership5);
        await context.SaveChangesAsync();


        // --- Zeiteinträge für den Gast-Benutzer generieren ---
        var random = new Random();
        var timeEntries = new List<TimeEntry>();
        var projectsForGuest = new[] { project1, project2, project3 }; // Gast hat Zugriff auf alle

        for (int i = 0; i < 90; i++) // Für die letzten 90 Tage
        {
            // Nicht jeden Tag einen Eintrag erstellen, um es realistischer zu machen
            if (random.Next(0, 10) < 3) continue;

            var date = DateTime.UtcNow.AddDays(-i);
            var numberOfEntries = random.Next(1, 4); // 1-3 Einträge pro Tag

            for (int j = 0; j < numberOfEntries; j++)
            {
                var startHour = random.Next(8, 15);
                var durationMinutes = random.Next(30, 240);
                var startTime = new DateTime(date.Year, date.Month, date.Day, startHour, random.Next(0, 60), 0, DateTimeKind.Utc);
                var endTime = startTime.AddMinutes(durationMinutes);

                timeEntries.Add(new TimeEntry
                {
                    Description = "Beispiel-Aufgabe " + random.Next(1, 100),
                    StartTime = startTime,
                    EndTime = endTime,
                    ProjectId = projectsForGuest[random.Next(0, projectsForGuest.Length)].Id,
                    UserId = guestUser.Id
                });
            }
        }
        await context.TimeEntries.AddRangeAsync(timeEntries);
        await context.SaveChangesAsync();
    }
}