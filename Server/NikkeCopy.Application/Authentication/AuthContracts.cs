using NikkeCopy.Domain.Accounts;

namespace NikkeCopy.Application.Authentication;

public sealed record RegisterRequest(string Username, string Password);
public sealed record LoginRequest(string Username, string Password);
public sealed record AccessToken(string Value, DateTime ExpiresAt);

public sealed record AuthResult(
    bool IsSuccess,
    string? AccessToken,
    DateTime? ExpiresAt,
    long? AccountId,
    string? Username,
    string? ErrorCode)
{
    public static AuthResult Success(Account account, AccessToken token) =>
        new(true, token.Value, token.ExpiresAt, account.Id, account.Username, null);

    public static AuthResult Failure(string errorCode) =>
        new(false, null, null, null, null, errorCode);
}

public interface IAccountRepository
{
    Task<bool> ExistsAsync(string normalizedUsername, CancellationToken cancellationToken);
    Task<Account?> FindAsync(string normalizedUsername, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface IJwtTokenService
{
    AccessToken Create(Account account);
}
