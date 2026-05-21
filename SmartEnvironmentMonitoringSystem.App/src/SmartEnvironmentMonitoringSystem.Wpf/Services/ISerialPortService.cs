using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface ISerialPortService
{
    event EventHandler<AirQualityData>? AirQualityReceived;

    event EventHandler<string>? RawMessageReceived;

    event EventHandler<string>? ErrorOccurred;

    bool IsOpen { get; }

    void Open(string portName, int baudRate);

    void Close();
}
