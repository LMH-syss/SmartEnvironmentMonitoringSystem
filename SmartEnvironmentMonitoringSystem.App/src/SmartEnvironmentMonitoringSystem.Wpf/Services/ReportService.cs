using SmartEnvironmentMonitoringSystem.Wpf.Entities;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class ReportService : IReportService
{
    private readonly IFreeSql database;

    public ReportService(IFreeSql database)
    {
        this.database = database;
    }

    public async Task<MonitorReportEntity> GenerateReportAsync(DateTime startTime, DateTime endTime, string createdBy)
    {
        var tempHumidityRecords = await database.Select<TempHumidityRecordEntity>()
            .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime)
            .ToListAsync();

        var airQualityRecords = await database.Select<AirQualityRecordEntity>()
            .Where(record => record.CollectTime >= startTime && record.CollectTime <= endTime)
            .ToListAsync();

        var alarmRecords = await database.Select<AlarmRecordEntity>()
            .Where(record => record.AlarmTime >= startTime && record.AlarmTime <= endTime)
            .ToListAsync();

        var report = new MonitorReportEntity
        {
            ReportNo = $"RPT-{DateTime.Now:yyyyMMddHHmmss}",
            StartTime = startTime,
            EndTime = endTime,
            MaxTemperature = tempHumidityRecords.Count == 0 ? 0 : tempHumidityRecords.Max(record => record.Temperature),
            MinTemperature = tempHumidityRecords.Count == 0 ? 0 : tempHumidityRecords.Min(record => record.Temperature),
            AvgTemperature = tempHumidityRecords.Count == 0 ? 0 : Math.Round(tempHumidityRecords.Average(record => record.Temperature), 2),
            MaxHumidity = tempHumidityRecords.Count == 0 ? 0 : tempHumidityRecords.Max(record => record.Humidity),
            MinHumidity = tempHumidityRecords.Count == 0 ? 0 : tempHumidityRecords.Min(record => record.Humidity),
            AvgHumidity = tempHumidityRecords.Count == 0 ? 0 : Math.Round(tempHumidityRecords.Average(record => record.Humidity), 2),
            MaxSmokePpm = airQualityRecords.Count == 0 ? 0 : airQualityRecords.Max(record => record.SmokePpm),
            AvgSmokePpm = airQualityRecords.Count == 0 ? 0 : Math.Round(airQualityRecords.Average(record => record.SmokePpm), 2),
            MaxCo2Ppm = airQualityRecords.Count == 0 ? 0 : airQualityRecords.Max(record => record.Co2Ppm),
            AvgCo2Ppm = airQualityRecords.Count == 0 ? 0 : Math.Round(airQualityRecords.Average(record => record.Co2Ppm), 2),
            AlarmCount = alarmRecords.Count,
            DangerAlarmCount = alarmRecords.Count(record => record.AlarmLevel == "DANGER"),
            CreatedAt = DateTime.Now,
            CreatedBy = createdBy
        };

        report.Evaluation = Evaluate(report);
        await database.Insert(report).ExecuteAffrowsAsync();
        return report;
    }

    public Task<List<MonitorReportEntity>> QueryReportsAsync(DateTime? startTime, DateTime? endTime)
    {
        var query = database.Select<MonitorReportEntity>();

        if (startTime is not null)
        {
            query = query.Where(report => report.StartTime >= startTime.Value);
        }

        if (endTime is not null)
        {
            query = query.Where(report => report.EndTime <= endTime.Value);
        }

        return query.OrderByDescending(report => report.CreatedAt).Limit(100).ToListAsync();
    }

    private static string Evaluate(MonitorReportEntity report)
    {
        if (report.DangerAlarmCount > 0)
        {
            return "环境存在严重风险，建议立即处理";
        }

        if (report.AlarmCount > 0)
        {
            return "环境存在轻微异常";
        }

        return "环境正常";
    }
}
