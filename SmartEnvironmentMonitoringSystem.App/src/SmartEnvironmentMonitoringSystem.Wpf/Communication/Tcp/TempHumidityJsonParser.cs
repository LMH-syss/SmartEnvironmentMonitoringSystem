using System.Text.Json;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Communication.Tcp;

public static class TempHumidityJsonParser
{
    public static bool TryParse(string json, out TempHumidityData? data, out string? error)
    {
        data = null;
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "空消息。";
            return false;
        }

        try
        {
            if (!TcpJsonMessageParser.TryParse(json, out var message, out error)
                || message?.TempHumidity is null)
            {
                return false;
            }

            data = message.TempHumidity;
            return true;
        }
        catch (JsonException ex)
        {
            error = $"JSON 格式错误：{ex.Message}";
            return false;
        }
        catch (InvalidOperationException ex)
        {
            error = $"JSON 字段类型错误：{ex.Message}";
            return false;
        }
    }
}
