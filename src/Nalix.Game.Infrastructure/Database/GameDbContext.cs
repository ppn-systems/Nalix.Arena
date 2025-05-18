using Microsoft.EntityFrameworkCore;
using Nalix.Game.Shared.Security;

namespace Nalix.Game.Infrastructure.Database;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options), IGameDbContext
{
    public DbSet<Credentials> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        ConfigureAccount(modelBuilder);
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credentials>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasIndex(a => a.Username).IsUnique();

            entity.Property(a => a.Username)
                .HasMaxLength(Credentials.UsernameMaxLength)
                .IsRequired();

            entity.Property(a => a.Role)
                .HasConversion<byte>()
                .IsRequired();
        });
    }
}