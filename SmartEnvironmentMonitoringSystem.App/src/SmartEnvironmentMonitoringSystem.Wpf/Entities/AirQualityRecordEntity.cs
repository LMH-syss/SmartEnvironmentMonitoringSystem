using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class AirQualityRecordEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SensorId { get; set; } = string.Empty;

    public int SmokePpm { get; set; }

    public int Co2Ppm { get; set; }

    public string Level { get; set; } = "NORMAL";

    public DateTime CollectTime { get; set; }

    public DateTime ReceiveTime { get; set; }

    public bool IsAlarm { get; set; }
}
