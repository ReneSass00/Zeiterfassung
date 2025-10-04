// Services/TimerStateService.cs
namespace Zeiterfassung.Services;

public class TimerStateService : IDisposable
{
    private System.Threading.Timer? _timer;

    public bool IsRunning { get; private set; }
    public DateTime? StartTime { get; private set; }
    public int? SelectedProjectId { get; private set; }
    public string? SelectedProjectName { get; private set; }

    // Event, um Komponenten über Änderungen zu informieren
    public event Action? OnChange;

    public void StartTimer(int projectId, string projectName)
    {
        if (IsRunning) return;

        SelectedProjectId = projectId;
        SelectedProjectName = projectName;
        StartTime = DateTime.UtcNow;
        IsRunning = true;
        // Timer nur zur Benachrichtigung, die eigentliche Zeit wird aus StartTime berechnet
        _timer = new System.Threading.Timer(_ => NotifyStateChanged(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        NotifyStateChanged();
    }

    public (DateTime? Start, int? ProjectId) StopTimer()
    {
        if (!IsRunning) return (null, null);

        _timer?.Dispose();
        _timer = null;
        IsRunning = false;

        var result = (StartTime, SelectedProjectId);

        // Zustand zurücksetzen
        StartTime = null;
        SelectedProjectId = null;
        SelectedProjectName = null;

        NotifyStateChanged();
        return result;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _timer?.Dispose();
    }
}