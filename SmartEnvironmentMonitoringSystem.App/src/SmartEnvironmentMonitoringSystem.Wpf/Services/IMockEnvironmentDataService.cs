using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IMockEnvironmentDataService
{
    event EventHandler<EnvironmentSnapshot>? SnapshotGenerated;

    bool IsRunning { get; }

    void Start();

    void Stop();
}
