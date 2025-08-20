using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nalix.Common.Environment;
using Nalix.Logging;
using Npgsql;
using System;
using System.IO;
using System.Net.NetworkInformation;

namespace Nalix.Infrastructure.Database;

public class AutoDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(String[] args)
    {
        NLogix.Host.Instance.Info("Starting initialization of AutoDbContext.");

        // Load cấu hình từ appsettings.json
        IConfigurationRoot configuration;

        try
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error("Error loading configuration from appsettings.json.", ex);
            throw;
        }

        // Đọc loại database và connection string
        String dbType = configuration["DatabaseType"] ?? "PostgreSQL";
        String connectionString = configuration.GetConnectionString("DefaultConnection");

        // Kiểm tra kết nối đến database
        if (!dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase) &&
            !CanConnectToDatabase(connectionString))
        {
            NLogix.Host.Instance.Error($"Cannot connect to the database at {connectionString}");
            throw new InvalidOperationException($"Cannot connect to the database at {connectionString}");
        }

        DbContextOptionsBuilder<GameDbContext> optionsBuilder = new();

        try
        {
            if (dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                _ = optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    _ = npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    _ = npgsqlOptions.CommandTimeout(60);
                    _ = npgsqlOptions.MigrationsHistoryTable("__MigrationsHistory", "public");
                    _ = npgsqlOptions.UseRelationalNulls();
                    _ = npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .EnableSensitiveDataLogging(false)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors(false)
                .EnableServiceProviderCaching();

                NLogix.Host.Instance.Info("DbContext configured for PostgreSQL.");
            }
            else if (dbType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                _ = optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                {
                    _ = sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    _ = sqlOptions.CommandTimeout(60);
                    _ = sqlOptions.MigrationsHistoryTable("__MigrationsHistory", "dbo");
                    _ = sqlOptions.UseRelationalNulls();
                })
                .EnableThreadSafetyChecks()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                NLogix.Host.Instance.Info("DbContext configured for SQL Server.");
            }
            else if (dbType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                _ = optionsBuilder.UseSqlite(
                    $"Data Source={Directories.BaseAssetsDirectory}\\GM.db",
                    sqliteOptions =>
                    {
                        _ = sqliteOptions.CommandTimeout(60);
                        _ = sqliteOptions.MigrationsHistoryTable("__MigrationsHistory");
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableServiceProviderCaching();
                //.LogTo(Console.WriteLine, LogLevel.Information);

                NLogix.Host.Instance.Info("DbContext configured for SQLite.");
            }
            else
            {
                NLogix.Host.Instance.Warn($"Unsupported database type: {dbType}");
                throw new InvalidOperationException($"Unsupported database type: {dbType}");
            }
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error($"Error configuring DbContext for {dbType}.", ex);
            throw;
        }

        GameDbContext dbContext = new(optionsBuilder.Options);
        NLogix.Host.Instance.Info("AutoDbContext successfully created.");
        return dbContext;
    }

    private static Boolean CanConnectToDatabase(String connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            String host = builder.Host;
            Int32 port = builder.Port;

            NLogix.Host.Instance.Info($"Pinging database server {host}:{port}...");

            using var ping = new Ping();
            PingReply reply = ping.Send(host, 3000); // Timeout 1 giây

            if (reply.Status == IPStatus.Success)
            {
                NLogix.Host.Instance.Info($"Ping to {host} successful.");
                return true;
            }

            NLogix.Host.Instance.Error($"Ping to {host} failed: {reply.Status}");
            return false;
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error("Error pinging database server.", ex);
            return false;
        }
    }
}