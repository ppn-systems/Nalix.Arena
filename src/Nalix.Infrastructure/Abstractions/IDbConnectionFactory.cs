// Copyright (c) 2025 PPN.

using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Infrastructure.Abstractions;

public interface IDbConnectionFactory
{
    System.String Provider { get; }
    ValueTask<IDbConnection> OpenAsync(CancellationToken ct = default);
}
