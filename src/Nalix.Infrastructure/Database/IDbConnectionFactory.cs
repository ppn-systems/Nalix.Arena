// Copyright (c) 2025 PPN.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Nalix.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<IDbConnection> OpenAsync(CancellationToken ct = default);
}

public sealed class NpgsqlConnectionFactory(String connectionString) : IDbConnectionFactory
{
    private readonly String _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async ValueTask<IDbConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct).ConfigureAwait(false);
        return conn;
    }
}
