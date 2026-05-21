using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 && int.TryParse(args[1], out var parsedPort) ? parsedPort : 9000;
var deviceId = args.Length > 2 ? args[2] : "TH-001";
var count = args.Length > 3 && int.TryParse(args[3], out var parsedCount) ? parsedCount : 20;
var intervalMilliseconds = args.Length > 4 && int.TryParse(args[4], out var parsedInterval) ? parsedInterval : 1000;
var heartbeatInterval = args.Length > 5 && int.TryParse(args[5], out var parsedHeartbeatInterval) ? parsedHeartbeatInterval : 5;
var useLegacyProtocol = args.Length > 6 && string.Equals(args[6], "legacy", StringComparison.OrdinalIgnoreCase);

Console.WriteLine($"TCP 温湿度模拟器 -> {host}:{port} / {deviceId}");
Console.WriteLine("参数：host port deviceId count intervalMilliseconds heartbeatInterval legacy");

using var client = new TcpClient();
await client.ConnectAsync(host, port);
await using var stream = client.GetStream();

var random = new Random();
for (var index = 0; index < count; index++)
{
    if (!useLegacyProtocol && index % heartbeatInterval == 0)
    {
        var heartbeat = new
        {
            type = "heartbeat",
            deviceId,
            status = "online",
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        };
        await WriteJsonAsync(stream, heartbeat);
    }

    var temperature = Math.Round(24.5 + Math.Sin(index / 4.0) * 2.5 + random.NextDouble(), 1);
    var humidity = Math.Round(55 + Math.Cos(index / 5.0) * 8 + random.NextDouble() * 2, 1);
    object payload = useLegacyProtocol
        ? new
        {
            id = deviceId,
            temperature,
            humidity,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        }
        : new
        {
            type = "telemetry",
            deviceId,
            temperature,
            humidity,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        };

    await WriteJsonAsync(stream, payload);
    await Task.Delay(intervalMilliseconds);
}

static async Task WriteJsonAsync(NetworkStream stream, object payload)
{
    var json = JsonSerializer.Serialize(payload);
    var bytes = Encoding.UTF8.GetBytes(json + "\n");
    await stream.WriteAsync(bytes);
    await stream.FlushAsync();
    Console.WriteLine(json);
}
