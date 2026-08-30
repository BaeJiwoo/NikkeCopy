using Microsoft.EntityFrameworkCore;
using NikkeCopy.Application.Authentication;
using NikkeCopy.Domain.Accounts;
using NikkeCopy.Infrastructure.Persistence;

namespace NikkeCopy.Infrastructure.Authentication;

public sealed class AccountRepository : IAccountRepository
{
    private readonly NikkeCopyDbContext _dbContext;

    public AccountRepository(NikkeCopyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(string normalizedUsername, CancellationToken cancellationToken) =>
        _dbContext.Accounts.AnyAsync(
            account => account.NormalizedUsername == normalizedUsername,
            cancellationToken);

    public Task<Account?> FindAsync(string normalizedUsername, CancellationToken cancellationToken) =>
        _dbContext.Accounts.SingleOrDefaultAsync(
            account => account.NormalizedUsername == normalizedUsername,
            cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
