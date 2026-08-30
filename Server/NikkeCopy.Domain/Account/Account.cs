namespace NikkeCopy.Domain.Accounts;

public sealed class Account
{
    public long Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string NormalizedUsername { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Account()
    {
    }

    public Account(string username, string passwordHash)
    {
        Username = username.Trim();
        NormalizedUsername = NormalizeUsername(username);
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public static string NormalizeUsername(string username) =>
        username.Trim().ToUpperInvariant();
}
