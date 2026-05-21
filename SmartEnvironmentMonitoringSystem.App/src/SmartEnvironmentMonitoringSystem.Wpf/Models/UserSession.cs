namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class UserSession
{
    public required string Username { get; init; }

    public required string Role { get; init; }

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
}
