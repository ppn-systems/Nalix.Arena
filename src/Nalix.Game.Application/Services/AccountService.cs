using Nalix.Common.Connection;
using Nalix.Common.Package;
using Nalix.Common.Package.Attributes;
using Nalix.Common.Security;
using System.Threading.Tasks;

namespace Nalix.Game.Application.Services;

[PacketController]
internal class AccountService
{
    [PacketId((ushort)Command.Login)]
    [PacketPermission(PermissionLevel.Guest)]
    internal async Task LoginAsync(IPacket packet, IConnection connection)
    {
    }
}