using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;
using SmartEnvironmentMonitoringSystem.Wpf.Services;

namespace SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

public sealed class UserManagementViewModel : ObservableObject
{
    private readonly IUserManagementService userManagementService;
    private UserEntity? selectedUser;
    private string selectedRole = "User";
    private string resetPassword = string.Empty;
    private string currentUsername = string.Empty;
    private string statusText = "请选择用户后进行管理。";

    public UserManagementViewModel(IUserManagementService userManagementService)
    {
        this.userManagementService = userManagementService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveRoleCommand = new AsyncRelayCommand(SaveRoleAsync, HasSelectedUser);
        EnableCommand = new AsyncRelayCommand(() => SetEnabledAsync(true), CanChangeSelectedUser);
        DisableCommand = new AsyncRelayCommand(() => SetEnabledAsync(false), CanChangeSelectedUser);
        ResetPasswordCommand = new AsyncRelayCommand(ResetPasswordAsync, CanResetPassword);
    }

    public ObservableCollection<UserEntity> Users { get; } = [];

    public IReadOnlyList<string> Roles { get; } = ["Admin", "User"];

    public UserEntity? SelectedUser
    {
        get => selectedUser;
        set
        {
            if (SetProperty(ref selectedUser, value))
            {
                SelectedRole = value?.Role ?? "User";
                NotifyCommands();
            }
        }
    }

    public string SelectedRole
    {
        get => selectedRole;
        set => SetProperty(ref selectedRole, value);
    }

    public string ResetPassword
    {
        get => resetPassword;
        set
        {
            if (SetProperty(ref resetPassword, value))
            {
                ResetPasswordCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveRoleCommand { get; }

    public IAsyncRelayCommand EnableCommand { get; }

    public IAsyncRelayCommand DisableCommand { get; }

    public IAsyncRelayCommand ResetPasswordCommand { get; }

    public void SetCurrentUser(string username)
    {
        currentUsername = username;
    }

    public Task ActivateAsync()
    {
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var users = await userManagementService.QueryUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            SelectedUser = Users.FirstOrDefault();
            StatusText = $"已加载用户 {users.Count} 个。";
        }
        catch (Exception ex)
        {
            StatusText = $"用户列表加载失败：{ex.Message}";
        }
    }

    private async Task SaveRoleAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        if (SelectedUser.Username == currentUsername && SelectedRole != "Admin")
        {
            StatusText = "不能把当前登录管理员降级。";
            return;
        }

        try
        {
            await userManagementService.SetUserRoleAsync(SelectedUser.Id, SelectedRole);
            StatusText = $"已更新 {SelectedUser.Username} 的角色。";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"角色更新失败：{ex.Message}";
        }
    }

    private async Task SetEnabledAsync(bool isEnabled)
    {
        if (SelectedUser is null)
        {
            return;
        }

        if (SelectedUser.Username == currentUsername && !isEnabled)
        {
            StatusText = "不能禁用当前登录管理员。";
            return;
        }

        try
        {
            await userManagementService.SetUserEnabledAsync(SelectedUser.Id, isEnabled);
            StatusText = isEnabled ? $"已启用 {SelectedUser.Username}。" : $"已禁用 {SelectedUser.Username}。";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"用户状态更新失败：{ex.Message}";
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        if (ResetPassword.Length < 6)
        {
            StatusText = "新密码至少需要 6 个字符。";
            return;
        }

        try
        {
            await userManagementService.ResetPasswordAsync(SelectedUser.Id, ResetPassword);
            ResetPassword = string.Empty;
            StatusText = $"已重置 {SelectedUser.Username} 的密码。";
        }
        catch (Exception ex)
        {
            StatusText = $"密码重置失败：{ex.Message}";
        }
    }

    private bool HasSelectedUser()
    {
        return SelectedUser is not null;
    }

    private bool CanChangeSelectedUser()
    {
        return SelectedUser is not null;
    }

    private bool CanResetPassword()
    {
        return SelectedUser is not null && !string.IsNullOrWhiteSpace(ResetPassword);
    }

    private void NotifyCommands()
    {
        SaveRoleCommand.NotifyCanExecuteChanged();
        EnableCommand.NotifyCanExecuteChanged();
        DisableCommand.NotifyCanExecuteChanged();
        ResetPasswordCommand.NotifyCanExecuteChanged();
    }
}
