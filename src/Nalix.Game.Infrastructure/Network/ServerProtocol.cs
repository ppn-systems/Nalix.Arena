using Nalix.Common.Connection;
using Nalix.Game.Infrastructure.Connections;
using Nalix.Logging;
using Nalix.Network.Dispatch;
using Nalix.Network.Package;
using Nalix.Network.Protocols;
using System;
using System.Threading;

namespace Nalix.Game.Infrastructure.Network;

public sealed class ServerProtocol(IPacketDispatch<Packet> packetDispatcher) : Protocol
{
    private readonly IPacketDispatch<Packet> _packetDispatcher = packetDispatcher;

    public override bool KeepConnectionOpen => true;

    public override void OnAccept(IConnection connection, CancellationToken cancellationToken = default)
    {
        base.OnAccept(connection, cancellationToken);

        // Thêm kết nối vào danh sách quản lý
        ConnectionManager.Instance.AddConnection(connection);

        NLogix.Host.Instance.Debug($"[OnAccept] Connection accepted from {connection.RemoteEndPoint}");
    }

    public override void ProcessMessage(object sender, IConnectEventArgs args)
    {
        try
        {
            NLogix.Host.Instance.Debug($"[ProcessMessage] Received packet from {args.Connection.RemoteEndPoint}");
            _packetDispatcher.HandlePacket(args.Connection.IncomingPacket, args.Connection);
            NLogix.Host.Instance.Debug($"[ProcessMessage] Successfully processed packet from {args.Connection.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error($"[ProcessMessage] Error processing packet from {args.Connection.RemoteEndPoint}: {ex}");
            args.Connection.Disconnect();
        }
    }

    protected override void OnConnectionError(IConnection connection, Exception exception)
    {
        base.OnConnectionError(connection, exception);
        NLogix.Host.Instance.Error($"[OnConnectionError] Connection error with {connection.RemoteEndPoint}: {exception}");
    }
}