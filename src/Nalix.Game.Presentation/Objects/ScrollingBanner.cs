using Nalix.Game.Presentation.Enums;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Game.Presentation.Objects;

/// <summary>
/// Đại diện cho một banner cuộn liên tục từ phải sang trái trên màn hình.
/// Khi nội dung cuộn hết sang trái, nó sẽ tự động quay lại từ bên phải.
/// </summary>
[IgnoredLoad("RenderObject")]
public class ScrollingBanner : RenderObject
{
    private const System.Single TextOffsetY = 4f;
    private const System.Single BannerHeight = 32f;

    private static readonly Vector2f ScrollDir = new(-1f, 0f);

    private readonly Text _text;
    private readonly System.Single _speed;
    private readonly RectangleShape _background;

    private System.Single _textWidth;

    /// <summary>
    /// Khởi tạo một thể hiện mới của <see cref="ScrollingBanner"/> với thông điệp và tốc độ cuộn được chỉ định.
    /// </summary>
    /// <param name="message">Thông điệp văn bản sẽ hiển thị trong banner.</param>
    /// <param name="speed">Tốc độ cuộn tính bằng pixel mỗi giây.</param>
    public ScrollingBanner(System.String message, System.Single speed = 100f)
    {
        base.SetZIndex(ZIndex.Banner.ToInt());
        base.Reveal();

        _speed = speed;
        Font font = Assets.Font.Load("1");

        _text = new Text(message, font, 18)
        {
            FillColor = new Color(255, 255, 255),
        };

        _background = new RectangleShape
        {
            FillColor = new Color(0, 0, 0, 100),
            Size = new Vector2f(GameEngine.ScreenSize.X, BannerHeight),
            Position = new Vector2f(0, GameEngine.ScreenSize.Y - BannerHeight),
        };

        this.SetMessage(message);
    }

    /// <summary>
    /// Đặt lại vị trí của văn bản để bắt đầu cuộn từ bên phải màn hình.
    /// </summary>
    private void ResetPosition()
        => _text.Position = new Vector2f(GameEngine.ScreenSize.X, GameEngine.ScreenSize.Y - BannerHeight + TextOffsetY);

    /// <summary>
    /// Cập nhật thông điệp được hiển thị trong banner và đặt lại vị trí cuộn.
    /// </summary>
    /// <param name="message">Thông điệp mới cần hiển thị.</param>
    public void SetMessage(System.String message)
    {
        _text.DisplayedString = message;
        _textWidth = _text.GetGlobalBounds().Width;
        ResetPosition();
    }

    /// <summary>
    /// Cập nhật vị trí của văn bản mỗi khung hình dựa trên thời gian đã trôi qua.
    /// Khi văn bản cuộn hết bên trái màn hình, nó sẽ quay lại bên phải.
    /// </summary>
    /// <param name="deltaTime">Thời gian đã trôi qua kể từ lần cập nhật trước (tính bằng giây).</param>
    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        _text.Position += ScrollDir * (_speed * deltaTime);

        if (_text.Position.X + _textWidth < 0)
        {
            _text.Position = new Vector2f(GameEngine.ScreenSize.X, _text.Position.Y);
        }
    }

    /// <summary>
    /// Vẽ banner (bao gồm nền và văn bản) lên đối tượng đích.
    /// </summary>
    /// <param name="target">Đối tượng render (ví dụ: cửa sổ chính) nơi banner sẽ được vẽ lên.</param>
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
    /// Không hỗ trợ phương thức này. Vui lòng sử dụng <see cref="Render(RenderTarget)"/> để vẽ banner.
    /// </summary>
    /// <returns>Không có giá trị trả về - luôn ném lỗi.</returns>
    /// <exception cref="System.NotSupportedException">Luôn ném lỗi vì không hỗ trợ.</exception>
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");
}