using System.Windows.Threading;
using SmartEnvironmentMonitoringSystem.Wpf.Models;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class MockEnvironmentDataService : IMockEnvironmentDataService, IDisposable
{
    private readonly DispatcherTimer timer;
    private readonly Random random = new();
    private int tick;

    public MockEnvironmentDataService()
    {
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += OnTimerTick;
    }

    public event EventHandler<EnvironmentSnapshot>? SnapshotGenerated;

    public bool IsRunning => timer.IsEnabled;

    public void Start()
    {
        if (!timer.IsEnabled)
        {
            timer.Start();
            GenerateSnapshot();
        }
    }

    public void Stop()
    {
        timer.Stop();
    }

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= OnTimerTick;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        GenerateSnapshot();
    }

    private void GenerateSnapshot()
    {
        tick++;

        var wave = Math.Sin(tick / 8.0);
        var snapshot = new EnvironmentSnapshot
        {
            TempHumiditySensorId = "TH-001",
            AirQualitySensorId = "AQ-001",
            Temperature = Math.Round(25.5 + wave * 3.2 + random.NextDouble() * 0.8, 1),
            Humidity = Math.Round(56 + Math.Cos(tick / 9.0) * 9 + random.NextDouble() * 2, 1),
            SmokePpm = 120 + random.Next(0, 90) + (tick % 18 == 0 ? 220 : 0),
            Co2Ppm = 620 + random.Next(0, 180) + (tick % 24 == 0 ? 420 : 0),
            ReceivedAt = DateTime.Now
        };

        SnapshotGenerated?.Invoke(this, snapshot);
    }
}
