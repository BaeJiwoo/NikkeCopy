using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NikkeCopy.Domain.Accounts;

namespace NikkeCopy.Infrastructure.Authentication;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedOnAdd();
        builder.Property(account => account.Username).HasMaxLength(30).IsRequired();
        builder.Property(account => account.NormalizedUsername).HasMaxLength(30).IsRequired();
        builder.Property(account => account.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(account => account.CreatedAt).IsRequired();
        builder.HasIndex(account => account.NormalizedUsername).IsUnique();
    }
}
