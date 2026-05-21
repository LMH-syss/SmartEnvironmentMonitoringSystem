namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IExcelExportService
{
    Task ExportAsync(DateTime startTime, DateTime endTime, string filePath);
}
