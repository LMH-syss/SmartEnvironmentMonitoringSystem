using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SmartEnvironmentMonitoringSystem.Wpf.Data;
using SmartEnvironmentMonitoringSystem.Wpf.Services;
using SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

namespace SmartEnvironmentMonitoringSystem.Wpf;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "data", "environment_monitor.db");
            var services = new ServiceCollection();
            services.AddSingleton(FreeSqlFactory.Create(dbPath));
            services.AddSingleton<DbInitializer>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAlarmService, AlarmService>();
            services.AddSingleton<IHistoryService, HistoryService>();
            services.AddSingleton<IReportService, ReportService>();
            services.AddSingleton<IExcelExportService, ExcelExportService>();
            services.AddSingleton<IUserManagementService, UserManagementService>();
            services.AddSingleton<IThresholdSettingsService, ThresholdSettingsService>();
            services.AddSingleton<ICommunicationSettingsService, CommunicationSettingsService>();
            services.AddSingleton<IMockEnvironmentDataService, MockEnvironmentDataService>();
            services.AddSingleton<ITcpServerService, TcpServerService>();
            services.AddSingleton<ISerialPortService, SerialPortService>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<HistoryViewModel>();
            services.AddSingleton<ReportViewModel>();
            services.AddSingleton<ExcelExportViewModel>();
            services.AddSingleton<UserManagementViewModel>();
            services.AddSingleton<ThresholdSettingsViewModel>();
            services.AddSingleton<CommunicationSettingsViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<DbInitializer>().InitializeAsync().GetAwaiter().GetResult();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"系统启动失败：{ex.Message}", "启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"程序运行异常：{e.Exception.Message}", "运行异常", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
    }
}
