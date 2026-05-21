namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class TcpCommunicationLog
{
    public DateTime Time { get; init; }

    public required string DeviceId { get; init; }

    public required string RemoteEndPoint { get; init; }

    public required string EventType { get; init; }

    public required string Message { get; init; }
}
