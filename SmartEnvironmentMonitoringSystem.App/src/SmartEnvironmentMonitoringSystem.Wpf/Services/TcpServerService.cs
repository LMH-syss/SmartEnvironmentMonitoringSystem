using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using SmartEnvironmentMonitoringSystem.Wpf.Communication.Tcp;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class TcpServerService : ITcpServerService, IDisposable
{
    private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(15);
    private readonly IFreeSql database;
    private readonly object syncRoot = new();
    private readonly ConcurrentDictionary<string, TcpDeviceConnectionState> connections = new();
    private TcpListener? listener;
    private CancellationTokenSource? serverCancellation;
    private Task? acceptLoopTask;
    private Timer? heartbeatTimer;

    public TcpServerService(IFreeSql database)
    {
        this.database = database;
    }

    public event EventHandler<TempHumidityData>? TempHumidityReceived;

    public event EventHandler<string>? RawMessageReceived;

    public event EventHandler<string>? ErrorOccurred;

    public event EventHandler<IReadOnlyList<TcpDeviceConnectionInfo>>? DeviceConnectionsChanged;

    public event EventHandler<TcpCommunicationLog>? CommunicationLogReceived;

    public bool IsRunning { get; private set; }

    public Task StartAsync(string ip, int port, CancellationToken cancellationToken = default)
    {
        lock (syncRoot)
        {
            if (IsRunning)
            {
                return Task.CompletedTask;
            }

            var address = ip == "0.0.0.0" ? IPAddress.Any : IPAddress.Parse(ip);
            listener = new TcpListener(address, port);
            listener.Start();
            serverCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IsRunning = true;
            acceptLoopTask = AcceptLoopAsync(serverCancellation.Token);
            heartbeatTimer = new Timer(CheckHeartbeatTimeouts, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        PublishLog("SYSTEM", "-", "启动", $"TCP 监听已启动：{ip}:{port}");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        Task? taskToWait;
        lock (syncRoot)
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            serverCancellation?.Cancel();
            listener?.Stop();
            heartbeatTimer?.Dispose();
            heartbeatTimer = null;
            taskToWait = acceptLoopTask;
        }

        if (taskToWait is not null)
        {
            try
            {
                await taskToWait.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        serverCancellation?.Dispose();
        serverCancellation = null;
        listener = null;
        acceptLoopTask = null;
        foreach (var state in connections.Values)
        {
            state.Status = "Offline";
            state.LastMessageType = "stop";
        }

        PublishConnections();
        PublishLog("SYSTEM", "-", "停止", "TCP 监听已停止。");
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && listener is not null)
            {
                var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"TCP 监听异常：{ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid().ToString("N");
        var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        var state = new TcpDeviceConnectionState
        {
            ConnectionId = connectionId,
            DeviceId = "未识别",
            RemoteEndPoint = remoteEndPoint,
            Status = "Connected",
            ConnectedAt = DateTime.Now,
            LastSeenAt = DateTime.Now,
            LastMessageType = "connect"
        };

        connections[connectionId] = state;
        PublishConnections();
        PublishLog(state.DeviceId, state.RemoteEndPoint, "连接", "TCP 客户端已连接。");

        try
        {
            using (client)
            using (var reader = new StreamReader(client.GetStream(), Encoding.UTF8))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (line is null)
                    {
                        break;
                    }

                    RawMessageReceived?.Invoke(this, line);

                    if (!TcpJsonMessageParser.TryParse(line, out var message, out var error) || message is null)
                    {
                        PublishLog(state.DeviceId, state.RemoteEndPoint, "解析失败", error ?? "TCP JSON 解析失败。");
                        ErrorOccurred?.Invoke(this, error ?? "温湿度 JSON 解析失败。");
                        continue;
                    }

                    UpdateConnection(state, message);

                    if (message.Type == "heartbeat")
                    {
                        PublishLog(state.DeviceId, state.RemoteEndPoint, "心跳", $"状态：{message.Status}");
                        continue;
                    }

                    if (message.TempHumidity is not null)
                    {
                        try
                        {
                            await SaveAsync(message.TempHumidity).ConfigureAwait(false);
                            PublishLog(state.DeviceId, state.RemoteEndPoint, "数据", $"温度 {message.TempHumidity.Temperature:F1} ℃，湿度 {message.TempHumidity.Humidity:F1} %RH");
                            TempHumidityReceived?.Invoke(this, message.TempHumidity);
                        }
                        catch (Exception ex)
                        {
                            PublishLog(state.DeviceId, state.RemoteEndPoint, "入库失败", ex.Message);
                            ErrorOccurred?.Invoke(this, $"温湿度数据入库失败：{ex.Message}");
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException ex)
        {
            PublishLog(state.DeviceId, state.RemoteEndPoint, "异常断开", ex.Message);
            ErrorOccurred?.Invoke(this, $"TCP 客户端连接异常：{ex.Message}");
        }
        catch (Exception ex)
        {
            PublishLog(state.DeviceId, state.RemoteEndPoint, "处理异常", ex.Message);
            ErrorOccurred?.Invoke(this, $"TCP 数据处理异常：{ex.Message}");
        }
        finally
        {
            state.Status = "Offline";
            state.LastMessageType = "disconnect";
            state.LastSeenAt = DateTime.Now;
            PublishConnections();
            PublishLog(state.DeviceId, state.RemoteEndPoint, "断开", "TCP 客户端已断开。");
        }
    }

    private Task SaveAsync(TempHumidityData data)
    {
        var record = new TempHumidityRecordEntity
        {
            SensorId = data.SensorId,
            Temperature = data.Temperature,
            Humidity = data.Humidity,
            CollectTime = data.CollectTime,
            ReceiveTime = data.ReceiveTime,
            IsAlarm = false
        };

        return database.Insert(record).ExecuteAffrowsAsync();
    }

    private void UpdateConnection(TcpDeviceConnectionState state, TcpDeviceMessage message)
    {
        state.DeviceId = message.DeviceId;
        state.Status = "Online";
        state.LastSeenAt = DateTime.Now;
        state.LastMessageType = message.Type;
        PublishConnections();
    }

    private void CheckHeartbeatTimeouts(object? state)
    {
        var changed = false;
        var now = DateTime.Now;
        foreach (var connection in connections.Values)
        {
            if (connection.Status is "Online" or "Connected"
                && now - connection.LastSeenAt > HeartbeatTimeout)
            {
                connection.Status = "Timeout";
                connection.LastMessageType = "timeout";
                changed = true;
                PublishLog(connection.DeviceId, connection.RemoteEndPoint, "心跳超时", $"超过 {HeartbeatTimeout.TotalSeconds:F0} 秒未收到数据。");
            }
        }

        if (changed)
        {
            PublishConnections();
        }
    }

    private void PublishConnections()
    {
        var snapshot = connections.Values
            .OrderByDescending(connection => connection.LastSeenAt)
            .Select(connection => new TcpDeviceConnectionInfo
            {
                ConnectionId = connection.ConnectionId,
                DeviceId = connection.DeviceId,
                RemoteEndPoint = connection.RemoteEndPoint,
                Status = connection.Status,
                ConnectedAt = connection.ConnectedAt,
                LastSeenAt = connection.LastSeenAt,
                LastMessageType = connection.LastMessageType
            })
            .ToList();

        DeviceConnectionsChanged?.Invoke(this, snapshot);
    }

    private void PublishLog(string deviceId, string remoteEndPoint, string eventType, string message)
    {
        var log = new TcpCommunicationLog
        {
            Time = DateTime.Now,
            DeviceId = deviceId,
            RemoteEndPoint = remoteEndPoint,
            EventType = eventType,
            Message = message
        };
        CommunicationLogReceived?.Invoke(this, log);
    }

    private sealed class TcpDeviceConnectionState
    {
        public required string ConnectionId { get; init; }

        public required string DeviceId { get; set; }

        public required string RemoteEndPoint { get; init; }

        public required string Status { get; set; }

        public DateTime ConnectedAt { get; init; }

        public DateTime LastSeenAt { get; set; }

        public required string LastMessageType { get; set; }
    }
}
