using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zeiterfassung.Data;

namespace Zeiterfassung.Data;

/// <summary>
/// Class to seed initial data into the database.
/// </summary>
public static class DataSeeder
{
    // Emails (users) to create
    private static readonly string[] SeederEmails = { "guest@example.com", "alice@example.com", "bob@example.com", "charlie@example.com", "dave@example.com" };

    public static async Task InitializeAsync(ZeiterfassungContext context, UserManager<User> userManager)
    {
        await context.Database.EnsureCreatedAsync();

        var random = new Random();

        var existingUsers = await userManager.Users.Where(u => SeederEmails.Contains(u.Email)).ToListAsync();

        // create users
        var guestUser = existingUsers.FirstOrDefault(u => u.Email == "guest@example.com");
        if (guestUser == null)
        {
            guestUser = new User { UserName = "guest@example.com", Email = "guest@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(guestUser, "GuestPassword123!");
        }

        var userAlice = existingUsers.FirstOrDefault(u => u.Email == "alice@example.com");
        if (userAlice == null)
        {
            userAlice = new User { UserName = "alice@example.com", Email = "alice@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(userAlice, "Password123!");
        }

        var userBob = existingUsers.FirstOrDefault(u => u.Email == "bob@example.com");
        if (userBob == null)
        {
            userBob = new User { UserName = "bob@example.com", Email = "bob@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(userBob, "Password123!");
        }

        var userCharlie = existingUsers.FirstOrDefault(u => u.Email == "charlie@example.com");
        if (userCharlie == null)
        {
            userCharlie = new User { UserName = "charlie@example.com", Email = "charlie@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(userCharlie, "Password123!");
        }

        var userDave = existingUsers.FirstOrDefault(u => u.Email == "dave@example.com");
        if (userDave == null)
        {
            userDave = new User { UserName = "dave@example.com", Email = "dave@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(userDave, "Password123!");
        }

        var usersToSeedTime = new[] { guestUser, userAlice, userBob, userCharlie, userDave };


        // --- create projects ---
        if (!await context.Projects.AnyAsync(p => p.Name.Contains("Website") || p.Name.Contains("Jahresplanung")))
        {
            var projectsToAdd = new List<Project>
            {
                new Project { Name = "Firmen-Website Relaunch", Description = "Neugestaltung der öffentlichen Website.", OwnerId = guestUser.Id },
                new Project { Name = "Strategische Jahresplanung", Description = "Definition von Zielen und Initiativen für das kommende Geschäftsjahr.", OwnerId = userAlice.Id },
                new Project { Name = "Mitarbeiter-Onboarding", Description = "Einarbeitung neuer Mitarbeiter in Prozesse und Tools (HR/Verwaltung).", OwnerId = userBob.Id },
                new Project { Name = "Monatliche Finanzberichterstattung", Description = "Erstellung und Analyse der Monatsabschlüsse und Berichte (Finanzen).", OwnerId = guestUser.Id },
                new Project { Name = "Urlaub", Description = "Bezahlter Jahresurlaub.", OwnerId = guestUser.Id },
                new Project { Name = "Feiertag", Description = "Gesetzliche Feiertage (z.B. Weihnachten, Ostern).", OwnerId = guestUser.Id },
                new Project { Name = "Krankheit", Description = "Krankheitsbedingte Abwesenheit.", OwnerId = guestUser.Id },
                new Project { Name = "Interne Schulung (Weiterbildung)", Description = "Teilnahme an internen/externen Schulungen.", OwnerId = userCharlie.Id },
                new Project { Name = "Support & Wartung", Description = "Regelmäßige Bugfixes und allgemeine Systemwartung.", OwnerId = userDave.Id },
            };

            await context.Projects.AddRangeAsync(projectsToAdd);
            await context.SaveChangesAsync();
        }

        
        var allProjects = await context.Projects.ToListAsync();
        var operationalProjects = allProjects.Where(p => p.Name != "Urlaub" && p.Name != "Feiertag" && p.Name != "Krankheit").ToArray();
        var nonOperationalProjects = allProjects.Where(p => p.Name == "Urlaub" || p.Name == "Feiertag" || p.Name == "Krankheit").ToArray();

        
        var existingMemberships = await context.ProjectUsers.ToListAsync();
        var newMemberships = new List<ProjectUser>();

        foreach (var user in usersToSeedTime)
        {
            foreach (var project in allProjects)
            {
                if (project.OwnerId != user.Id &&
                    !existingMemberships.Any(m => m.UserId == user.Id && m.ProjectId == project.Id))
                {
                    newMemberships.Add(new ProjectUser { ProjectId = project.Id, UserId = user.Id });
                }
            }
        }
        await context.ProjectUsers.AddRangeAsync(newMemberships);
        await context.SaveChangesAsync();

        if (!await context.TimeEntries.AnyAsync())
        {
            await GenerateInitialTimeEntries(context, random, usersToSeedTime, operationalProjects, nonOperationalProjects);
        }

        // Shift the time Entries of the users to current date, therefore the data never gets to old 
        await ShiftTimeEntries(context, guestUser);
        await ShiftTimeEntries(context, userAlice);
        await ShiftTimeEntries(context, userBob);
        await ShiftTimeEntries(context, userCharlie);
        await ShiftTimeEntries(context, userDave);
    }

    /// <summary>
    /// Generates initial time entries for users over the past year.
    /// </summary>
    private static async Task GenerateInitialTimeEntries(ZeiterfassungContext context, Random random, User[] usersToSeedTime, Project[] operationalProjects, Project[] nonOperationalProjects)
    {
        var timeEntries = new List<TimeEntry>();
        var totalDays = 365; 

        foreach (var user in usersToSeedTime)
        {
            for (int i = 0; i < totalDays; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i);
                var dayOfWeek = date.DayOfWeek;

                if (dayOfWeek == DayOfWeek.Sunday) continue;
                if (dayOfWeek == DayOfWeek.Saturday && random.Next(1, 51) != 1) continue; // 1:50 to work on saturday

                // chance of absence day (5%)
                if (random.Next(1, 20) == 1)
                {
                    var absenceProject = nonOperationalProjects[random.Next(0, nonOperationalProjects.Length)];
                    timeEntries.Add(new TimeEntry
                    {
                        Description = absenceProject.Name,
                        StartTime = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0, DateTimeKind.Utc),
                        EndTime = new DateTime(date.Year, date.Month, date.Day, 16, 0, 0, DateTimeKind.Utc).AddMinutes(random.Next(0, 30)),
                        ProjectId = absenceProject.Id,
                        UserId = user.Id
                    });
                    continue;
                }

                // regular workday
                var totalDurationMinutes = random.Next(6 * 60, 9 * 60);
                var remainingMinutes = totalDurationMinutes;
                var numberOfEntries = random.Next(1, 4);
                var startHour = random.Next(7, 10);
                var currentStartTime = new DateTime(date.Year, date.Month, date.Day, startHour, random.Next(0, 30), 0, DateTimeKind.Utc);

                for (int j = 0; j < numberOfEntries; j++)
                {
                    var maxDuration = remainingMinutes - (numberOfEntries - j - 1) * 30;
                    var durationMinutes = random.Next(30, maxDuration > 120 ? 120 : maxDuration);

                    if (j == numberOfEntries - 1)
                    {
                        durationMinutes = remainingMinutes;
                    }

                    var endTime = currentStartTime.AddMinutes(durationMinutes);
                    remainingMinutes -= durationMinutes;
                    var project = operationalProjects[random.Next(0, operationalProjects.Length)];

                    timeEntries.Add(new TimeEntry
                    {
                        Description = $"Aufgabe für {project.Name}",
                        StartTime = currentStartTime,
                        EndTime = endTime,
                        ProjectId = project.Id,
                        UserId = user.Id
                    });

                    currentStartTime = endTime.AddMinutes(random.Next(5, 16));
                }
            }
        }
        await context.TimeEntries.AddRangeAsync(timeEntries);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Shift time entries of a user to ensure the latest entry is at most yesterday.
    /// </summary>
    private static async Task ShiftTimeEntries(ZeiterfassungContext context, User guestUser)
    {
        var guestEntries = await context.TimeEntries
            .Where(te => te.UserId == guestUser.Id)
            .OrderBy(te => te.StartTime)
            .ToListAsync();

        if (!guestEntries.Any()) return;

        var lastEntryTime = guestEntries.Last().EndTime;

        if (lastEntryTime.Date < DateTime.UtcNow.Date.AddDays(-1))
        {
            var today = DateTime.UtcNow.Date;
            var timeSpanSinceLastEntry = today - lastEntryTime.Date.AddDays(1); 

            foreach (var entry in guestEntries)
            {
                entry.StartTime = entry.StartTime.Add(timeSpanSinceLastEntry);
                entry.EndTime = entry.EndTime.Add(timeSpanSinceLastEntry);
            }

            await context.SaveChangesAsync();
        }
    }
}