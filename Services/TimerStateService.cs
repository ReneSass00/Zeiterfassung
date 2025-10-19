// Services/TimerStateService.cs
namespace Zeiterfassung.Services;

public class TimerStateService : IDisposable
{
    private System.Threading.Timer? _timer;
    private TimeSpan _elapsedBeforePause = TimeSpan.Zero;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; } 
    public DateTime? StartTime { get; private set; } 
    public int? SelectedProjectId { get; private set; }
    public string? SelectedProjectName { get; private set; }
    public string? Description { get; private set; }

    public event Action? OnChange;

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
        _elapsedBeforePause = TimeSpan.Zero; 
        StartTime = DateTime.UtcNow;
        IsRunning = true;
        IsPaused = false;

        _timer = new System.Threading.Timer(_ => NotifyStateChanged(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        NotifyStateChanged();
    }


    public void PauseTimer()
    {
        if (!IsRunning) return;

        _timer?.Dispose();
        _timer = null;
        _elapsedBeforePause += (DateTime.UtcNow - StartTime!.Value); 
        IsRunning = false;
        IsPaused = true;
        StartTime = null; 

        NotifyStateChanged();
    }

    public void ResumeTimer()
    {
        if (!IsPaused) return;

        StartTime = DateTime.UtcNow; 
        IsRunning = true;
        IsPaused = false;

        _timer = new System.Threading.Timer(_ => NotifyStateChanged(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        NotifyStateChanged();
    }

    public (DateTime? Start, DateTime? End, int? ProjectId, string? Description) StopTimer()
    {
        if (!IsRunning && !IsPaused) return (null, null, null, null);

        _timer?.Dispose();
        _timer = null;

        var endTime = DateTime.UtcNow;
        var totalDuration = GetElapsedTime();
        var effectiveStartTime = endTime - totalDuration;

        var result = (effectiveStartTime, endTime, SelectedProjectId, Description);

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