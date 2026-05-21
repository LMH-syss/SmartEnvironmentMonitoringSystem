using System.Globalization;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class AlarmService : IAlarmService
{
    private readonly IFreeSql database;

    public AlarmService(IFreeSql database)
    {
        this.database = database;
    }

    public async Task<List<AlarmRecordEntity>> CheckTempHumidityAsync(TempHumidityData data)
    {
        var thresholds = await LoadThresholdsAsync();
        var alarms = new List<AlarmRecordEntity>();

        if (data.Temperature < thresholds.TemperatureMin || data.Temperature > thresholds.TemperatureMax)
        {
            alarms.Add(new AlarmRecordEntity
            {
                SensorId = data.SensorId,
                AlarmType = "Temperature",
                ActualValue = data.Temperature,
                ThresholdDescription = $"{thresholds.TemperatureMin} ~ {thresholds.TemperatureMax} ℃",
                AlarmLevel = "WARN",
                AlarmTime = data.ReceiveTime,
                IsHandled = false
            });
        }

        if (data.Humidity < thresholds.HumidityMin || data.Humidity > thresholds.HumidityMax)
        {
            alarms.Add(new AlarmRecordEntity
            {
                SensorId = data.SensorId,
                AlarmType = "Humidity",
                ActualValue = data.Humidity,
                ThresholdDescription = $"{thresholds.HumidityMin} ~ {thresholds.HumidityMax} %RH",
                AlarmLevel = "WARN",
                AlarmTime = data.ReceiveTime,
                IsHandled = false
            });
        }

        return await SaveAlarmsAsync(alarms);
    }

    public async Task<List<AlarmRecordEntity>> CheckAirQualityAsync(AirQualityData data)
    {
        var thresholds = await LoadThresholdsAsync();
        var alarms = new List<AlarmRecordEntity>();

        if (data.SmokePpm > thresholds.SmokeMax)
        {
            alarms.Add(new AlarmRecordEntity
            {
                SensorId = data.SensorId,
                AlarmType = "Smoke",
                ActualValue = data.SmokePpm,
                ThresholdDescription = $"<= {thresholds.SmokeMax} ppm",
                AlarmLevel = data.SmokePpm >= 700 ? "DANGER" : "WARN",
                AlarmTime = data.ReceiveTime,
                IsHandled = false
            });
        }

        if (data.Co2Ppm > thresholds.Co2Max)
        {
            alarms.Add(new AlarmRecordEntity
            {
                SensorId = data.SensorId,
                AlarmType = "CO2",
                ActualValue = data.Co2Ppm,
                ThresholdDescription = $"<= {thresholds.Co2Max} ppm",
                AlarmLevel = data.Co2Ppm >= 1500 ? "DANGER" : "WARN",
                AlarmTime = data.ReceiveTime,
                IsHandled = false
            });
        }

        return await SaveAlarmsAsync(alarms);
    }

    public Task<List<AlarmRecordEntity>> QueryRecentAlarmsAsync(int count = 20)
    {
        return database.Select<AlarmRecordEntity>()
            .OrderByDescending(alarm => alarm.AlarmTime)
            .Limit(count)
            .ToListAsync();
    }

    public Task HandleAlarmAsync(long alarmId, string username, string remark)
    {
        return database.Update<AlarmRecordEntity>()
            .Set(alarm => alarm.IsHandled, true)
            .Set(alarm => alarm.HandledBy, username)
            .Set(alarm => alarm.HandledAt, DateTime.Now)
            .Set(alarm => alarm.Remark, remark)
            .Where(alarm => alarm.Id == alarmId)
            .ExecuteAffrowsAsync();
    }

    private async Task<List<AlarmRecordEntity>> SaveAlarmsAsync(List<AlarmRecordEntity> alarms)
    {
        if (alarms.Count == 0)
        {
            return alarms;
        }

        await database.Insert(alarms).ExecuteAffrowsAsync();
        return alarms;
    }

    private async Task<ThresholdSettings> LoadThresholdsAsync()
    {
        var configs = await database.Select<SystemConfigEntity>().ToListAsync();

        return new ThresholdSettings
        {
            TemperatureMin = GetDouble(configs, "Threshold.TemperatureMin", 0),
            TemperatureMax = GetDouble(configs, "Threshold.TemperatureMax", 35),
            HumidityMin = GetDouble(configs, "Threshold.HumidityMin", 20),
            HumidityMax = GetDouble(configs, "Threshold.HumidityMax", 80),
            SmokeMax = GetDouble(configs, "Threshold.SmokeMax", 300),
            Co2Max = GetDouble(configs, "Threshold.Co2Max", 1000)
        };
    }

    private static double GetDouble(IReadOnlyCollection<SystemConfigEntity> configs, string key, double fallback)
    {
        var value = configs.FirstOrDefault(config => config.ConfigKey == key)?.ConfigValue;
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private sealed class ThresholdSettings
    {
        public double TemperatureMin { get; init; }

        public double TemperatureMax { get; init; }

        public double HumidityMin { get; init; }

        public double HumidityMax { get; init; }

        public double SmokeMax { get; init; }

        public double Co2Max { get; init; }
    }
}
