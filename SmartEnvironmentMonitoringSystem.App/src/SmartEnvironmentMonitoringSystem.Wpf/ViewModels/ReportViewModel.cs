using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class ReportViewModel : ObservableObject
{
    private readonly IReportService reportService;
    private DateTime? startDate = DateTime.Today;
    private DateTime? endDate = DateTime.Today;
    private MonitorReportEntity? selectedReport;
    private string currentUsername = string.Empty;
    private string statusText = "请选择时间范围生成或查询报告。";

    public ReportViewModel(IReportService reportService)
    {
        this.reportService = reportService;
        GenerateCommand = new AsyncRelayCommand(GenerateAsync);
        QueryCommand = new AsyncRelayCommand(QueryAsync);
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

    public MonitorReportEntity? SelectedReport
    {
        get => selectedReport;
        set => SetProperty(ref selectedReport, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public ObservableCollection<MonitorReportEntity> Reports { get; } = [];

    public IAsyncRelayCommand GenerateCommand { get; }

    public IAsyncRelayCommand QueryCommand { get; }

    public void SetCurrentUser(string username)
    {
        currentUsername = username;
    }

    private async Task GenerateAsync()
    {
        var (start, end, valid) = GetDateRange();
        if (!valid)
        {
            return;
        }

        try
        {
            var report = await reportService.GenerateReportAsync(start, end, string.IsNullOrWhiteSpace(currentUsername) ? "Unknown" : currentUsername);
            Reports.Insert(0, report);
            SelectedReport = report;
            StatusText = $"已生成报告：{report.ReportNo}";
        }
        catch (Exception ex)
        {
            StatusText = $"报告生成失败：{ex.Message}";
        }
    }

    private async Task QueryAsync()
    {
        var (start, end, valid) = GetDateRange();
        if (!valid)
        {
            return;
        }

        try
        {
            var reports = await reportService.QueryReportsAsync(start, end);
            Reports.Clear();
            foreach (var report in reports)
            {
                Reports.Add(report);
            }

            SelectedReport = Reports.FirstOrDefault();
            StatusText = $"查询完成：报告 {reports.Count} 份。";
        }
        catch (Exception ex)
        {
            StatusText = $"报告查询失败：{ex.Message}";
        }
    }

    private (DateTime Start, DateTime End, bool Valid) GetDateRange()
    {
        var start = (StartDate ?? DateTime.Today).Date;
        var end = (EndDate ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

        if (end < start)
        {
            StatusText = "结束日期不能早于开始日期。";
            return (start, end, false);
        }

        return (start, end, true);
    }
}
