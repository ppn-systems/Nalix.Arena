using Dapper;
using Nalix.Communication.Models;
using Nalix.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Infrastructure.Repositories;

public sealed class CredentialsRepository(IDbConnectionFactory factory) : ICredentialsRepository
{
    private readonly IDbConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    private String Q(String ident) => _factory.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
        ? $"\"{ident}\"" : ident; // SQLite mặc định không cần quote kiểu này

    private static String Table => "Account"; // theo [Table("Account")] của Credentials :contentReference[oaicite:10]{index=10}

    public async Task<Int32> CountAsync(CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        return await conn.ExecuteScalarAsync<Int32>($"SELECT COUNT(1) FROM {Table}");
    }

    public async Task<Boolean> ExistsByIdAsync(Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var sql = $"SELECT 1 FROM {Table} WHERE {Q("Id")} = @id LIMIT 1";
        return await conn.ExecuteScalarAsync<Int32?>(sql, new { id }) is not null;
    }

    public async Task<Credentials> GetByIdAsync(Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var sql = $@"SELECT {Q("Id")}, {Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                            {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                            {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")}
                     FROM {Table} WHERE {Q("Id")} = @id";
        return await conn.QuerySingleOrDefaultAsync<Credentials>(sql, new { id });
    }

    public async Task<Credentials> GetByUsernameAsync(String username, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var sql = $@"SELECT {Q("Id")}, {Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                            {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                            {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")}
                     FROM {Table} WHERE {Q("Username")} = @username";
        return await conn.QuerySingleOrDefaultAsync<Credentials>(sql, new { username });
    }

    public async Task<IReadOnlyList<Credentials>> GetPagedAsync(Int32 pageNumber, Int32 pageSize, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var offset = (pageNumber - 1) * pageSize;

        var sqlPg = $@"SELECT {Q("Id")}, {Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                              {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                              {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")}
                       FROM {Table}
                       ORDER BY {Q("Id")}
                       LIMIT @pageSize OFFSET @offset";

        // SQLite cũng hỗ trợ LIMIT .. OFFSET, nên dùng chung.
        var rows = await conn.QueryAsync<Credentials>(sqlPg, new { pageSize, offset });
        return rows.AsList();
    }

    public async Task<Int32> InsertAsync(Credentials e, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);

        // Password là [NotMapped] (không lưu DB) theo model hiện tại :contentReference[oaicite:11]{index=11}
        var sqlPg = $@"
            INSERT INTO {Table} ({Q("Username")}, {Q("Salt")}, {Q("Hash")}, {Q("Role")},
                                  {Q("FailedLoginCount")}, {Q("LastLoginAt")}, {Q("LastLogoutAt")},
                                  {Q("LastFailedLoginAt")}, {Q("IsActive")}, {Q("CreatedAt")})
            VALUES (@Username, @Salt, @Hash, @Role,
                    @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                    @LastFailedLoginAt, @IsActive, @CreatedAt)
            RETURNING {Q("Id")};";

        if (_factory.Provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            // SQLite dùng last_insert_rowid()
            var sqlLite = $@"
                INSERT INTO {Table} (Username, Salt, Hash, Role,
                                     FailedLoginCount, LastLoginAt, LastLogoutAt,
                                     LastFailedLoginAt, IsActive, CreatedAt)
                VALUES (@Username, @Salt, @Hash, @Role,
                        @FailedLoginCount, @LastLoginAt, @LastLogoutAt,
                        @LastFailedLoginAt, @IsActive, @CreatedAt);
                SELECT last_insert_rowid();";
            return await conn.ExecuteScalarAsync<Int32>(sqlLite, e);
        }

        return await conn.ExecuteScalarAsync<Int32>(sqlPg, e);
    }

    public async Task<Boolean> UpdateAsync(Credentials e, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var sql = $@"
            UPDATE {Table}
            SET {Q("Username")}=@Username,
                {Q("Salt")}=@Salt,
                {Q("Hash")}=@Hash,
                {Q("Role")}=@Role,
                {Q("FailedLoginCount")}=@FailedLoginCount,
                {Q("LastLoginAt")}=@LastLoginAt,
                {Q("LastLogoutAt")}=@LastLogoutAt,
                {Q("LastFailedLoginAt")}=@LastFailedLoginAt,
                {Q("IsActive")}=@IsActive,
                {Q("CreatedAt")}=@CreatedAt
            WHERE {Q("Id")}=@Id";
        return await conn.ExecuteAsync(sql, e) > 0;
    }

    public async Task<Boolean> DeleteAsync(Int32 id, CancellationToken ct = default)
    {
        using var conn = await _factory.OpenAsync(ct);
        var sql = $"DELETE FROM {Table} WHERE {Q("Id")}=@id";
        return await conn.ExecuteAsync(sql, new { id }) > 0;
    }
}

