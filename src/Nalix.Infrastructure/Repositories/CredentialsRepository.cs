using Dapper;
using Nalix.Communication.Models;           // Credentials entity (Id, Username, Salt, Hash, Role, ...)
using Nalix.Infrastructure.Abstractions;    // IDbConnectionFactory, ICredentialsRepository
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Infrastructure.Repositories;

/// <summary>
/// High-performance repository for the Account/Credentials table.
/// Optimizations:
/// - Minimal column reads for auth flow (GetAuthViewByUsernameAsync).
/// - Atomic updates for login fail/reset (no full-row UPDATE).
/// - EXISTS for existence checks to avoid data transfer.
/// - Keyset pagination helper to avoid deep OFFSET.
/// - Optional PostgreSQL RETURNING for single round-trip.
/// - Portable quoting based on provider (PostgreSQL vs SQLite).
/// </summary>
public sealed class CredentialsRepository(IDbConnectionFactory factory)
{
    private readonly IDbConnectionFactory _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));

    // Quote identifier for PostgreSQL only. SQLite default identifiers are fine unquoted here.
    private System.String Q(System.String ident)
        => _factory.Provider.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase) ? $"\"{ident}\"" : ident;

    private System.String Table => _factory.Provider.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase) ? "\"Account\"" : "Account";

    #region Basic CRUD

    public async Task<System.Int32> InsertAsync(Credentials e, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);

        if (_factory.Provider.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase))
        {
            // Use RETURNING for single round-trip.
            System.String sql = $@"
                INSERT INTO {Table}
                ({Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                 {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                 {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")})
                VALUES (@Username, @Salt, @Hash, @Role,
                        @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                        @LastFailedLoginAt, @IsActive, @CreatedAt)
                RETURNING {Q("Id")};";

            return await conn.ExecuteScalarAsync<System.Int32>(sql, e).ConfigureAwait(false);
        }
        else
        {
            // SQLite path. last_insert_rowid() after insert.
            System.String sql = $@"
                INSERT INTO {Table}
                ({Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                 {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                 {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")})
                VALUES (@Username, @Salt, @Hash, @Role,
                        @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                        @LastFailedLoginAt, @IsActive, @CreatedAt);
                SELECT last_insert_rowid();";
            return await conn.ExecuteScalarAsync<System.Int32>(sql, e).ConfigureAwait(false);
        }
    }

    public async Task<System.Boolean> UpdateAsync(Credentials e, CancellationToken ct = default)
    {
        // Keep for backward-compat (full-row update). Prefer granular APIs below.
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            UPDATE {Table} SET
                {Q("Username")} = @Username,
                {Q("Salt")} = @Salt,
                {Q("Hash")} = @Hash,
                {Q("Role")} = @Role,
                {Q("FailedLoginCount")} = @FailedLoginCount,
                {Q("LastLoginAt")} = @LastLoginAt,
                {Q("LastLogoutAt")} = @LastLogoutAt,
                {Q("LastFailedLoginAt")} = @LastFailedLoginAt,
                {Q("IsActive")} = @IsActive,
                {Q("CreatedAt")} = @CreatedAt
            WHERE {Q("Id")} = @Id;";
        return await conn.ExecuteAsync(sql, e).ConfigureAwait(false) > 0;
    }

    public async Task<System.Boolean> DeleteAsync(System.Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $"DELETE FROM {Table} WHERE {Q("Id")} = @id;";
        return await conn.ExecuteAsync(sql, new { id }).ConfigureAwait(false) > 0;
    }

    public async Task<Credentials> GetByIdAsync(System.Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            SELECT {Q("Id")}, {Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                   {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                   {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")}
            FROM {Table}
            WHERE {Q("Id")} = @id;";
        return await conn.QuerySingleOrDefaultAsync<Credentials>(sql, new { id }).ConfigureAwait(false);
    }

    public async Task<Credentials> GetByUsernameAsync(System.String username, CancellationToken ct = default)
    {
        // Full entity fetch. For login, prefer GetAuthViewByUsernameAsync to read fewer columns.
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            SELECT {Q("Id")}, {Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                   {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                   {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")}
            FROM {Table}
            WHERE {Q("Username")} = @username
            LIMIT 1;";
        return await conn.QuerySingleOrDefaultAsync<Credentials>(sql, new { username }).ConfigureAwait(false);
    }

    public async Task<System.Boolean> ExistsByIdAsync(System.Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $"SELECT EXISTS(SELECT 1 FROM {Table} WHERE {Q("Id")} = @id);";
        return await conn.ExecuteScalarAsync<System.Boolean>(sql, new { id }).ConfigureAwait(false);
    }

    public async Task<System.Int64> CountAsync(CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $"SELECT COUNT(*) FROM {Table};";
        return await conn.ExecuteScalarAsync<System.Int64>(sql).ConfigureAwait(false);
    }

    #endregion

    #region Auth-focused APIs (minimal columns + atomic updates)

    /// <summary>
    /// Minimal projection for authentication: only columns needed for verify & lockout.
    /// </summary>
    public async Task<(System.Int32 Id, System.Byte[] Salt, System.Byte[] Hash, System.Boolean IsActive, System.Int32 FailedCount, System.DateTime? LastFailedAt, System.Int32 Role)?>
        GetAuthViewByUsernameAsync(System.String username, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            SELECT {Q("Id")}                    AS Id,
                   {Q("Salt")}                  AS Salt,
                   {Q("Hash")}                  AS Hash,
                   {Q("IsActive")}              AS IsActive,
                   {Q("FailedLoginCount")}      AS FailedCount,
                   {Q("LastFailedLoginAt")}     AS LastFailedAt,
                   {Q("Role")}                  AS Role
            FROM {Table}
            WHERE {Q("Username")} = @username
            LIMIT 1;";
        return await conn.QuerySingleOrDefaultAsync<(System.Int32, System.Byte[], System.Byte[], System.Boolean, System.Int32, System.DateTime?, System.Int32)?>(sql, new { username }).ConfigureAwait(false);
    }

    /// <summary>
    /// Atomically increments failed login counters and stamps last failed time.
    /// </summary>
    public async Task<System.Int32> IncrementFailedAsync(System.Int32 id, System.DateTime utcNow, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            UPDATE {Table}
            SET {Q("FailedLoginCount")} = {Q("FailedLoginCount")} + 1,
                {Q("LastFailedLoginAt")} = @utcNow
            WHERE {Q("Id")} = @id;";
        return await conn.ExecuteAsync(sql, new { id, utcNow }).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets failed counters and stamps last login time atomically.
    /// </summary>
    public async Task<System.Int32> ResetFailedAndStampLoginAsync(System.Int32 id, System.DateTime utcNow, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            UPDATE {Table}
            SET {Q("FailedLoginCount")} = 0,
                {Q("LastFailedLoginAt")} = NULL,
                {Q("LastLoginAt")} = @utcNow
            WHERE {Q("Id")} = @id;";
        return await conn.ExecuteAsync(sql, new { id, utcNow }).ConfigureAwait(false);
    }

    /// <summary>
    /// Stamps logout time only.
    /// </summary>
    public async Task<System.Int32> StampLogoutAsync(System.String username, System.DateTime utcNow, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);
        System.String sql = $@"
            UPDATE {Table}
            SET {Q("LastLogoutAt")} = @utcNow
            WHERE {Q("Username")} = @username;";

        return await conn.ExecuteAsync(sql, new { username, utcNow }).ConfigureAwait(false);
    }

    /// <summary>
    /// Insert with upsert/ignore on Username (requires unique index on Username).
    /// Returns new Id or 0 when ignored (SQLite path).
    /// </summary>
    public async Task<System.Int32> InsertOrIgnoreAsync(Credentials e, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);

        if (_factory.Provider.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase))
        {
            System.String sql = $@"
                INSERT INTO {Table}
                ({Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                 {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                 {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")})
                VALUES (@Username, @Salt, @Hash, @Role,
                        @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                        @LastFailedLoginAt, @IsActive, @CreatedAt)
                ON CONFLICT ({Q("Username")}) DO NOTHING
                RETURNING {Q("Id")};";
            return await conn.ExecuteScalarAsync<System.Int32?>(sql, e).ConfigureAwait(false) ?? 0;
        }
        else
        {
            System.String sql = $@"
                INSERT INTO {Table}
                ({Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                 {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                 {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")})
                VALUES (@Username, @Salt, @Hash, @Role,
                        @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                        @LastFailedLoginAt, @IsActive, @CreatedAt)
                ON CONFLICT ({Q("Username")}) DO NOTHING;
                SELECT CASE WHEN changes() = 0 THEN 0 ELSE last_insert_rowid() END;";
            return await conn.ExecuteScalarAsync<System.Int32>(sql, e).ConfigureAwait(false);
        }
    }

    #endregion

    #region Password reset helpers

    /// <summary>
    /// Minimal projection for password change flow:
    /// Only Id, Salt, Hash, IsActive are fetched.
    /// </summary>
    public async Task<(System.Int32 Id, System.Byte[] Salt, System.Byte[] Hash, System.Boolean IsActive)?>
        GetForPasswordChangeByUsernameAsync(System.String username, CancellationToken ct = default)
    {
        if (System.String.IsNullOrWhiteSpace(username))
        {
            throw new System.ArgumentException("username is required", nameof(username));
        }

        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);

        var sql = $@"
            SELECT {Q("Id")}        AS Id,
                   {Q("Salt")}      AS Salt,
                   {Q("Hash")}      AS Hash,
                   {Q("IsActive")}  AS IsActive
            FROM {Table}
            WHERE {Q("Username")} = @username
            LIMIT 1;";

        return await conn.QuerySingleOrDefaultAsync<(System.Int32, System.Byte[], System.Byte[], System.Boolean)?>(sql, new { username })
                         .ConfigureAwait(false);
    }

    /// <summary>
    /// Atomic password update:
    /// Update Salt & Hash only when the current Hash matches the expected OLD hash.
    /// Returns number of affected rows (0 => mismatch / concurrent change).
    /// </summary>
    public async Task<System.Int32> UpdatePasswordIfMatchesAsync(
        System.Int32 id,
        System.Byte[] expectedOldHash,
        System.Byte[] newSalt,
        System.Byte[] newHash,
        CancellationToken ct = default)
    {
        System.ArgumentNullException.ThrowIfNull(newSalt);
        System.ArgumentNullException.ThrowIfNull(newHash);
        System.ArgumentNullException.ThrowIfNull(expectedOldHash);
        System.ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);

        var sql = $@"
            UPDATE {Table}
            SET {Q("Salt")} = @newSalt,
                {Q("Hash")} = @newHash
            WHERE {Q("Id")} = @id
              AND {Q("Hash")} = @expectedOldHash;";

        return await conn.ExecuteAsync(sql, new { id, expectedOldHash, newSalt, newHash })
                         .ConfigureAwait(false);
    }

    /// <summary>
    /// Force password update (no old-hash check). Use carefully.
    /// Returns number of affected rows.
    /// </summary>
    public async Task<System.Int32> UpdatePasswordAsync(
        System.Int32 id,
        System.Byte[] newSalt,
        System.Byte[] newHash,
        CancellationToken ct = default)
    {
        System.ArgumentNullException.ThrowIfNull(newSalt);
        System.ArgumentNullException.ThrowIfNull(newHash);
        System.ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        using var conn = await _factory.OpenAsync(ct).ConfigureAwait(false);

        var sql = $@"
            UPDATE {Table}
            SET {Q("Salt")} = @newSalt,
                {Q("Hash")} = @newHash
            WHERE {Q("Id")} = @id;";

        return await conn.ExecuteAsync(sql, new { id, newSalt, newHash })
                         .ConfigureAwait(false);
    }

    #endregion Password reset helpers
}
