using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);

    Task<AuthResult> RegisterUserAsync(string username, string password);
}
