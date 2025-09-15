// Copyright (c) 2025 PPN.

using Nalix.Infrastructure.Abstractions;
using System;
using System.Threading.Tasks;

namespace Nalix.Infrastructure.Database;

public static class DbInitializer
{
    /// <summary>
    /// Đảm bảo schema tồn tại. Nếu chưa có sẽ tự tạo.
    /// Hỗ trợ SQLite và PostgreSQL.
    /// </summary>
    public static async Task EnsureDatabaseInitializedAsync(
        IDbConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using var conn = await factory.OpenAsync().ConfigureAwait(false);
        using var cmd = conn.CreateCommand();

        if (factory.Provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Account (
  Id                INTEGER PRIMARY KEY AUTOINCREMENT,
  Username          TEXT NOT NULL UNIQUE,
  Salt              BLOB NOT NULL,
  Hash              BLOB NOT NULL,
  Role              INTEGER NOT NULL DEFAULT 0,
  FailedLoginCount  INTEGER NOT NULL DEFAULT 0,
  LastLoginAt       TEXT NULL,
  LastLogoutAt      TEXT NULL,
  LastFailedLoginAt TEXT NULL,
  IsActive          INTEGER NOT NULL DEFAULT 0,
  CreatedAt         TEXT NOT NULL DEFAULT (datetime('now'))
);
CREATE INDEX IF NOT EXISTS ix_account_username ON Account(Username);";
        }
        else if (factory.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            cmd.CommandText = """
CREATE TABLE IF NOT EXISTS "Account" (
  "Id"              SERIAL PRIMARY KEY,
  "Username"        VARCHAR(20) NOT NULL UNIQUE,
  "Salt"            BYTEA NOT NULL,
  "Hash"            BYTEA NOT NULL,
  "Role"            SMALLINT NOT NULL DEFAULT 0,
  "FailedLoginCount" INT NOT NULL DEFAULT 0,
  "LastLoginAt"     TIMESTAMP NULL,
  "LastLogoutAt"    TIMESTAMP NULL,
  "LastFailedLoginAt" TIMESTAMP NULL,
  "IsActive"        BOOLEAN NOT NULL DEFAULT FALSE,
  "CreatedAt"       TIMESTAMP NOT NULL DEFAULT (NOW())
);
CREATE INDEX IF NOT EXISTS ix_account_username ON "Account"("Username");
""";
        }
        else
        {
            throw new NotSupportedException($"Unsupported provider: {factory.Provider}");
        }

        _ = cmd.ExecuteNonQuery();
    }
}
