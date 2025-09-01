using SFML.Graphics;

namespace Nalix.Rendering.Effects.Transitions.Abstractions;

/// <summary>
/// Giao diện vẽ overlay cho từng hiệu ứng.
/// </summary>
internal interface ITransitionDrawable
{
    /// <summary>
    /// Cập nhật hình dạng theo tiến trình [0..1] và pha (closing=true: che kín, false: mở ra).
    /// </summary>
    void Update(System.Single progress01, System.Boolean closing);

    /// <summary>
    /// Trả về Drawable để RenderObject vẽ mỗi frame.
    /// </summary>
    Drawable GetDrawable();
}
