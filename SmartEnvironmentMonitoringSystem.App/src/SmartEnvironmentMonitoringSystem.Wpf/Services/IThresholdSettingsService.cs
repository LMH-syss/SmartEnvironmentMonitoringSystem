using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IThresholdSettingsService
{
    Task<ThresholdSettings> LoadAsync();

    Task SaveAsync(ThresholdSettings settings);
}
