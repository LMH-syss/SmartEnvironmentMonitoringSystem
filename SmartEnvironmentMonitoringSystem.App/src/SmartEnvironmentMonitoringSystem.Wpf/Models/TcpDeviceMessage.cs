namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class TcpDeviceMessage
{
    public required string Type { get; init; }

    public required string DeviceId { get; init; }

    public string Status { get; init; } = string.Empty;

    public TempHumidityData? TempHumidity { get; init; }
}
