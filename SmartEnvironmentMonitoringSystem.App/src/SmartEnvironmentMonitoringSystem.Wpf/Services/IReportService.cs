using SmartEnvironmentMonitoringSystem.Wpf.Entities;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public interface IReportService
{
    Task<MonitorReportEntity> GenerateReportAsync(DateTime startTime, DateTime endTime, string createdBy);

    Task<List<MonitorReportEntity>> QueryReportsAsync(DateTime? startTime, DateTime? endTime);
}
