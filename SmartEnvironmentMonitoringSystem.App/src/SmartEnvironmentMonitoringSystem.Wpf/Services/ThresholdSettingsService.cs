using System.Globalization;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class ThresholdSettingsService : IThresholdSettingsService
{
    private readonly IFreeSql database;

    public ThresholdSettingsService(IFreeSql database)
    {
        this.database = database;
    }

    public async Task<ThresholdSettings> LoadAsync()
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

    public async Task SaveAsync(ThresholdSettings settings)
    {
        await SaveConfigAsync("Threshold.TemperatureMin", settings.TemperatureMin.ToString(CultureInfo.InvariantCulture), "温度下限");
        await SaveConfigAsync("Threshold.TemperatureMax", settings.TemperatureMax.ToString(CultureInfo.InvariantCulture), "温度上限");
        await SaveConfigAsync("Threshold.HumidityMin", settings.HumidityMin.ToString(CultureInfo.InvariantCulture), "湿度下限");
        await SaveConfigAsync("Threshold.HumidityMax", settings.HumidityMax.ToString(CultureInfo.InvariantCulture), "湿度上限");
        await SaveConfigAsync("Threshold.SmokeMax", settings.SmokeMax.ToString(CultureInfo.InvariantCulture), "烟雾上限");
        await SaveConfigAsync("Threshold.Co2Max", settings.Co2Max.ToString(CultureInfo.InvariantCulture), "CO2 上限");
    }

    private async Task SaveConfigAsync(string key, string value, string description)
    {
        var affected = await database.Update<SystemConfigEntity>()
            .Set(config => config.ConfigValue, value)
            .Set(config => config.Description, description)
            .Where(config => config.ConfigKey == key)
            .ExecuteAffrowsAsync();

        if (affected > 0)
        {
            return;
        }

        await database.Insert(new SystemConfigEntity
        {
            ConfigKey = key,
            ConfigValue = value,
            Description = description
        }).ExecuteAffrowsAsync();
    }

    private static double GetDouble(IReadOnlyCollection<SystemConfigEntity> configs, string key, double fallback)
    {
        var value = configs.FirstOrDefault(config => config.ConfigKey == key)?.ConfigValue;
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }
}
