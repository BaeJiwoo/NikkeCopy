using Microsoft.EntityFrameworkCore;
using NikkeCopy.Domain.Players;

namespace NikkeCopy.Infrastructure.Persistence;

public sealed class NikkeCopyDbContext : DbContext
{
    public NikkeCopyDbContext(
        DbContextOptions<NikkeCopyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NikkeCopyDbContext).Assembly);
    }
}