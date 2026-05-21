using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class DashboardViewModel : ObservableObject, IDisposable
{
    private const int MaxTemperaturePoints = 50;
    private const int MaxAlarmRows = 20;
    private const int MaxTcpLogRows = 50;
    private readonly ITcpServerService tcpServerService;
    private readonly ISerialPortService serialPortService;
    private readonly IAlarmService alarmService;
    private readonly ICommunicationSettingsService communicationSettingsService;
    private double currentTemperature;
    private double currentHumidity;
    private int currentSmokePpm;
    private int currentCo2Ppm;
    private string lastReceivedText = "--";
    private string sensorStatusText = "等待 TCP 温湿度数据";
    private string alarmStatusText = "正常";
    private string tcpStatusText = "TCP 未启动";
    private string serialStatusText = "串口未打开";
    private string currentUsername = string.Empty;
    private string lastTempHumiditySensorId = string.Empty;
    private bool canHandleAlarms;
    private bool started;

    public DashboardViewModel(
        ITcpServerService tcpServerService,
        ISerialPortService serialPortService,
        IAlarmService alarmService,
        ICommunicationSettingsService communicationSettingsService)
    {
        this.tcpServerService = tcpServerService;
        this.serialPortService = serialPortService;
        this.alarmService = alarmService;
        this.communicationSettingsService = communicationSettingsService;

        this.tcpServerService.TempHumidityReceived += OnTempHumidityReceived;
        this.tcpServerService.ErrorOccurred += OnTcpErrorOccurred;
        this.tcpServerService.DeviceConnectionsChanged += OnTcpDeviceConnectionsChanged;
        this.tcpServerService.CommunicationLogReceived += OnTcpCommunicationLogReceived;
        this.serialPortService.AirQualityReceived += OnAirQualityReceived;
        this.serialPortService.ErrorOccurred += OnSerialErrorOccurred;

        HandleAlarmCommand = new AsyncRelayCommand<AlarmRecordEntity>(HandleAlarmAsync, alarm => CanHandleAlarms && alarm is { IsHandled: false });

        TemperatureSeries =
        [
            new LineSeries<double>
            {
                Name = "温度",
                Values = TemperatureValues,
                GeometrySize = 8,
                Fill = null
            }
        ];

        TemperatureAxes =
        [
            new Axis
            {
                Name = "温度 ℃",
                MinLimit = 15,
                MaxLimit = 38
            }
        ];
    }

    public ObservableCollection<double> TemperatureValues { get; } = [];

    public ObservableCollection<AlarmRecordEntity> RecentAlarms { get; } = [];

    public ObservableCollection<TcpDeviceConnectionInfo> TcpDevices { get; } = [];

    public ObservableCollection<TcpCommunicationLog> TcpCommunicationLogs { get; } = [];

    public ISeries[] TemperatureSeries { get; }

    public Axis[] TemperatureAxes { get; }

    public IAsyncRelayCommand<AlarmRecordEntity> HandleAlarmCommand { get; }

    public double CurrentTemperature
    {
        get => currentTemperature;
        private set => SetProperty(ref currentTemperature, value);
    }

    public double CurrentHumidity
    {
        get => currentHumidity;
        private set => SetProperty(ref currentHumidity, value);
    }

    public int CurrentSmokePpm
    {
        get => currentSmokePpm;
        private set => SetProperty(ref currentSmokePpm, value);
    }

    public int CurrentCo2Ppm
    {
        get => currentCo2Ppm;
        private set => SetProperty(ref currentCo2Ppm, value);
    }

    public string LastReceivedText
    {
        get => lastReceivedText;
        private set => SetProperty(ref lastReceivedText, value);
    }

    public string SensorStatusText
    {
        get => sensorStatusText;
        private set => SetProperty(ref sensorStatusText, value);
    }

    public string AlarmStatusText
    {
        get => alarmStatusText;
        private set => SetProperty(ref alarmStatusText, value);
    }

    public string TcpStatusText
    {
        get => tcpStatusText;
        private set => SetProperty(ref tcpStatusText, value);
    }

    public string SerialStatusText
    {
        get => serialStatusText;
        private set => SetProperty(ref serialStatusText, value);
    }

    public bool CanHandleAlarms
    {
        get => canHandleAlarms;
        private set
        {
            if (SetProperty(ref canHandleAlarms, value))
            {
                HandleAlarmCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public void SetCurrentUser(string username, bool isAdmin)
    {
        currentUsername = username;
        CanHandleAlarms = isAdmin;
    }

    public void Start()
    {
        if (started)
        {
            return;
        }

        started = true;
        _ = LoadRecentAlarmsAsync();
        _ = StartTcpAsync();
        StartSerial();
    }

    public void Dispose()
    {
        tcpServerService.TempHumidityReceived -= OnTempHumidityReceived;
        tcpServerService.ErrorOccurred -= OnTcpErrorOccurred;
        // 修正 CS8601：将委托置为 null，避免 null 赋值警告
        tcpServerService.DeviceConnectionsChanged -= OnTcpDeviceConnectionsChanged;
        tcpServerService.CommunicationLogReceived -= OnTcpCommunicationLogReceived;
        serialPortService.AirQualityReceived -= OnAirQualityReceived;
        serialPortService.ErrorOccurred -= OnSerialErrorOccurred;

        try
        {
            serialPortService.Close();
            tcpServerService.StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }
    }

    private async Task StartTcpAsync()
    {
        try
        {
            var settings = await communicationSettingsService.LoadAsync();
            await tcpServerService.StartAsync(settings.TcpIp, settings.TcpPort);
            TcpStatusText = $"TCP 监听中：{settings.TcpIp}:{settings.TcpPort}";
        }
        catch (Exception ex)
        {
            TcpStatusText = $"TCP 启动失败：{ex.Message}";
        }
    }

    private void StartSerial()
    {
        try
        {
            var settings = communicationSettingsService.LoadAsync().GetAwaiter().GetResult();
            serialPortService.Open(settings.SerialPortName, settings.SerialBaudRate);
            SerialStatusText = $"串口已打开：{settings.SerialPortName} / {settings.SerialBaudRate}";
        }
        catch (Exception ex)
        {
            SerialStatusText = $"串口打开失败：{ex.Message}";
        }
    }

    private void OnTempHumidityReceived(object? sender, TempHumidityData data)
    {
        RunOnUiThread(() =>
        {
            CurrentTemperature = data.Temperature;
            CurrentHumidity = data.Humidity;
            LastReceivedText = data.ReceiveTime.ToString("HH:mm:ss");
            lastTempHumiditySensorId = data.SensorId;
            SensorStatusText = $"{data.SensorId} TCP 在线";

            TemperatureValues.Add(data.Temperature);
            while (TemperatureValues.Count > MaxTemperaturePoints)
            {
                TemperatureValues.RemoveAt(0);
            }
        });

        _ = CheckTempHumidityAlarmAsync(data);
    }

    private void OnAirQualityReceived(object? sender, AirQualityData data)
    {
        RunOnUiThread(() =>
        {
            CurrentSmokePpm = data.SmokePpm;
            CurrentCo2Ppm = data.Co2Ppm;
            LastReceivedText = data.ReceiveTime.ToString("HH:mm:ss");
            SerialStatusText = $"{data.SensorId} 串口在线：{data.Level}";
        });

        _ = CheckAirQualityAlarmAsync(data);
    }

    private async Task CheckTempHumidityAlarmAsync(TempHumidityData data)
    {
        try
        {
            var alarms = await alarmService.CheckTempHumidityAsync(data);
            AddAlarms(alarms);
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => AlarmStatusText = $"温湿度告警检查失败：{ex.Message}");
        }
    }

    private async Task CheckAirQualityAlarmAsync(AirQualityData data)
    {
        try
        {
            var alarms = await alarmService.CheckAirQualityAsync(data);
            AddAlarms(alarms);
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => AlarmStatusText = $"空气质量告警检查失败：{ex.Message}");
        }
    }

    private async Task LoadRecentAlarmsAsync()
    {
        try
        {
            var alarms = await alarmService.QueryRecentAlarmsAsync(MaxAlarmRows);
            RunOnUiThread(() =>
            {
                RecentAlarms.Clear();
                foreach (var alarm in alarms)
                {
                    RecentAlarms.Add(alarm);
                }

                RefreshAlarmStatus();
            });
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => AlarmStatusText = $"告警记录加载失败：{ex.Message}");
        }
    }

    private async Task HandleAlarmAsync(AlarmRecordEntity? alarm)
    {
        if (alarm is null || alarm.IsHandled || string.IsNullOrWhiteSpace(currentUsername))
        {
            return;
        }

        try
        {
            await alarmService.HandleAlarmAsync(alarm.Id, currentUsername, "Dashboard 标记已处理");
            alarm.IsHandled = true;
            alarm.HandledBy = currentUsername;
            alarm.HandledAt = DateTime.Now;

            await LoadRecentAlarmsAsync();
        }
        catch (Exception ex)
        {
            AlarmStatusText = $"告警处理失败：{ex.Message}";
        }
    }

    private void AddAlarms(IReadOnlyCollection<AlarmRecordEntity> alarms)
    {
        if (alarms.Count == 0)
        {
            RunOnUiThread(RefreshAlarmStatus);
            return;
        }

        RunOnUiThread(() =>
        {
            foreach (var alarm in alarms.OrderByDescending(item => item.AlarmTime))
            {
                RecentAlarms.Insert(0, alarm);
            }

            while (RecentAlarms.Count > MaxAlarmRows)
            {
                RecentAlarms.RemoveAt(RecentAlarms.Count - 1);
            }

            RefreshAlarmStatus();
            HandleAlarmCommand.NotifyCanExecuteChanged();
        });
    }

    private void RefreshAlarmStatus()
    {
        var unhandled = RecentAlarms.Where(alarm => !alarm.IsHandled).ToList();
        if (unhandled.Count == 0)
        {
            AlarmStatusText = "正常";
            return;
        }

        AlarmStatusText = unhandled.Any(alarm => alarm.AlarmLevel == "DANGER")
            ? $"危险告警 {unhandled.Count} 条"
            : $"未处理告警 {unhandled.Count} 条";
    }

    private void OnTcpErrorOccurred(object? sender, string message)
    {
        RunOnUiThread(() => TcpStatusText = message);
    }

    private void OnTcpDeviceConnectionsChanged(object? sender, IReadOnlyList<TcpDeviceConnectionInfo> devices)
    {
        RunOnUiThread(() =>
        {
            TcpDevices.Clear();
            foreach (var device in devices)
            {
                TcpDevices.Add(device);
            }

            var onlineCount = devices.Count(device => device.Status == "Online");
            TcpStatusText = tcpServerService.IsRunning
                ? $"TCP 监听中，在线设备 {onlineCount} 个，连接记录 {devices.Count} 个"
                : "TCP 未启动";

            RefreshSensorStatus(devices);
        });
    }

    private void OnTcpCommunicationLogReceived(object? sender, TcpCommunicationLog log)
    {
        RunOnUiThread(() =>
        {
            TcpCommunicationLogs.Insert(0, log);
            while (TcpCommunicationLogs.Count > MaxTcpLogRows)
            {
                TcpCommunicationLogs.RemoveAt(TcpCommunicationLogs.Count - 1);
            }
        });
    }

    private void OnSerialErrorOccurred(object? sender, string message)
    {
        RunOnUiThread(() => SerialStatusText = message);
    }

    private void RefreshSensorStatus(IReadOnlyList<TcpDeviceConnectionInfo> devices)
    {
        if (string.IsNullOrWhiteSpace(lastTempHumiditySensorId))
        {
            SensorStatusText = "等待 TCP 温湿度数据";
            return;
        }

        var matchedDevice = devices.LastOrDefault(device => device.DeviceId == lastTempHumiditySensorId);
        if (matchedDevice is null)
        {
            SensorStatusText = $"{lastTempHumiditySensorId} TCP 离线";
            return;
        }

        SensorStatusText = matchedDevice.Status switch
        {
            "Online" => $"{matchedDevice.DeviceId} TCP 在线",
            "Connected" => $"{matchedDevice.DeviceId} TCP 已连接",
            "Timeout" => $"{matchedDevice.DeviceId} TCP 超时",
            _ => $"{matchedDevice.DeviceId} TCP 离线"
        };
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.BeginInvoke(action);
    }
}
