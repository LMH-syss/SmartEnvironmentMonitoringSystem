using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class ThresholdSettingsViewModel : ObservableObject
{
    private readonly IThresholdSettingsService thresholdSettingsService;
    private double temperatureMin;
    private double temperatureMax;
    private double humidityMin;
    private double humidityMax;
    private double smokeMax;
    private double co2Max;
    private string statusText = "请加载或修改阈值。";

    public ThresholdSettingsViewModel(IThresholdSettingsService thresholdSettingsService)
    {
        this.thresholdSettingsService = thresholdSettingsService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public double TemperatureMin
    {
        get => temperatureMin;
        set => SetProperty(ref temperatureMin, value);
    }

    public double TemperatureMax
    {
        get => temperatureMax;
        set => SetProperty(ref temperatureMax, value);
    }

    public double HumidityMin
    {
        get => humidityMin;
        set => SetProperty(ref humidityMin, value);
    }

    public double HumidityMax
    {
        get => humidityMax;
        set => SetProperty(ref humidityMax, value);
    }

    public double SmokeMax
    {
        get => smokeMax;
        set => SetProperty(ref smokeMax, value);
    }

    public double Co2Max
    {
        get => co2Max;
        set => SetProperty(ref co2Max, value);
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
            var settings = await thresholdSettingsService.LoadAsync();
            TemperatureMin = settings.TemperatureMin;
            TemperatureMax = settings.TemperatureMax;
            HumidityMin = settings.HumidityMin;
            HumidityMax = settings.HumidityMax;
            SmokeMax = settings.SmokeMax;
            Co2Max = settings.Co2Max;
            StatusText = "阈值已加载。";
        }
        catch (Exception ex)
        {
            StatusText = $"阈值加载失败：{ex.Message}";
        }
    }

    private async Task SaveAsync()
    {
        if (TemperatureMin > TemperatureMax)
        {
            StatusText = "温度下限不能大于温度上限。";
            return;
        }

        if (HumidityMin > HumidityMax)
        {
            StatusText = "湿度下限不能大于湿度上限。";
            return;
        }

        if (SmokeMax < 0 || Co2Max < 0)
        {
            StatusText = "烟雾和 CO2 阈值不能小于 0。";
            return;
        }

        try
        {
            await thresholdSettingsService.SaveAsync(new ThresholdSettings
            {
                TemperatureMin = TemperatureMin,
                TemperatureMax = TemperatureMax,
                HumidityMin = HumidityMin,
                HumidityMax = HumidityMax,
                SmokeMax = SmokeMax,
                Co2Max = Co2Max
            });

            StatusText = "阈值已保存，后续告警将按新阈值判断。";
        }
        catch (Exception ex)
        {
            StatusText = $"阈值保存失败：{ex.Message}";
        }
    }
}
