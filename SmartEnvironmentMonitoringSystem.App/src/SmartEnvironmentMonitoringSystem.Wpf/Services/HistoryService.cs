using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class HistoryService : IHistoryService
{
    private readonly IFreeSql database;

    public HistoryService(IFreeSql database)
    {
        this.database = database;
    }

    public Task<List<TempHumidityRecordEntity>> QueryTempHumidityAsync(DateTime startTime, DateTime endTime, string? sensorId)
    {
        var query = database.Select<TempHumidityRecordEntity>()
            .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime);

        if (!string.IsNullOrWhiteSpace(sensorId))
        {
            var keyword = sensorId.Trim();
            query = query.Where(record => record.SensorId == keyword);
        }

        return query.OrderByDescending(record => record.CollectTime).Limit(500).ToListAsync();
    }

    public Task<List<AirQualityRecordEntity>> QueryAirQualityAsync(DateTime startTime, DateTime endTime, string? sensorId)
    {
        var query = database.Select<AirQualityRecordEntity>()
            .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime);

        if (!string.IsNullOrWhiteSpace(sensorId))
        {
            var keyword = sensorId.Trim();
            query = query.Where(record => record.SensorId == keyword);
        }

        return query.OrderByDescending(record => record.CollectTime).Limit(500).ToListAsync();
    }

    public Task<List<AlarmRecordEntity>> QueryAlarmsAsync(DateTime startTime, DateTime endTime, string? sensorId)
    {
        var query = database.Select<AlarmRecordEntity>()
            .Where(record => record.AlarmTime >= startTime && record.AlarmTime <= endTime);

        if (!string.IsNullOrWhiteSpace(sensorId))
        {
            var keyword = sensorId.Trim();
            query = query.Where(record => record.SensorId == keyword);
        }

        return query.OrderByDescending(record => record.AlarmTime).Limit(500).ToListAsync();
    }

    public async Task<HistoryDeleteResult> DeleteHistoryAsync(DateTime startTime, DateTime endTime, string? sensorId, HistoryDeleteOptions options)
    {
        var result = new HistoryDeleteResult();
        var keyword = string.IsNullOrWhiteSpace(sensorId) ? null : sensorId.Trim();

        if (options.DeleteTempHumidity)
        {
            var delete = database.Delete<TempHumidityRecordEntity>()
                .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime);

            if (keyword is not null)
            {
                delete = delete.Where(record => record.SensorId == keyword);
            }

            result.TempHumidityCount = await delete.ExecuteAffrowsAsync();
        }

        if (options.DeleteAirQuality)
        {
            var delete = database.Delete<AirQualityRecordEntity>()
                .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime);

            if (keyword is not null)
            {
                delete = delete.Where(record => record.SensorId == keyword);
            }

            result.AirQualityCount = await delete.ExecuteAffrowsAsync();
        }

        if (options.DeleteAlarms)
        {
            var delete = database.Delete<AlarmRecordEntity>()
                .Where(record => record.AlarmTime >= startTime && record.AlarmTime <= endTime);

            if (keyword is not null)
            {
                delete = delete.Where(record => record.SensorId == keyword);
            }

            result.AlarmCount = await delete.ExecuteAffrowsAsync();
        }

        if (options.DeleteReports)
        {
            result.ReportCount = await database.Delete<MonitorReportEntity>()
                .Where(record => record.StartTime >= startTime && record.EndTime <= endTime)
                .ExecuteAffrowsAsync();
        }

        return result;
    }
}
