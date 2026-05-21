namespace SmartEnvironmentMonitoringSystem.Wpf.Models;

public sealed class AuthResult
{
    private AuthResult(bool succeeded, string message, UserSession? session)
    {
        Succeeded = succeeded;
        Message = message;
        Session = session;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public UserSession? Session { get; }

    public static AuthResult Success(UserSession session)
    {
        return new AuthResult(true, "操作成功", session);
    }

    public static AuthResult Failed(string message)
    {
        return new AuthResult(false, message, null);
    }
}
