namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class TempHumidityData
{
    public required string SensorId { get; init; }

    public double Temperature { get; init; }

    public double Humidity { get; init; }

    public DateTime CollectTime { get; init; }

    public DateTime ReceiveTime { get; init; }
}
