using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Zeiterfassung.Data
{
    public static class SampleUserData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ZeiterfassungContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Stellt sicher, dass die Datenbank existiert und alle Migrationen angewendet wurden.
            await context.Database.MigrateAsync();

            // Prüfen, ob bereits Muster-User existieren
            if (await userManager.Users.AnyAsync(u => u.IsSampleUser))
            {
                return; // Seeding wurde bereits durchgeführt
            }

            // Muster-User 1
            var user1 = new User
            {
                UserName = "anna.muster@email.com",
                Email = "anna.muster@email.com",
                EmailConfirmed = true,
                IsSampleUser = true
            };
            await userManager.CreateAsync(user1, "Passwort123!");

            // Muster-User 2
            var user2 = new User
            {
                UserName = "ben.beispiel@email.com",
                Email = "ben.beispiel@email.com",
                EmailConfirmed = true,
                IsSampleUser = true
            };
            await userManager.CreateAsync(user2, "Passwort123!");
        }
    }
}