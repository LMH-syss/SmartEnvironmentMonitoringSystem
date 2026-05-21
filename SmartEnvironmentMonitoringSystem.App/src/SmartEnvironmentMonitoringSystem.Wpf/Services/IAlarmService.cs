using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IAlarmService
{
    Task<List<AlarmRecordEntity>> CheckTempHumidityAsync(TempHumidityData data);

    Task<List<AlarmRecordEntity>> CheckAirQualityAsync(AirQualityData data);

    Task<List<AlarmRecordEntity>> QueryRecentAlarmsAsync(int count = 20);

    Task HandleAlarmAsync(long alarmId, string username, string remark);
}
