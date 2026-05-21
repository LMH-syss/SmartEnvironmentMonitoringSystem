namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class HistoryDeleteResult
{
    public int TempHumidityCount { get; set; }

    public int AirQualityCount { get; set; }

    public int AlarmCount { get; set; }

    public int ReportCount { get; set; }

    public int TotalCount => TempHumidityCount + AirQualityCount + AlarmCount + ReportCount;
}
