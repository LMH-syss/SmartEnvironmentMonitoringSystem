using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class TempHumidityRecordEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SensorId { get; set; } = string.Empty;

    public double Temperature { get; set; }

    public double Humidity { get; set; }

    public DateTime CollectTime { get; set; }

    public DateTime ReceiveTime { get; set; }

    public bool IsAlarm { get; set; }
}
