using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface ICommunicationSettingsService
{
    Task<CommunicationSettings> LoadAsync();

    Task SaveAsync(CommunicationSettings settings);
}
