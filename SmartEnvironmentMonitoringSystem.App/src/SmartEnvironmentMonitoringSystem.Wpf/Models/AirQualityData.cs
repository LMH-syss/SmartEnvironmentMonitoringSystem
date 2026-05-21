namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class AirQualityData
{
    public required string SensorId { get; init; }

    public int SmokePpm { get; init; }

    public int Co2Ppm { get; init; }

    public required string Level { get; init; }

    public DateTime CollectTime { get; init; }

    public DateTime ReceiveTime { get; init; }
}
