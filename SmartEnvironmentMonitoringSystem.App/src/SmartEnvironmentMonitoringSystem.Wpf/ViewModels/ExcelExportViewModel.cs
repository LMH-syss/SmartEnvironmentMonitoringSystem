using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class ExcelExportViewModel : ObservableObject
{
    private readonly IExcelExportService excelExportService;
    private DateTime? startDate = DateTime.Today;
    private DateTime? endDate = DateTime.Today;
    private string outputPath = string.Empty;
    private string statusText = "请选择时间范围和导出位置。";
    private bool isExporting;

    public ExcelExportViewModel(IExcelExportService excelExportService)
    {
        this.excelExportService = excelExportService;
        BrowseCommand = new RelayCommand(Browse);
        ExportCommand = new AsyncRelayCommand(ExportAsync, CanExport);
    }

    public DateTime? StartDate
    {
        get => startDate;
        set
        {
            if (SetProperty(ref startDate, value))
            {
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public DateTime? EndDate
    {
        get => endDate;
        set
        {
            if (SetProperty(ref endDate, value))
            {
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string OutputPath
    {
        get => outputPath;
        set
        {
            if (SetProperty(ref outputPath, value))
            {
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public bool IsExporting
    {
        get => isExporting;
        private set
        {
            if (SetProperty(ref isExporting, value))
            {
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IRelayCommand BrowseCommand { get; }

    public IAsyncRelayCommand ExportCommand { get; }

    private bool CanExport()
    {
        return !IsExporting && StartDate.HasValue && EndDate.HasValue;
    }

    private void Browse()
    {
        var dialog = new SaveFileDialog
        {
            Title = "选择 Excel 导出文件",
            FileName = $"环境监测数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            DefaultExt = ".xlsx",
            Filter = "Excel 工作簿 (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    private async Task ExportAsync()
    {
        var (start, end, valid) = GetDateRange();
        if (!valid)
        {
            return;
        }

        var filePath = NormalizeOutputPath(OutputPath);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Browse();
            filePath = NormalizeOutputPath(OutputPath);
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusText = "已取消导出。";
            return;
        }

        try
        {
            IsExporting = true;
            StatusText = "正在导出 Excel 文件...";

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await excelExportService.ExportAsync(start, end, filePath);
            OutputPath = filePath;
            StatusText = $"导出完成：{filePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"导出失败：{ex.Message}";
        }
        finally
        {
            IsExporting = false;
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

    private static string NormalizeOutputPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmedPath = path.Trim();
        return Path.GetExtension(trimmedPath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
            ? trimmedPath
            : $"{trimmedPath}.xlsx";
    }
}
