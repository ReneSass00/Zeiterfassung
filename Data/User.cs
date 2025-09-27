using Microsoft.AspNetCore.Identity;



namespace Zeiterfassung.Data
{
    public class User : IdentityUser
    {
        // Dein Flag, um Muster-User zu identifizieren
        public bool IsSampleUser { get; set; } = false;

        // Navigationseigenschaft zur Verknüpfungstabelle
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
    }
}