using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class UserEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Salt { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsEnabled { get; set; } = true;
}
