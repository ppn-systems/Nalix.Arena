using Nalix.Common.Package;
using Nalix.Common.Package.Enums;
using Nalix.Game.Shared.Commands;
using Nalix.Game.Shared.Messages;
using Nalix.Shared.Serialization;

namespace Nalix.Game.Application.Caching;

public static class PacketCache<TPacket>
    where TPacket : IPacket, IPacketFactory<TPacket>
{
    public static readonly System.Memory<System.Byte> HandshakeAlreadyDone;
    public static readonly System.Memory<System.Byte> HandshakeInvalidType;
    public static readonly System.Memory<System.Byte> HandshakeInvalidKeyLength;

    public static readonly System.Memory<System.Byte> DuplicateUsername;
    public static readonly System.Memory<System.Byte> RegisterSuccess;
    public static readonly System.Memory<System.Byte> RegisterInternalError;

    public static readonly System.Memory<System.Byte> LoginUserNotExist;
    public static readonly System.Memory<System.Byte> LoginAccountLocked;
    public static readonly System.Memory<System.Byte> LoginIncorrectPassword;
    public static readonly System.Memory<System.Byte> LoginAccountDisabled;
    public static readonly System.Memory<System.Byte> LoginSuccess;
    public static readonly System.Memory<System.Byte> LoginInternalError;

    public static readonly System.Memory<System.Byte> LogoutInvalidSession;
    public static readonly System.Memory<System.Byte> LogoutUserNotExist;
    public static readonly System.Memory<System.Byte> LogoutUpdateFailed;
    public static readonly System.Memory<System.Byte> LogoutSuccess;

    static PacketCache()
    {
        #region Handshake Responses

        // HandshakeAlreadyDone
        using (TPacket packet = TPacket.Create(
            Command.Handshake.AsUInt16(),
            PacketType.Object,
            PacketFlags.None,
            PacketPriority.Low,
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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
            LiteSerializer.Serialize(new PacketResponse<System.Byte>
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