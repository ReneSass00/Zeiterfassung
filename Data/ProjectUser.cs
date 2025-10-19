using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeiterfassung.Data
{
    public class ProjectUser
    {
        public const string RoleManager = "Manager";
        public const string RoleMember = "Member";

        public int ProjectId { get; set; }
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; } = RoleMember; 

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}