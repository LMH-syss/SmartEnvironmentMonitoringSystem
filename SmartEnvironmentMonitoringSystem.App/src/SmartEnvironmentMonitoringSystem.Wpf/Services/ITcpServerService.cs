using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface ITcpServerService
{
    event EventHandler<TempHumidityData>? TempHumidityReceived;

    event EventHandler<string>? RawMessageReceived;

    event EventHandler<string>? ErrorOccurred;

    event EventHandler<IReadOnlyList<TcpDeviceConnectionInfo>>? DeviceConnectionsChanged;

    event EventHandler<TcpCommunicationLog>? CommunicationLogReceived;

    bool IsRunning { get; }

    Task StartAsync(string ip, int port, CancellationToken cancellationToken = default);

    Task StopAsync();
}
