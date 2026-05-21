using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Infrastructure;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IFreeSql database;

    public UserManagementService(IFreeSql database)
    {
        this.database = database;
    }

    public Task<List<UserEntity>> QueryUsersAsync()
    {
        return database.Select<UserEntity>()
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync();
    }

    public Task SetUserEnabledAsync(long userId, bool isEnabled)
    {
        return database.Update<UserEntity>()
            .Set(user => user.IsEnabled, isEnabled)
            .Where(user => user.Id == userId)
            .ExecuteAffrowsAsync();
    }

    public Task SetUserRoleAsync(long userId, string role)
    {
        return database.Update<UserEntity>()
            .Set(user => user.Role, role)
            .Where(user => user.Id == userId)
            .ExecuteAffrowsAsync();
    }

    public Task ResetPasswordAsync(long userId, string newPassword)
    {
        var salt = PasswordHasher.CreateSalt();
        var passwordHash = PasswordHasher.HashPassword(newPassword, salt);

        return database.Update<UserEntity>()
            .Set(user => user.Salt, salt)
            .Set(user => user.PasswordHash, passwordHash)
            .Where(user => user.Id == userId)
            .ExecuteAffrowsAsync();
    }
}
