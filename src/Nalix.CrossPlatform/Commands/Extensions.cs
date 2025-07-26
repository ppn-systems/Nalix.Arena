namespace Nalix.CrossPlatform.Commands;

public static class Extensions
{
    /// <summary>
    /// Chuyển một giá trị enum Command sang ushort.
    /// </summary>
    /// <param name="command">Giá trị enum Command.</param>
    /// <returns>Giá trị ushort tương ứng.</returns>
    public static System.UInt16 AsUInt16(this Command command) => (System.UInt16)command;
}