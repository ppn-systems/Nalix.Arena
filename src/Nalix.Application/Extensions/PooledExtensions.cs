using Nalix.Common.Connection;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;

namespace Nalix.Application.Extensions;

internal static class PooledExtensions
{
    private static readonly ObjectPoolManager Pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();

    /// <summary>
    /// Rent ResponsePacket, init, serialize -> return to pool -> send.
    /// Trả pooled object về pool TRƯỚC khi gọi SendAsync để không giữ instance quá lâu.
    /// </summary>
    public static async System.Threading.Tasks.Task SendAsync(
        this IConnection connection,
        System.UInt16 opCode, ResponseStatus status)
    {
        // Rent
        System.Byte[] payload;
        ResponsePacket resp = Pool.Get<ResponsePacket>();

        try
        {
            // Init + Serialize
            resp.Initialize(opCode, status);
            payload = resp.Serialize();
        }
        finally
        {
            Pool.Return(resp);
        }

        _ = await connection.Tcp.SendAsync(payload).ConfigureAwait(false);
    }
}
