using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Models;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthService authService;
    private readonly Action<UserSession> loginCompleted;
    private string username = "admin";
    private string password = "admin123";
    private string message = "默认管理员：admin / admin123";
    private bool isBusy;

    public LoginViewModel(IAuthService authService, Action<UserSession> loginCompleted)
    {
        this.authService = authService;
        this.loginCompleted = loginCompleted;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanSubmit);
        RegisterCommand = new AsyncRelayCommand(RegisterAsync, CanSubmit);
    }

    public string Username
    {
        get => username;
        set
        {
            if (SetProperty(ref username, value))
            {
                NotifyCommandStateChanged();
            }
        }
    }

    public string Password
    {
        get => password;
        set
        {
            if (SetProperty(ref password, value))
            {
                NotifyCommandStateChanged();
            }
        }
    }

    public string Message
    {
        get => message;
        private set => SetProperty(ref message, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                NotifyCommandStateChanged();
            }
        }
    }

    public IAsyncRelayCommand LoginCommand { get; }

    public IAsyncRelayCommand RegisterCommand { get; }

    private async Task LoginAsync()
    {
        await RunAuthActionAsync(() => authService.LoginAsync(Username, Password), true);
    }

    private async Task RegisterAsync()
    {
        await RunAuthActionAsync(() => authService.RegisterUserAsync(Username, Password), false);
    }

    private async Task RunAuthActionAsync(Func<Task<AuthResult>> action, bool enterAfterSuccess)
    {
        IsBusy = true;
        try
        {
            var result = await action();
            Message = result.Message;
            if (result.Succeeded && result.Session is not null)
            {
                if (enterAfterSuccess)
                {
                    loginCompleted(result.Session);
                }
                else
                {
                    Message = "注册成功，请使用新账号登录。";
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSubmit()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password);
    }

    private void NotifyCommandStateChanged()
    {
        LoginCommand.NotifyCanExecuteChanged();
        RegisterCommand.NotifyCanExecuteChanged();
    }
}
