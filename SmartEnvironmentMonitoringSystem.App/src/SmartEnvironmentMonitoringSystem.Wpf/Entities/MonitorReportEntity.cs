using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class MonitorReportEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string ReportNo { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public double MaxTemperature { get; set; }

    public double MinTemperature { get; set; }

    public double AvgTemperature { get; set; }

    public double MaxHumidity { get; set; }

    public double MinHumidity { get; set; }

    public double AvgHumidity { get; set; }

    public int MaxSmokePpm { get; set; }

    public double AvgSmokePpm { get; set; }

    public int MaxCo2Ppm { get; set; }

    public double AvgCo2Ppm { get; set; }

    public int AlarmCount { get; set; }

    public int DangerAlarmCount { get; set; }

    public string Evaluation { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
