using FreeSql.DataAnnotations;

namespace SmartEnvironmentMonitoringSystem.Wpf.Entities;

public sealed class SensorEntity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SensorId { get; set; } = string.Empty;

    public string SensorName { get; set; } = string.Empty;

    public string SensorType { get; set; } = string.Empty;

    public string CommunicationType { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
