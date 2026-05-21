using System.Globalization;
using System.IO.Ports;

var portName = args.Length > 0 ? args[0] : "COM3";
var baudRate = args.Length > 1 && int.TryParse(args[1], out var parsedBaudRate) ? parsedBaudRate : 9600;
var count = args.Length > 2 && int.TryParse(args[2], out var parsedCount) ? parsedCount : 20;
var intervalMilliseconds = args.Length > 3 && int.TryParse(args[3], out var parsedInterval) ? parsedInterval : 1000;

Console.WriteLine($"串口空气质量模拟器 -> {portName} / {baudRate}");
Console.WriteLine("参数：portName baudRate count intervalMilliseconds");

using var serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
{
    NewLine = "\r\n"
};

serialPort.Open();

var random = new Random();
for (var index = 0; index < count; index++)
{
    var smoke = 130 + random.Next(0, 120) + (index % 8 == 0 ? 280 : 0);
    var co2 = 650 + random.Next(0, 260) + (index % 10 == 0 ? 520 : 0);
    var level = GetLevel(smoke, co2);
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    var frame = $"AQ,AQ-001,{smoke},{co2},{level},{timestamp}";

    serialPort.WriteLine(frame);
    Console.WriteLine(frame);
    await Task.Delay(intervalMilliseconds);
}

static string GetLevel(int smoke, int co2)
{
    if (smoke >= 700 || co2 >= 1500)
    {
        return "DANGER";
    }

    if (smoke >= 300 || co2 >= 1000)
    {
        return "WARN";
    }

    return "NORMAL";
}
