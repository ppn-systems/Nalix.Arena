// Copyright (c) 2025 PPN.

using Nalix.Infrastructure.Abstractions;
using Nalix.Shared.Configuration;
using Npgsql;
using Microsoft.Data.Sqlite; // <-- use Microsoft.Data.Sqlite
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Infrastructure.Database;

/// <summary>
/// Factory for database connections (PostgreSQL / SQLite).
/// Uses Microsoft.Data.Sqlite for SQLite to avoid native DLL issues.
/// </summary>
public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly System.String _provider;
    private readonly System.String _cs;

    public DbConnectionFactory()
    {
        var cfg = ConfigurationManager.Instance.Get<DbConfig>()
                  ?? throw new System.InvalidOperationException("DbConfig not found in ConfigurationManager.");

        _provider = cfg.Provider ?? throw new System.ArgumentNullException(nameof(cfg.Provider));
        _cs = cfg.ConnectionString ?? throw new System.ArgumentNullException(nameof(cfg.ConnectionString));

        Nalix.Logging.NLogix.Host.Instance.Info(
            "[DB_FACTORY] Initialized with Provider={0}, ConnectionString={1}",
            _provider,
            MaskConnectionString(_cs));
    }

    System.String IDbConnectionFactory.Provider => _provider;

    public async ValueTask<IDbConnection> OpenAsync(CancellationToken ct = default)
    {
        Nalix.Logging.NLogix.Host.Instance.Debug("[DB_FACTORY] Opening connection (Provider={0})", _provider);

        try
        {
            switch (_provider.ToLowerInvariant())
            {
                case "postgresql":
                    {
                        var npg = new NpgsqlConnection(_cs);
                        await npg.OpenAsync(ct).ConfigureAwait(false);
                        Nalix.Logging.NLogix.Host.Instance.Info("[DB_FACTORY] PostgreSQL connection opened.");
                        return npg;
                    }

                case "sqlite":
                    {
                        // Ensure native e_sqlite3 is initialized (harmless if called multiple times)
                        try { SQLitePCL.Batteries_V2.Init(); } catch { /* ignore */ }

                        var sqlite = CreateAndPrepareSqliteConnection(_cs);
                        await sqlite.OpenAsync(ct).ConfigureAwait(false);

                        await ApplySqliteRuntimePragmasAsync(sqlite, ct).ConfigureAwait(false);
                        Nalix.Logging.NLogix.Host.Instance.Trace("[DB_FACTORY] SQLite connection opened.");
                        return sqlite;
                    }

                default:
                    throw new System.NotSupportedException($"Unsupported provider: {_provider}");
            }
        }
        catch (System.Exception ex)
        {
            Nalix.Logging.NLogix.Host.Instance.Error(
                "[DB_FACTORY] Failed to open connection (Provider={0}, CS={1})",
                _provider, MaskConnectionString(_cs));

            Nalix.Logging.NLogix.Host.Instance.Error(
                "Exception: {0}: {1} (HResult=0x{2:X8}){3}{4}",
                ex.GetType().FullName, ex.Message, ex.HResult,
                System.Environment.NewLine, ex.StackTrace);

            if (ex.InnerException != null)
            {
                Nalix.Logging.NLogix.Host.Instance.Error("Inner: {0}", ex.InnerException);
            }

            throw;
        }
    }

    /// <summary>
    /// Build a SqliteConnection with normalized path and safe defaults.
    /// </summary>
    private static SqliteConnection CreateAndPrepareSqliteConnection(System.String rawCs)
    {
        var builder = new SqliteConnectionStringBuilder(rawCs);

        // Normalize DataSource path and ensure folder exists
        var dataSource = builder.DataSource ?? System.String.Empty;
        if (!Path.IsPathFullyQualified(dataSource))
        {
            dataSource = Path.GetFullPath(dataSource);
        }

        dataSource = dataSource.Replace('/', Path.DirectorySeparatorChar);

        var dir = Path.GetDirectoryName(dataSource);
        if (!System.String.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        builder.DataSource = dataSource;

        // Connection defaults:
        // - ReadWriteCreate: create the DB file if missing
        // - Shared cache is helpful with WAL
        builder.Mode = SqliteOpenMode.ReadWriteCreate;
        builder.Cache = SqliteCacheMode.Shared;

        // DefaultTimeout is the command timeout (seconds). Keep it reasonable.
        if (!builder.TryGetValue("Default Timeout", out _))
        {
            builder.DefaultTimeout = 30;
        }

        // Note: Pooling is enabled by default in Microsoft.Data.Sqlite

        return new SqliteConnection(builder.ToString());
    }

    /// <summary>
    /// Apply runtime PRAGMAs (WAL + busy_timeout) after opening the connection.
    /// </summary>
    private static async Task ApplySqliteRuntimePragmasAsync(SqliteConnection conn, CancellationToken ct)
    {
        await using SqliteCommand cmd = conn.CreateCommand();

        // Enable WAL for better concurrency
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        _ = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

        // Avoid 'database is locked' storms
        cmd.CommandText = "PRAGMA busy_timeout=30000;"; // 30s
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        // Optional performance trade-offs:
        // cmd.CommandText = "PRAGMA synchronous=NORMAL;"; await cmd.ExecuteNonQueryAsync(ct);
        // cmd.CommandText = "PRAGMA temp_store=MEMORY;"; await cmd.ExecuteNonQueryAsync(ct);
    }

    private static System.String MaskConnectionString(System.String cs)
    {
        if (System.String.IsNullOrEmpty(cs))
        {
            return "<empty>";
        }

        var parts = cs.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        for (System.Int32 i = 0; i < parts.Length; i++)
        {
            var p = parts[i].Trim();
            if (p.StartsWith("Password=", System.StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("Pwd=", System.StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = p.Split('=')[0] + "=******";
            }
        }
        return System.String.Join(';', parts);
    }
}
