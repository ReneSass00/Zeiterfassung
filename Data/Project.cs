
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeiterfassung.Data
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual User User { get; set; }

        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }

        public virtual ICollection<TimeEntry> TimeEntries { get; set; }

    }
}