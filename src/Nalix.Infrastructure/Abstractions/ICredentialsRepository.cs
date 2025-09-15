// Nalix.Infrastructure.Repositories/CredentialsRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nalix.Communication.Models;

namespace Nalix.Infrastructure.Abstractions;

public interface ICredentialsRepository
{
    Task<Int32> CountAsync(CancellationToken ct = default);
    Task<Boolean> ExistsByIdAsync(Int32 id, CancellationToken ct = default);
    Task<Credentials> GetByIdAsync(Int32 id, CancellationToken ct = default);
    Task<Credentials> GetByUsernameAsync(String username, CancellationToken ct = default);
    Task<IReadOnlyList<Credentials>> GetPagedAsync(Int32 pageNumber, Int32 pageSize, CancellationToken ct = default);
    Task<Int32> InsertAsync(Credentials entity, CancellationToken ct = default);
    Task<Boolean> UpdateAsync(Credentials entity, CancellationToken ct = default);
    Task<Boolean> DeleteAsync(Int32 id, CancellationToken ct = default);
}