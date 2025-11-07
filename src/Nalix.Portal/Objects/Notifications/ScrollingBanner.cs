using Nalix.Portal;
using Nalix.Portal.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Portal.Objects.Notifications;

/// <summary>
/// Đại diện cho một banner cuộn liên tục từ phải sang trái trên màn hình.
/// Khi nội dung cuộn hết sang trái, nó sẽ tự động quay lại từ bên phải.
/// </summary>
[IgnoredLoad("RenderObject")]
public class ScrollingBanner : RenderObject
{
    #region Constants

    /// <summary>Độ dịch Y của văn bản trong banner (px).</summary>
    private const System.Single TextOffsetYPx = 4f;

    /// <summary>Chiều cao banner (px).</summary>
    private const System.Single BannerHeightPx = 32f;

    /// <summary>Kích thước font chữ (px).</summary>
    private const System.UInt32 FontSizePx = 18u;

    /// <summary>Màu chữ mặc định.</summary>
    private static readonly Color DefaultTextColor = new(255, 255, 255);

    /// <summary>Màu nền mặc định (đen mờ, alpha 100).</summary>
    private static readonly Color BackgroundColor = new(0, 0, 0, 100);

    /// <summary>Hướng cuộn từ phải sang trái.</summary>
    private static readonly Vector2f ScrollDirection = new(-1f, 0f);

    #endregion

    #region Fields

    private readonly Text _text;
    private readonly System.Single _speedPxPerSec;
    private readonly RectangleShape _background;

    private System.Single _textWidthPx;

    #endregion

    #region Ctors

    /// <summary>
    /// Khởi tạo một thể hiện mới của <see cref="ScrollingBanner"/> với thông điệp và tốc độ cuộn.
    /// </summary>
    /// <param name="message">Thông điệp văn bản sẽ hiển thị trong banner.</param>
    /// <param name="speedPxPerSec">Tốc độ cuộn tính bằng pixel/giây.</param>
    public ScrollingBanner(System.String message, System.Single speedPxPerSec = 100f)
    {
        SetZIndex(ZIndex.Banner.ToInt());
        Reveal();

        _speedPxPerSec = speedPxPerSec;

        _text = CreateText(message);
        _background = CreateBackground();

        SetMessage(message);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Cập nhật thông điệp được hiển thị trong banner và đặt lại vị trí cuộn.
    /// </summary>
    /// <param name="message">Thông điệp mới cần hiển thị.</param>
    public void SetMessage(System.String message)
    {
        _text.DisplayedString = message;
        _textWidthPx = _text.GetGlobalBounds().Width;
        ResetPosition();
    }

    #endregion

    #region Internal Logic

    /// <summary>
    /// Cập nhật vị trí của văn bản mỗi khung hình dựa trên thời gian đã trôi qua.
    /// Khi văn bản cuộn hết bên trái màn hình, nó sẽ quay lại bên phải.
    /// </summary>
    /// <param name="deltaTime">Thời gian đã trôi qua kể từ lần cập nhật trước (giây).</param>
    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        MoveText(deltaTime);
        RecycleTextIfNeeded();
    }

    /// <summary>
    /// Vẽ banner (bao gồm nền và văn bản) lên đối tượng đích.
    /// </summary>
    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_background);
        target.Draw(_text);
    }

    /// <summary>
    /// Không hỗ trợ phương thức này. Vui lòng dùng <see cref="Render(RenderTarget)"/>.
    /// </summary>
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");

    #endregion

    #region Helpers

    /// <summary>
    /// Tạo đối tượng nền của banner.
    /// </summary>
    private static RectangleShape CreateBackground()
    {
        return new RectangleShape
        {
            FillColor = BackgroundColor,
            Size = new Vector2f(GraphicsEngine.ScreenSize.X, BannerHeightPx),
            Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - BannerHeightPx),
        };
    }

    /// <summary>
    /// Tạo đối tượng văn bản với style mặc định.
    /// </summary>
    private static Text CreateText(System.String message)
    {
        Font font = Assets.Font.Load("1");
        return new Text(message, font, FontSizePx)
        {
            FillColor = DefaultTextColor,
        };
    }

    /// <summary>
    /// Đặt lại vị trí văn bản để bắt đầu cuộn từ bên phải màn hình.
    /// </summary>
    private void ResetPosition() => _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y - BannerHeightPx + TextOffsetYPx);

    /// <summary>
    /// Di chuyển văn bản theo tốc độ và thời gian.
    /// </summary>
    private void MoveText(System.Single deltaTime) => _text.Position += ScrollDirection * (_speedPxPerSec * deltaTime);

    /// <summary>
    /// Nếu văn bản đã cuộn hết bên trái thì đưa lại ra bên phải.
    /// </summary>
    private void RecycleTextIfNeeded()
    {
        if (_text.Position.X + _textWidthPx < 0)
        {
            _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, _text.Position.Y);
        }
    }

    #endregion
}
