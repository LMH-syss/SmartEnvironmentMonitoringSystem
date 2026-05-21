using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class HistoryViewModel : ObservableObject
{
    private readonly IHistoryService historyService;
    private DateTime? startDate = DateTime.Today;
    private DateTime? endDate = DateTime.Today;
    private string sensorId = string.Empty;
    private string statusText = "请选择条件后查询。";
    private bool canDeleteHistory;
    private bool deleteTempHumidity = true;
    private bool deleteAirQuality = true;
    private bool deleteAlarms = true;
    private bool deleteReports;

    public HistoryViewModel(IHistoryService historyService)
    {
        this.historyService = historyService;
        QueryCommand = new AsyncRelayCommand(QueryAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CanDeleteHistory);
    }

    public DateTime? StartDate
    {
        get => startDate;
        set => SetProperty(ref startDate, value);
    }

    public DateTime? EndDate
    {
        get => endDate;
        set => SetProperty(ref endDate, value);
    }

    public string SensorId
    {
        get => sensorId;
        set => SetProperty(ref sensorId, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public ObservableCollection<TempHumidityRecordEntity> TempHumidityRecords { get; } = [];

    public ObservableCollection<AirQualityRecordEntity> AirQualityRecords { get; } = [];

    public ObservableCollection<AlarmRecordEntity> AlarmRecords { get; } = [];

    public IAsyncRelayCommand QueryCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public bool CanDeleteHistory
    {
        get => canDeleteHistory;
        private set
        {
            if (SetProperty(ref canDeleteHistory, value))
            {
                OnPropertyChanged(nameof(DeletePanelVisibility));
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Visibility DeletePanelVisibility => CanDeleteHistory ? Visibility.Visible : Visibility.Collapsed;

    public bool DeleteTempHumidity
    {
        get => deleteTempHumidity;
        set => SetProperty(ref deleteTempHumidity, value);
    }

    public bool DeleteAirQuality
    {
        get => deleteAirQuality;
        set => SetProperty(ref deleteAirQuality, value);
    }

    public bool DeleteAlarms
    {
        get => deleteAlarms;
        set => SetProperty(ref deleteAlarms, value);
    }

    public bool DeleteReports
    {
        get => deleteReports;
        set => SetProperty(ref deleteReports, value);
    }

    public void SetCurrentUser(bool isAdmin)
    {
        CanDeleteHistory = isAdmin;
    }

    private async Task QueryAsync()
    {
        var start = (StartDate ?? DateTime.Today).Date;
        var end = (EndDate ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        if (end < start)
        {
            StatusText = "结束日期不能早于开始日期。";
            return;
        }

        try
        {
            var tempHumidity = await historyService.QueryTempHumidityAsync(start, end, SensorId);
            var airQuality = await historyService.QueryAirQualityAsync(start, end, SensorId);
            var alarms = await historyService.QueryAlarmsAsync(start, end, SensorId);

            Replace(TempHumidityRecords, tempHumidity);
            Replace(AirQualityRecords, airQuality);
            Replace(AlarmRecords, alarms);

            StatusText = $"查询完成：温湿度 {tempHumidity.Count} 条，空气质量 {airQuality.Count} 条，告警 {alarms.Count} 条。";
        }
        catch (Exception ex)
        {
            StatusText = $"历史数据查询失败：{ex.Message}";
        }
    }

    private async Task DeleteAsync()
    {
        var start = (StartDate ?? DateTime.Today).Date;
        var end = (EndDate ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        if (end < start)
        {
            StatusText = "结束日期不能早于开始日期。";
            return;
        }

        var options = new HistoryDeleteOptions
        {
            DeleteTempHumidity = DeleteTempHumidity,
            DeleteAirQuality = DeleteAirQuality,
            DeleteAlarms = DeleteAlarms,
            DeleteReports = DeleteReports
        };

        if (!options.DeleteTempHumidity && !options.DeleteAirQuality && !options.DeleteAlarms && !options.DeleteReports)
        {
            StatusText = "请至少选择一种要删除的数据。";
            return;
        }

        var confirm = MessageBox.Show(
            $"确认删除 {start:yyyy-MM-dd} 至 {end:yyyy-MM-dd} 范围内选中的历史数据？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            StatusText = "已取消删除。";
            return;
        }

        try
        {
            var result = await historyService.DeleteHistoryAsync(start, end, SensorId, options);
            StatusText = $"删除完成：温湿度 {result.TempHumidityCount} 条，空气质量 {result.AirQualityCount} 条，告警 {result.AlarmCount} 条，报告 {result.ReportCount} 份。";
            await QueryAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"历史数据删除失败：{ex.Message}";
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }
}
