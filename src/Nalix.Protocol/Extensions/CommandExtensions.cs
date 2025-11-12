using Nalix.Protocol.Enums;

namespace Nalix.Protocol.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Chuyển một giá trị enum OpCommand sang ushort.
    /// </summary>
    /// <param name="command">Giá trị enum OpCommand.</param>
    /// <returns>Giá trị ushort tương ứng.</returns>
    public static System.UInt16 AsUInt16(this OpCommand command) => (System.UInt16)command;

    /// <summary>
    /// Chuyển một giá trị enum OpCommand sang uint.
    /// </summary>
    /// <param name="command">Giá trị enum OpCommand.</param>
    /// <returns>Giá trị ushort tương ứng.</returns>
    public static System.UInt32 AsUInt32(this PacketMagic command) => (System.UInt32)command;
}