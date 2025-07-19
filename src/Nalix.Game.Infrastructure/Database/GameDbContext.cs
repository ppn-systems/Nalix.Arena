using Microsoft.EntityFrameworkCore;
using Nalix.Game.Shared.Security;

namespace Nalix.Game.Infrastructure.Database;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options), IGameDbContext
{
    public DbSet<Credentials> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        ConfigureAccount(modelBuilder);
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Credentials>(entity =>
        {
            _ = entity.HasKey(a => a.Id);

            _ = entity.HasIndex(a => a.Username).IsUnique();

            _ = entity.Property(a => a.Username)
                .HasMaxLength(Credentials.UsernameMaxLength)
                .IsRequired();

            _ = entity.Property(a => a.Role)
                .HasConversion<System.Byte>()
                .IsRequired();
        });
    }
}