using Nalix.Common.Package;
using Nalix.Common.Package.Enums;
using Nalix.Game.Shared.Commands;
using Nalix.Game.Shared.Messages;
using Nalix.Shared.Serialization;
using System;

namespace Nalix.Game.Application.Caching;

public static class PacketCache<TPacket>
    where TPacket : IPacket, IPacketFactory<TPacket>
{
    public static readonly Memory<byte> HandshakeAlreadyDone;
    public static readonly Memory<byte> HandshakeInvalidType;
    public static readonly Memory<byte> HandshakeInvalidKeyLength;

    public static readonly Memory<byte> DuplicateUsername;
    public static readonly Memory<byte> RegisterSuccess;
    public static readonly Memory<byte> RegisterInternalError;

    public static readonly Memory<byte> LoginUserNotExist;
    public static readonly Memory<byte> LoginAccountLocked;
    public static readonly Memory<byte> LoginIncorrectPassword;
    public static readonly Memory<byte> LoginAccountDisabled;
    public static readonly Memory<byte> LoginSuccess;
    public static readonly Memory<byte> LoginInternalError;

    public static readonly Memory<byte> LogoutInvalidSession;
    public static readonly Memory<byte> LogoutUserNotExist;
    public static readonly Memory<byte> LogoutUpdateFailed;
    public static readonly Memory<byte> LogoutSuccess;

    static PacketCache()
    {
        #region Handshake Responses

        // HandshakeAlreadyDone
        using (TPacket packet = TPacket.Create(
            Command.Handshake.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InternalError,
                Message = "Handshake already completed.",
                Data = 0x00
            })
        ))
        {
            HandshakeAlreadyDone = packet.Serialize();
        }

        // HandshakeInvalidType
        using (TPacket packet = TPacket.Create(
            Command.Handshake.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InvalidType,
                Message = "Invalid packet type",
                Data = 0x00
            })
        ))
        {
            HandshakeInvalidType = packet.Serialize();
        }

        // HandshakeInvalidKeyLength
        using (TPacket packet = TPacket.Create(
            Command.Handshake.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InvalidLength,
                Message = "Invalid public key length",
                Data = 0x00
            })
        ))
        {
            HandshakeInvalidKeyLength = packet.Serialize();
        }

        #endregion Handshake Responses

        #region Register Responses

        // DuplicateUsername
        using (TPacket packet = TPacket.Create(
            Command.Register.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Duplicate,
                Message = "Username already existed.",
                Data = 0x00
            })
        ))
        {
            DuplicateUsername = packet.Serialize();
        }

        // RegisterSuccess
        using (TPacket packet = TPacket.Create(
            Command.Register.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Success,
                Message = "Account registered successfully.",
                Data = 0x01
            })
        ))
        {
            RegisterSuccess = packet.Serialize();
        }

        // RegisterInternalError
        using (TPacket packet = TPacket.Create(
            Command.Register.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InternalError,
                Message = "Failed to register account due to an internal error.",
                Data = 0x00
            })
        ))
        {
            RegisterInternalError = packet.Serialize();
        }

        #endregion Register Responses

        #region Login Responses

        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.NotFound,
                Message = "Username does not exist.",
                Data = 0x00
            })
        ))
        {
            LoginUserNotExist = packet.Serialize();
        }

        // LoginAccountLocked
        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Forbidden,
                Message = "Account locked due to too many failed attempts.",
                Data = 0x00
            })
        ))
        {
            LoginAccountLocked = packet.Serialize();
        }

        // LoginIncorrectPassword
        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InvalidPassword,
                Message = "Incorrect password.",
                Data = 0x00
            })
        ))
        {
            LoginIncorrectPassword = packet.Serialize();
        }

        // LoginAccountDisabled
        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Forbidden,
                Message = "Account is disabled.",
                Data = 0x00
            })
        ))
        {
            LoginAccountDisabled = packet.Serialize();
        }

        // LoginSuccess
        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Success,
                Message = "Login successful.",
                Data = 0x01
            })
        ))
        {
            LoginSuccess = packet.Serialize();
        }

        // LoginInternalError
        using (var packet = TPacket.Create(
            Command.Login.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InternalError,
                Message = "Failed to login due to an internal error.",
                Data = 0x00
            })
        ))
        {
            LoginInternalError = packet.Serialize();
        }

        #endregion Login Responses

        #region Logout Responses

        // LogoutInvalidSession
        using (TPacket packet = TPacket.Create(
            Command.Logout.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InvalidSession,
                Message = "Invalid session. Please login again.",
                Data = 0x00
            })
        ))
        {
            LogoutInvalidSession = packet.Serialize();
        }

        // LogoutUserNotExist
        using (TPacket packet = TPacket.Create(
            Command.Logout.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.NotFound,
                Message = "Username does not exist.",
                Data = 0x00
            })
        ))
        {
            LogoutUserNotExist = packet.Serialize();
        }

        // LogoutUpdateFailed
        using (TPacket packet = TPacket.Create(
            Command.Logout.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.InternalError,
                Message = "Failed to update account status.",
                Data = 0x00
            })
        ))
        {
            LogoutUpdateFailed = packet.Serialize();
        }

        // LogoutSuccess
        using (TPacket packet = TPacket.Create(
            Command.Logout.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            BitSerializer.Serialize(new PacketResponse<byte>
            {
                Code = ResponseCode.Success,
                Message = "Logout successful.",
                Data = 0x01
            })
        ))
        {
            LogoutSuccess = packet.Serialize();
        }

        #endregion Logout Responses
    }
}