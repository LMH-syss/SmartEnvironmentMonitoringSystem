using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IHistoryService
{
    Task<List<TempHumidityRecordEntity>> QueryTempHumidityAsync(DateTime startTime, DateTime endTime, string? sensorId);

    Task<List<AirQualityRecordEntity>> QueryAirQualityAsync(DateTime startTime, DateTime endTime, string? sensorId);

    Task<List<AlarmRecordEntity>> QueryAlarmsAsync(DateTime startTime, DateTime endTime, string? sensorId);

    Task<HistoryDeleteResult> DeleteHistoryAsync(DateTime startTime, DateTime endTime, string? sensorId, HistoryDeleteOptions options);
}
