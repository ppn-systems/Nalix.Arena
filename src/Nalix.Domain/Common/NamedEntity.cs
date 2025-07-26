namespace Nalix.Domain.Common;

/// <summary>
/// Lớp trừu tượng cơ bản đại diện cho một thực thể có mã định danh và tên.
/// </summary>
/// <typeparam name="TId">Kiểu dữ liệu của mã định danh.</typeparam>
public abstract class NamedEntity<TId>
{
    /// >>
    public TId Id { get; set; }

    /// <summary>
    /// Tên của thực thể.
    /// </summary>
    public System.String Name { get; set; }
}