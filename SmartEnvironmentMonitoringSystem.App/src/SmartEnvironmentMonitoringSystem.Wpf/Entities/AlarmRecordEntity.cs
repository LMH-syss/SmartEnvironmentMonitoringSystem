using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class AlarmRecordEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SensorId { get; set; } = string.Empty;

    public string AlarmType { get; set; } = string.Empty;

    public double ActualValue { get; set; }

    public string ThresholdDescription { get; set; } = string.Empty;

    public string AlarmLevel { get; set; } = "WARN";

    public DateTime AlarmTime { get; set; }

    public bool IsHandled { get; set; }

    public string? HandledBy { get; set; }

    public DateTime? HandledAt { get; set; }

    public string? Remark { get; set; }
}
