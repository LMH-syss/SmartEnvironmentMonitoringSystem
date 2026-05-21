using System.Text.Json;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Communication.Tcp;

public static class TcpJsonMessageParser
{
    public static bool TryParse(string json, out TcpDeviceMessage? message, out string? error)
    {
        message = null;
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "空消息。";
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var type = GetString(root, "type");

            if (string.IsNullOrWhiteSpace(type))
            {
                return TryParseLegacyTelemetry(root, out message, out error);
            }

            return type.Trim().ToLowerInvariant() switch
            {
                "telemetry" => TryParseTelemetry(root, out message, out error),
                "heartbeat" => TryParseHeartbeat(root, out message, out error),
                _ => Fail($"不支持的 TCP 消息类型：{type}。", out message, out error)
            };
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

    private static bool TryParseLegacyTelemetry(JsonElement root, out TcpDeviceMessage? message, out string? error)
    {
        return TryParseTelemetry(root, out message, out error, "id");
    }

    private static bool TryParseTelemetry(JsonElement root, out TcpDeviceMessage? message, out string? error, string deviceField = "deviceId")
    {
        message = null;
        error = null;

        var deviceId = GetString(root, deviceField);
        if (string.IsNullOrWhiteSpace(deviceId) && deviceField != "id")
        {
            deviceId = GetString(root, "id");
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            error = "TCP telemetry 缺少 deviceId。";
            return false;
        }

        if (!root.TryGetProperty("temperature", out var temperatureElement)
            || !root.TryGetProperty("humidity", out var humidityElement))
        {
            error = "TCP telemetry 缺少 temperature 或 humidity 字段。";
            return false;
        }

        if (!temperatureElement.TryGetDouble(out var temperature)
            || !humidityElement.TryGetDouble(out var humidity))
        {
            error = "temperature 或 humidity 不是有效数字。";
            return false;
        }

        var receiveTime = DateTime.Now;
        var collectTime = ParseTimestamp(root) ?? receiveTime;
        var data = new TempHumidityData
        {
            SensorId = deviceId.Trim(),
            Temperature = temperature,
            Humidity = humidity,
            CollectTime = collectTime,
            ReceiveTime = receiveTime
        };

        message = new TcpDeviceMessage
        {
            Type = "telemetry",
            DeviceId = data.SensorId,
            TempHumidity = data
        };
        return true;
    }

    private static bool TryParseHeartbeat(JsonElement root, out TcpDeviceMessage? message, out string? error)
    {
        message = null;
        error = null;

        var deviceId = GetString(root, "deviceId");
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            error = "TCP heartbeat 缺少 deviceId。";
            return false;
        }

        message = new TcpDeviceMessage
        {
            Type = "heartbeat",
            DeviceId = deviceId.Trim(),
            Status = GetString(root, "status") ?? "online"
        };
        return true;
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var element) ? element.GetString() : null;
    }

    private static DateTime? ParseTimestamp(JsonElement root)
    {
        if (!root.TryGetProperty("timestamp", out var timestampElement))
        {
            return null;
        }

        var timestamp = timestampElement.GetString();
        return !string.IsNullOrWhiteSpace(timestamp) && DateTime.TryParse(timestamp, out var parsedTime)
            ? parsedTime
            : null;
    }

    private static bool Fail(string messageText, out TcpDeviceMessage? message, out string? error)
    {
        message = null;
        error = messageText;
        return false;
    }
}
