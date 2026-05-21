namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class ThresholdSettings
{
    public double TemperatureMin { get; set; }

    public double TemperatureMax { get; set; }

    public double HumidityMin { get; set; }

    public double HumidityMax { get; set; }

    public double SmokeMax { get; set; }

    public double Co2Max { get; set; }
}
