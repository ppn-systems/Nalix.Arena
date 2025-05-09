using Nalix.Common.Caching;
using Nalix.Common.Logging;
using Nalix.Network.Listeners;
using Nalix.Network.Protocols;

namespace Nalix.Game.Infrastructure.Network;

public sealed class ServerListener(IProtocol protocol, IBufferPool bufferPool, ILogger logger)
    : Listener(protocol, bufferPool, logger)
{
}