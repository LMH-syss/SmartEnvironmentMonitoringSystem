namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class EnvironmentSnapshot
{
    public required string TempHumiditySensorId { get; init; }

    public required string AirQualitySensorId { get; init; }

    public double Temperature { get; init; }

    public double Humidity { get; init; }

    public int SmokePpm { get; init; }

    public int Co2Ppm { get; init; }

    public DateTime ReceivedAt { get; init; }
}
