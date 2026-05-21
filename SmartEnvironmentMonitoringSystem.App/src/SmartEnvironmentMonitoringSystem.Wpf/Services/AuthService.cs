using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Infrastructure;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class AuthService : IAuthService
{
    private readonly IFreeSql database;

    public AuthService(IFreeSql database)
    {
        this.database = database;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failed("请输入用户名和密码。");
        }

        var user = await database.Select<UserEntity>()
            .Where(item => item.Username == normalizedUsername)
            .FirstAsync();

        if (user is null || !user.IsEnabled)
        {
            return AuthResult.Failed("用户不存在或已被禁用。");
        }

        if (!PasswordHasher.Verify(password, user.Salt, user.PasswordHash))
        {
            return AuthResult.Failed("用户名或密码不正确。");
        }

        user.LastLoginAt = DateTime.Now;
        await database.Update<UserEntity>()
            .Set(item => item.LastLoginAt, user.LastLoginAt)
            .Where(item => item.Id == user.Id)
            .ExecuteAffrowsAsync();

        return AuthResult.Success(new UserSession
        {
            Username = user.Username,
            Role = user.Role
        });
    }

    public async Task<AuthResult> RegisterUserAsync(string username, string password)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (normalizedUsername.Length < 3)
        {
            return AuthResult.Failed("用户名至少需要 3 个字符。");
        }

        if (password.Length < 6)
        {
            return AuthResult.Failed("密码至少需要 6 个字符。");
        }

        var exists = await database.Select<UserEntity>()
            .Where(item => item.Username == normalizedUsername)
            .AnyAsync();

        if (exists)
        {
            return AuthResult.Failed("用户名已存在。");
        }

        var salt = PasswordHasher.CreateSalt();
        var user = new UserEntity
        {
            Username = normalizedUsername,
            Salt = salt,
            PasswordHash = PasswordHasher.HashPassword(password, salt),
            Role = "User",
            CreatedAt = DateTime.Now,
            IsEnabled = true
        };

        await database.Insert(user).ExecuteAffrowsAsync();

        return AuthResult.Success(new UserSession
        {
            Username = user.Username,
            Role = user.Role
        });
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim();
    }
}
