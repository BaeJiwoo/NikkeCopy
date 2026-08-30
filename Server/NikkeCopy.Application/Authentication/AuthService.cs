using NikkeCopy.Domain.Accounts;

namespace NikkeCopy.Application.Authentication;

public sealed class AuthService
{
    private readonly IAccountRepository _accounts;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokens;

    public AuthService(
        IAccountRepository accounts,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokens)
    {
        _accounts = accounts;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        if (username.Length is < 3 or > 30 || password.Length is < 8 or > 128)
        {
            return AuthResult.Failure("invalid_credentials_format");
        }

        var normalized = Account.NormalizeUsername(username);
        if (await _accounts.ExistsAsync(normalized, cancellationToken))
        {
            return AuthResult.Failure("username_unavailable");
        }

        var account = new Account(username, _passwordHasher.Hash(password));
        await _accounts.AddAsync(account, cancellationToken);
        return AuthResult.Success(account, _tokens.Create(account));
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        var account = await _accounts.FindAsync(
            Account.NormalizeUsername(username), cancellationToken);

        if (account is null || !_passwordHasher.Verify(request.Password ?? string.Empty, account.PasswordHash))
        {
            return AuthResult.Failure("invalid_credentials");
        }

        return AuthResult.Success(account, _tokens.Create(account));
    }
}
