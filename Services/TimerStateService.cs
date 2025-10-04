// Services/TimerStateService.cs
namespace Zeiterfassung.Services;

public class TimerStateService : IDisposable
{
    private System.Threading.Timer? _timer;
    private TimeSpan _elapsedBeforePause = TimeSpan.Zero; // NEU: Speichert die bereits vergangene Zeit vor einer Pause

    public bool IsRunning { get; private set; } // Bedeutet: Die Uhr tickt aktiv
    public bool IsPaused { get; private set; } // NEU: Ist der Timer pausiert?
    public DateTime? StartTime { get; private set; } // Der Zeitpunkt, an dem die Uhr (wieder) zu ticken begann
    public int? SelectedProjectId { get; private set; }
    public string? SelectedProjectName { get; private set; }
    public string? Description { get; private set; }

    public event Action? OnChange;

    // NEU: Eine Hilfsmethode, um die korrekte Gesamtzeit zu berechnen
    public TimeSpan GetElapsedTime()
    {
        if (IsRunning && StartTime.HasValue)
        {
            return _elapsedBeforePause + (DateTime.UtcNow - StartTime.Value);
        }
        return _elapsedBeforePause;
    }

    public void StartTimer(int projectId, string projectName, string description)
    {
        if (IsRunning || IsPaused) return;

        SelectedProjectId = projectId;
        SelectedProjectName = projectName;
        Description = description;
        _elapsedBeforePause = TimeSpan.Zero; // Zurücksetzen für einen neuen Timer
        StartTime = DateTime.UtcNow;
        IsRunning = true;
        IsPaused = false;

        _timer = new System.Threading.Timer(_ => NotifyStateChanged(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        NotifyStateChanged();
    }

    // NEUE METHODE zum Pausieren
    public void PauseTimer()
    {
        if (!IsRunning) return;

        _timer?.Dispose();
        _timer = null;
        _elapsedBeforePause += (DateTime.UtcNow - StartTime!.Value); // Vergangene Zeit zum Puffer addieren
        IsRunning = false;
        IsPaused = true;
        StartTime = null; // Die Uhr stoppen

        NotifyStateChanged();
    }

    // NEUE METHODE zum Fortsetzen
    public void ResumeTimer()
    {
        if (!IsPaused) return;

        StartTime = DateTime.UtcNow; // Die Uhr neu starten
        IsRunning = true;
        IsPaused = false;

        _timer = new System.Threading.Timer(_ => NotifyStateChanged(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        NotifyStateChanged();
    }

    // GEÄNDERT: Gibt jetzt Start- und Endzeit zurück für eine korrekte Speicherung
    public (DateTime? Start, DateTime? End, int? ProjectId, string? Description) StopTimer()
    {
        if (!IsRunning && !IsPaused) return (null, null, null, null);

        _timer?.Dispose();
        _timer = null;

        var endTime = DateTime.UtcNow;
        var totalDuration = GetElapsedTime();
        // Der "effektive" Startzeitpunkt wird aus der Endzeit und der totalen Dauer berechnet
        var effectiveStartTime = endTime - totalDuration;

        var result = (effectiveStartTime, endTime, SelectedProjectId, Description);

        // Kompletter Reset des Zustands
        IsRunning = false;
        IsPaused = false;
        StartTime = null;
        SelectedProjectId = null;
        SelectedProjectName = null;
        Description = null;
        _elapsedBeforePause = TimeSpan.Zero;

        NotifyStateChanged();
        return result;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _timer?.Dispose();
    }
}