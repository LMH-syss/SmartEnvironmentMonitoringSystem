using System.IO.Ports;
using System.Text;
using SmartEnvironmentMonitoringSystem.Wpf.Communication.Serial;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class SerialPortService : ISerialPortService, IDisposable
{
    private readonly IFreeSql database;
    private readonly SerialFrameBuffer frameBuffer = new();
    private SerialPort? serialPort;

    public SerialPortService(IFreeSql database)
    {
        this.database = database;
    }

    public event EventHandler<AirQualityData>? AirQualityReceived;

    public event EventHandler<string>? RawMessageReceived;

    public event EventHandler<string>? ErrorOccurred;

    public bool IsOpen => serialPort?.IsOpen == true;

    public void Open(string portName, int baudRate)
    {
        if (IsOpen)
        {
            return;
        }

        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            Encoding = Encoding.ASCII,
            NewLine = "\r\n",
            ReadTimeout = 500
        };

        try
        {
            serialPort.DataReceived += OnDataReceived;
            serialPort.ErrorReceived += OnErrorReceived;
            serialPort.Open();
        }
        catch
        {
            serialPort.DataReceived -= OnDataReceived;
            serialPort.ErrorReceived -= OnErrorReceived;
            serialPort.Dispose();
            serialPort = null;
            throw;
        }
    }

    public void Close()
    {
        if (serialPort is null)
        {
            return;
        }

        serialPort.DataReceived -= OnDataReceived;
        serialPort.ErrorReceived -= OnErrorReceived;

        try
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"串口关闭异常：{ex.Message}");
        }

        serialPort.Dispose();
        serialPort = null;
    }

    public void Dispose()
    {
        Close();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (serialPort is null)
            {
                return;
            }

            var text = serialPort.ReadExisting();
            foreach (var frame in frameBuffer.Append(text))
            {
                RawMessageReceived?.Invoke(this, frame);

                if (!AirQualitySerialParser.TryParse(frame, out var data, out var error) || data is null)
                {
                    ErrorOccurred?.Invoke(this, error ?? "空气质量串口帧解析失败。");
                    continue;
                }

                _ = SaveAndPublishAsync(data);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"串口数据处理异常：{ex.Message}");
        }
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        ErrorOccurred?.Invoke(this, $"串口错误：{e.EventType}");
    }

    private async Task SaveAndPublishAsync(AirQualityData data)
    {
        try
        {
            var record = new AirQualityRecordEntity
            {
                SensorId = data.SensorId,
                SmokePpm = data.SmokePpm,
                Co2Ppm = data.Co2Ppm,
                Level = data.Level,
                CollectTime = data.CollectTime,
                ReceiveTime = data.ReceiveTime,
                IsAlarm = false
            };

            await database.Insert(record).ExecuteAffrowsAsync().ConfigureAwait(false);
            AirQualityReceived?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"空气质量数据入库失败：{ex.Message}");
        }
    }
}
