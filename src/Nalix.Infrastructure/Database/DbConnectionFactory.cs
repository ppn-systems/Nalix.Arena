// Copyright (c) 2025 PPN.

using Nalix.Infrastructure.Abstractions;
using Nalix.Shared.Configuration;
using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Nalix.Infrastructure.Database;

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly String _provider;
    private readonly String _cs;

    public DbConnectionFactory()
    {
        var cfg = ConfigurationManager.Instance.Get<DbConfig>()
                  ?? throw new InvalidOperationException("DbConfig not found in ConfigurationManager.");

        _provider = cfg.Provider ?? throw new ArgumentNullException(nameof(cfg.Provider));
        _cs = cfg.ConnectionString ?? throw new ArgumentNullException(nameof(cfg.ConnectionString));

        Nalix.Logging.NLogix.Host.Instance.Info(
            "[DB_FACTORY] Initialized with Provider={0}, ConnectionString={1}",
            _provider,
            MaskConnectionString(_cs));
    }

    String IDbConnectionFactory.Provider => _provider;

    public async ValueTask<IDbConnection> OpenAsync(CancellationToken ct = default)
    {
        Nalix.Logging.NLogix.Host.Instance.Debug("[DB_FACTORY] Opening connection (Provider={0})", _provider);

        try
        {
            IDbConnection conn = _provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
                ? new NpgsqlConnection(_cs)
                : _provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase)
                    ? new SQLiteConnection(_cs)
                    : throw new NotSupportedException($"Unsupported provider: {_provider}");

            if (conn is NpgsqlConnection npg)
            {
                await npg.OpenAsync(ct).ConfigureAwait(false);
                Nalix.Logging.NLogix.Host.Instance.Info("[DB_FACTORY] PostgreSQL connection opened successfully.");
                return npg;
            }

            conn.Open();
            Nalix.Logging.NLogix.Host.Instance.Info("[DB_FACTORY] SQLite connection opened successfully.");
            return conn;
        }
        catch (Exception ex)
        {
            Nalix.Logging.NLogix.Host.Instance.Error(
                "[DB_FACTORY] Failed to open connection (Provider={0}, CS={1})",
                _provider, MaskConnectionString(_cs));
            Nalix.Logging.NLogix.Host.Instance.Error("Exception: {0}", ex);
            throw;
        }
    }

    private static String MaskConnectionString(String cs)
    {
        if (String.IsNullOrEmpty(cs))
        {
            return "<empty>";
        }

        var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (Int32 i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                parts[i].Trim().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = parts[i].Split('=')[0] + "=******";
            }
        }
        return String.Join(';', parts);
    }
}
