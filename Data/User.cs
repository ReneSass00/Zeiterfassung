using Microsoft.AspNetCore.Identity;



namespace Zeiterfassung.Data
{
    public class User : IdentityUser
    {
        public bool IsSampleUser { get; set; } = false;
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
    }
}