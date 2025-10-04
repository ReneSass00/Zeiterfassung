using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Zeiterfassung.Data;

public class QuickStartTemplate
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    public User User { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } // Text auf dem Button, z.B. "Team-Meeting"

    [MaxLength(500)]
    public string Description { get; set; } // Vorgefüllte Beschreibung
}