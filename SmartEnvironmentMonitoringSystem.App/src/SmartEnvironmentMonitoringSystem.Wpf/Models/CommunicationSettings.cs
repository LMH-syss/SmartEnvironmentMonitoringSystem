namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class CommunicationSettings
{
    public string TcpIp { get; set; } = "0.0.0.0";

    public int TcpPort { get; set; } = 9000;

    public string SerialPortName { get; set; } = "COM3";

    public int SerialBaudRate { get; set; } = 9600;
}
