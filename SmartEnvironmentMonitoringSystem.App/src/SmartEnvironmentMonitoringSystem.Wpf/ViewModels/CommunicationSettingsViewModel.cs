using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class CommunicationSettingsViewModel : ObservableObject
{
    private readonly ICommunicationSettingsService communicationSettingsService;
    private string tcpIp = "0.0.0.0";
    private int tcpPort = 9000;
    private string serialPortName = "COM3";
    private int serialBaudRate = 9600;
    private string statusText = "请加载或修改通信参数。";

    public CommunicationSettingsViewModel(ICommunicationSettingsService communicationSettingsService)
    {
        this.communicationSettingsService = communicationSettingsService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public string TcpIp
    {
        get => tcpIp;
        set => SetProperty(ref tcpIp, value);
    }

    public int TcpPort
    {
        get => tcpPort;
        set => SetProperty(ref tcpPort, value);
    }

    public string SerialPortName
    {
        get => serialPortName;
        set => SetProperty(ref serialPortName, value);
    }

    public int SerialBaudRate
    {
        get => serialBaudRate;
        set => SetProperty(ref serialBaudRate, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public Task ActivateAsync()
    {
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var settings = await communicationSettingsService.LoadAsync();
            TcpIp = settings.TcpIp;
            TcpPort = settings.TcpPort;
            SerialPortName = settings.SerialPortName;
            SerialBaudRate = settings.SerialBaudRate;
            StatusText = "通信参数已加载。";
        }
        catch (Exception ex)
        {
            StatusText = $"通信参数加载失败：{ex.Message}";
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TcpIp))
        {
            StatusText = "TCP 监听 IP 不能为空。";
            return;
        }

        if (TcpPort is < 1 or > 65535)
        {
            StatusText = "TCP 端口必须在 1 到 65535 之间。";
            return;
        }

        if (string.IsNullOrWhiteSpace(SerialPortName))
        {
            StatusText = "串口号不能为空。";
            return;
        }

        if (SerialBaudRate <= 0)
        {
            StatusText = "串口波特率必须大于 0。";
            return;
        }

        try
        {
            await communicationSettingsService.SaveAsync(new CommunicationSettings
            {
                TcpIp = TcpIp.Trim(),
                TcpPort = TcpPort,
                SerialPortName = SerialPortName.Trim(),
                SerialBaudRate = SerialBaudRate
            });

            StatusText = "通信参数已保存，重新登录或重启程序后 Dashboard 将按新配置启动通信。";
        }
        catch (Exception ex)
        {
            StatusText = $"通信参数保存失败：{ex.Message}";
        }
    }
}
