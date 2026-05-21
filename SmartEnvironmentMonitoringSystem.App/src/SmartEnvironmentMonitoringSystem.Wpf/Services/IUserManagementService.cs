using SmartEnvironmentMonitoringSystem.Wpf.Entities;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IUserManagementService
{
    Task<List<UserEntity>> QueryUsersAsync();

    Task SetUserEnabledAsync(long userId, bool isEnabled);

    Task SetUserRoleAsync(long userId, string role);

    Task ResetPasswordAsync(long userId, string newPassword);
}
