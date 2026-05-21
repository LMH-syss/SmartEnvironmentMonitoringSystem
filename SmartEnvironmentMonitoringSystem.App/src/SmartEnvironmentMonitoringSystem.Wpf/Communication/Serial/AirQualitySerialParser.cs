using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Communication.Serial;

public static class AirQualitySerialParser
{
    public static bool TryParse(string frame, out AirQualityData? data, out string? error)
    {
        data = null;
        error = null;

        if (string.IsNullOrWhiteSpace(frame))
        {
            error = "空串口帧。";
            return false;
        }

        var parts = frame.Trim().Split(',');
        if (parts.Length != 6)
        {
            error = "空气质量帧字段数量不正确。";
            return false;
        }

        if (!string.Equals(parts[0], "AQ", StringComparison.OrdinalIgnoreCase))
        {
            error = "空气质量帧头不是 AQ。";
            return false;
        }

        var sensorId = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(sensorId))
        {
            error = "空气质量传感器编号为空。";
            return false;
        }

        if (!int.TryParse(parts[2], out var smokePpm))
        {
            error = "烟雾浓度不是有效整数。";
            return false;
        }

        if (!int.TryParse(parts[3], out var co2Ppm))
        {
            error = "CO2 浓度不是有效整数。";
            return false;
        }

        var level = parts[4].Trim().ToUpperInvariant();
        if (level is not ("NORMAL" or "WARN" or "DANGER"))
        {
            error = "空气质量等级必须是 NORMAL、WARN 或 DANGER。";
            return false;
        }

        var receiveTime = DateTime.Now;
        var collectTime = receiveTime;
        if (!string.IsNullOrWhiteSpace(parts[5]) && DateTime.TryParse(parts[5], out var parsedTime))
        {
            collectTime = parsedTime;
        }

        data = new AirQualityData
        {
            SensorId = sensorId,
            SmokePpm = smokePpm,
            Co2Ppm = co2Ppm,
            Level = level,
            CollectTime = collectTime,
            ReceiveTime = receiveTime
        };

        return true;
    }
}
