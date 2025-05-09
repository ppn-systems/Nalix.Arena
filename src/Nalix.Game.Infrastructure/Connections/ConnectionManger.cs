using Nalix.Common.Connection;
using Nalix.Common.Identity;
using Nalix.Identifiers;
using Nalix.Shared.Injection.DI;
using System;
using System.Collections.Concurrent;

namespace Nalix.Game.Infrastructure.Connections;

public sealed class ConnectionManager : SingletonBase<ConnectionManager>
{
    private readonly ConcurrentDictionary<Base36Id, IConnection> _connections = [];

    private ConnectionManager()
    {
    }

    /// <summary>
    /// Thêm một kết nối mới vào danh sách quản lý.
    /// </summary>
    public void AddConnection(IConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (_connections.ContainsKey((Base36Id)connection.Id)) return;

        _connections[(Base36Id)connection.Id] = connection;
        connection.OnCloseEvent += OnConnectionClosed;
    }

    /// <summary>
    /// Xóa kết nối khỏi danh sách quản lý.
    /// </summary>
    public void RemoveConnection(IConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (_connections.TryRemove((Base36Id)connection.Id, out var removedConnection))
        {
            removedConnection.OnCloseEvent -= OnConnectionClosed;
        }
    }

    /// <summary>
    /// Tìm kết nối theo id.
    /// </summary>
    public IConnection GetConnection(Base36Id id)
    {
        if (_connections.TryGetValue(id, out var connection))
        {
            return connection;
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra sự tồn tại của kết nối.
    /// </summary>
    public bool ConnectionExists(IEncodedId id) => _connections.ContainsKey((Base36Id)id);

    /// <summary>
    /// Kiểm tra sự tồn tại của kết nối.
    /// </summary>
    public bool ConnectionExists(Base36Id id) => _connections.ContainsKey(id);

    /// <summary>
    /// Ngắt tất cả kết nối khi server tắt.
    /// </summary>
    public void DisconnectAll()
    {
        foreach (IConnection connection in _connections.Values)
        {
            connection.Disconnect("Server shutting down.");
            connection.Dispose();
        }
    }

    /// <summary>
    /// Sự kiện khi kết nối bị đóng.
    /// </summary>
    private void OnConnectionClosed(object sender, IConnectEventArgs e)
    {
        if (sender is IConnection connection)
        {
            this.RemoveConnection(connection);
        }
    }
}