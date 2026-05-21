namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class HistoryDeleteOptions
{
    public bool DeleteTempHumidity { get; set; }

    public bool DeleteAirQuality { get; set; }

    public bool DeleteAlarms { get; set; }

    public bool DeleteReports { get; set; }
}
