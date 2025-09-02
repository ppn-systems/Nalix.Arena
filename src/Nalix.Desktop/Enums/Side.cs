namespace Nalix.Desktop.Enums;

/// <summary>
/// Biểu thị một hướng trong bối cảnh trò chơi 2D.
/// Enum này định nghĩa bốn hướng chính (Trên, Dưới, Trái, Phải)
/// được sử dụng để chỉ định vị trí, căn chỉnh hoặc hướng cho các thành phần trò chơi
/// như giao diện người dùng, đối tượng trò chơi hoặc ranh giới.
/// </summary>
public enum Side : System.Int32
{
    /// <summary>
    /// Cạnh trên của một thành phần trò chơi hoặc ranh giới.
    /// </summary>
    Top,

    /// <summary>
    /// Cạnh dưới của một thành phần trò chơi hoặc ranh giới.
    /// </summary>
    Bottom,

    /// <summary>
    /// Cạnh trái của một thành phần trò chơi hoặc ranh giới.
    /// </summary>
    Left,

    /// <summary>
    /// Cạnh phải của một thành phần trò chơi hoặc ranh giới.
    /// </summary>
    Right
}