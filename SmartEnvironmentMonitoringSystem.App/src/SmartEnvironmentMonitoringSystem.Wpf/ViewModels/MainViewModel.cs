using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly DashboardViewModel dashboardViewModel;
    private readonly HistoryViewModel historyViewModel;
    private readonly ReportViewModel reportViewModel;
    private readonly ExcelExportViewModel excelExportViewModel;
    private readonly UserManagementViewModel userManagementViewModel;
    private readonly ThresholdSettingsViewModel thresholdSettingsViewModel;
    private readonly CommunicationSettingsViewModel communicationSettingsViewModel;
    private readonly DispatcherTimer clockTimer;
    private object currentViewModel;
    private string currentUserName = "未登录";
    private string currentRole = "访客";
    private string currentTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    private bool isLoggedIn;
    private bool isAdmin;

    public MainViewModel(
        DashboardViewModel dashboardViewModel,
        HistoryViewModel historyViewModel,
        ReportViewModel reportViewModel,
        ExcelExportViewModel excelExportViewModel,
        UserManagementViewModel userManagementViewModel,
        ThresholdSettingsViewModel thresholdSettingsViewModel,
        CommunicationSettingsViewModel communicationSettingsViewModel,
        IAuthService authService)
    {
        this.dashboardViewModel = dashboardViewModel;
        this.historyViewModel = historyViewModel;
        this.reportViewModel = reportViewModel;
        this.excelExportViewModel = excelExportViewModel;
        this.userManagementViewModel = userManagementViewModel;
        this.thresholdSettingsViewModel = thresholdSettingsViewModel;
        this.communicationSettingsViewModel = communicationSettingsViewModel;
        LoginViewModel = new LoginViewModel(authService, CompleteLogin);
        currentViewModel = LoginViewModel;
        ShowDashboardCommand = new RelayCommand(ShowDashboard, () => IsLoggedIn);
        ShowHistoryCommand = new RelayCommand(ShowHistory, () => IsLoggedIn);
        ShowReportCommand = new RelayCommand(ShowReport, () => IsLoggedIn);
        ShowExcelExportCommand = new RelayCommand(ShowExcelExport, () => IsLoggedIn);
        ShowUserManagementCommand = new AsyncRelayCommand(ShowUserManagementAsync, () => IsLoggedIn && IsAdmin);
        ShowThresholdSettingsCommand = new AsyncRelayCommand(ShowThresholdSettingsAsync, () => IsLoggedIn && IsAdmin);
        ShowCommunicationSettingsCommand = new AsyncRelayCommand(ShowCommunicationSettingsAsync, () => IsLoggedIn && IsAdmin);

        clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        clockTimer.Tick += (_, _) => CurrentTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        clockTimer.Start();
    }

    public LoginViewModel LoginViewModel { get; }

    public IRelayCommand ShowDashboardCommand { get; }

    public IRelayCommand ShowHistoryCommand { get; }

    public IRelayCommand ShowReportCommand { get; }

    public IRelayCommand ShowExcelExportCommand { get; }

    public IAsyncRelayCommand ShowUserManagementCommand { get; }

    public IAsyncRelayCommand ShowThresholdSettingsCommand { get; }

    public IAsyncRelayCommand ShowCommunicationSettingsCommand { get; }

    public object CurrentViewModel
    {
        get => currentViewModel;
        private set => SetProperty(ref currentViewModel, value);
    }

    public string CurrentUserName
    {
        get => currentUserName;
        private set => SetProperty(ref currentUserName, value);
    }

    public string CurrentRole
    {
        get => currentRole;
        private set => SetProperty(ref currentRole, value);
    }

    public string CurrentTimeText
    {
        get => currentTimeText;
        private set => SetProperty(ref currentTimeText, value);
    }

    public bool IsLoggedIn
    {
        get => isLoggedIn;
        private set
        {
            if (SetProperty(ref isLoggedIn, value))
            {
                ShowDashboardCommand.NotifyCanExecuteChanged();
                ShowHistoryCommand.NotifyCanExecuteChanged();
                ShowReportCommand.NotifyCanExecuteChanged();
                ShowExcelExportCommand.NotifyCanExecuteChanged();
                ShowUserManagementCommand.NotifyCanExecuteChanged();
                ShowThresholdSettingsCommand.NotifyCanExecuteChanged();
                ShowCommunicationSettingsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsAdmin
    {
        get => isAdmin;
        private set
        {
            if (SetProperty(ref isAdmin, value))
            {
                OnPropertyChanged(nameof(AdminMenuVisibility));
                ShowUserManagementCommand.NotifyCanExecuteChanged();
                ShowThresholdSettingsCommand.NotifyCanExecuteChanged();
                ShowCommunicationSettingsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Visibility AdminMenuVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;

    public void Dispose()
    {
        clockTimer.Stop();
    }

    private void CompleteLogin(UserSession session)
    {
        CurrentUserName = session.Username;
        CurrentRole = session.Role;
        IsLoggedIn = true;
        IsAdmin = session.IsAdmin;
        dashboardViewModel.SetCurrentUser(session.Username, session.IsAdmin);
        historyViewModel.SetCurrentUser(session.IsAdmin);
        reportViewModel.SetCurrentUser(session.Username);
        userManagementViewModel.SetCurrentUser(session.Username);
        ShowDashboard();
        dashboardViewModel.Start();
    }

    private void ShowDashboard()
    {
        if (IsLoggedIn)
        {
            CurrentViewModel = dashboardViewModel;
        }
    }

    private void ShowHistory()
    {
        if (IsLoggedIn)
        {
            CurrentViewModel = historyViewModel;
        }
    }

    private void ShowReport()
    {
        if (IsLoggedIn)
        {
            CurrentViewModel = reportViewModel;
        }
    }

    private void ShowExcelExport()
    {
        if (IsLoggedIn)
        {
            CurrentViewModel = excelExportViewModel;
        }
    }

    private async Task ShowUserManagementAsync()
    {
        if (IsLoggedIn && IsAdmin)
        {
            CurrentViewModel = userManagementViewModel;
            await userManagementViewModel.ActivateAsync();
        }
    }

    private async Task ShowThresholdSettingsAsync()
    {
        if (IsLoggedIn && IsAdmin)
        {
            CurrentViewModel = thresholdSettingsViewModel;
            await thresholdSettingsViewModel.ActivateAsync();
        }
    }

    private async Task ShowCommunicationSettingsAsync()
    {
        if (IsLoggedIn && IsAdmin)
        {
            CurrentViewModel = communicationSettingsViewModel;
            await communicationSettingsViewModel.ActivateAsync();
        }
    }
}
