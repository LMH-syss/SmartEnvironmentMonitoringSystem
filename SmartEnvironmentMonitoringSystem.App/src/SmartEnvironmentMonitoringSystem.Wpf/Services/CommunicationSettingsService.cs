using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class CommunicationSettingsService : ICommunicationSettingsService
{
    private readonly IFreeSql database;

    public CommunicationSettingsService(IFreeSql database)
    {
        this.database = database;
    }

    public async Task<CommunicationSettings> LoadAsync()
    {
        var configs = await database.Select<SystemConfigEntity>().ToListAsync();
        return new CommunicationSettings
        {
            TcpIp = GetString(configs, "Communication.TcpIp", "0.0.0.0"),
            TcpPort = GetInt(configs, "Communication.TcpPort", 9000),
            SerialPortName = GetString(configs, "Communication.SerialPortName", "COM3"),
            SerialBaudRate = GetInt(configs, "Communication.SerialBaudRate", 9600)
        };
    }

    public async Task SaveAsync(CommunicationSettings settings)
    {
        await SaveConfigAsync("Communication.TcpIp", settings.TcpIp, "TCP 监听 IP");
        await SaveConfigAsync("Communication.TcpPort", settings.TcpPort.ToString(), "TCP 监听端口");
        await SaveConfigAsync("Communication.SerialPortName", settings.SerialPortName, "串口号");
        await SaveConfigAsync("Communication.SerialBaudRate", settings.SerialBaudRate.ToString(), "串口波特率");
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

    private static string GetString(IReadOnlyCollection<SystemConfigEntity> configs, string key, string fallback)
    {
        var value = configs.FirstOrDefault(config => config.ConfigKey == key)?.ConfigValue;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static int GetInt(IReadOnlyCollection<SystemConfigEntity> configs, string key, int fallback)
    {
        var value = configs.FirstOrDefault(config => config.ConfigKey == key)?.ConfigValue;
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
