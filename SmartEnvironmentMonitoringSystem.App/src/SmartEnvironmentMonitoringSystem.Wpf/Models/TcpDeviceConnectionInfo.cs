namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class TcpDeviceConnectionInfo
{
    public required string ConnectionId { get; init; }

    public required string DeviceId { get; init; }

    public required string RemoteEndPoint { get; init; }

    public required string Status { get; init; }

    public DateTime ConnectedAt { get; init; }

    public DateTime LastSeenAt { get; init; }

    public required string LastMessageType { get; init; }
}
