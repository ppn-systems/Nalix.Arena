using Nalix.Common.Attributes;
using Nalix.Common.Caching;
using Nalix.Common.Packets;
using Nalix.Common.Packets.Abstractions;
using Nalix.Common.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Communication.Enums;
using Nalix.Communication.Extensions;
using Nalix.Shared.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;
using System;

namespace Nalix.Communication.Collections;

/// <summary>
/// Gói phản hồi siêu nhẹ từ server.
/// Chỉ gồm StatusCode (1 byte), không có chuỗi message để tiết kiệm băng thông.
/// </summary>
[SerializePackable(SerializeLayout.Sequential)]
[MagicNumber((UInt32)PacketMagic.RESPONSE)]
public sealed class ResponsePacket : FrameBase, IPoolable, IPacketDeserializer<ResponsePacket>
{
    /// <summary>
    /// Trạng thái phản hồi (Ok, InvalidCredentials, Locked…).
    /// Được serialize thành 1 byte.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DataRegion)]
    public ResponseStatus Status { get; set; }

    /// <summary>
    /// Tổng độ dài gói tin = Header + 1 byte status.
    /// </summary>
    [SerializeIgnore]
    public override UInt16 Length =>
        PacketConstants.HeaderSize + sizeof(Byte);

    /// <summary>
    /// Khởi tạo mặc định.
    /// </summary>
    public ResponsePacket()
    {
        OpCode = OpCommand.NONE.AsUInt16();
        Status = ResponseStatus.INTERNAL_ERROR;
        MagicNumber = PacketMagic.RESPONSE.AsUInt32();
    }

    /// <summary>
    /// Thiết lập nhanh giá trị.
    /// </summary>
    public void Initialize(UInt16 opCode, ResponseStatus status)
    {
        OpCode = opCode;
        Status = status;
    }

    /// <summary>
    /// Reset trạng thái để reuse từ pool.
    /// </summary>
    public override void ResetForPool()
    {
        OpCode = OpCommand.NONE.AsUInt16();
        Status = ResponseStatus.INTERNAL_ERROR;
    }

    /// <summary>
    /// Deserialize từ buffer.
    /// </summary>
    public static ResponsePacket Deserialize(ReadOnlySpan<Byte> buffer)
    {
        ResponsePacket packet = InstanceManager.Instance
                                               .GetOrCreateInstance<ObjectPoolManager>()
                                               .Get<ResponsePacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }
}
