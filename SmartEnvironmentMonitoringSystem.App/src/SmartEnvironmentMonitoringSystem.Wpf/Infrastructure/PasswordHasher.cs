using System.Security.Cryptography;
using System.Text;

namespace SmartEnvironmentMonitoringSystem.Wpf.Infrastructure;

public static class PasswordHasher
{
    public static string CreateSalt()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }

    public static string HashPassword(string password, string salt)
    {
        var bytes = Encoding.UTF8.GetBytes($"{salt}:{password}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    public static bool Verify(string password, string salt, string expectedHash)
    {
        var actualHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(actualHash),
            Convert.FromHexString(expectedHash));
    }
}
