using System.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NikkeCopy.Domain.Players;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");

        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id).ValueGeneratedOnAdd();

        builder.Property(player => player.Name).HasMaxLength(30).IsRequired();

        builder.Property(player => player.CreatedAt).IsRequired();

        builder.Property(player => player.CreatedAt).IsRequired();

        builder.HasIndex(player => player.Name);
    }
}