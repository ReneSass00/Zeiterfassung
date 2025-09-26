using System;
using System.ComponentModel.DataAnnotations;
namespace Zeiterfassung.Models
{

    public class TimeEntry
    {
        [Key]
        public int Id { get; set; } // Primärschlüssel (wird automatisch als PK erkannt)
        public string? UserId { get; set; } // Verweis auf den Benutzer
        public DateTime StartTime { get; set; } // Startzeit des Eintrags
        public DateTime EndTime { get; set; } // Endzeit des Eintrags
        public string? TaskDescription { get; set; } // Beschreibung der Aufgabe
    }

}
