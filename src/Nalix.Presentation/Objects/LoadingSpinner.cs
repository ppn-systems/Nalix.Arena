using Nalix.Presentation.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Presentation.Objects;

/// <summary>
/// Một đối tượng có thể hiển thị, thể hiện một vòng quay tải (loading spinner) với nền mờ dần và biểu tượng xoay, thay đổi kích thước.
/// Vòng quay được căn giữa màn hình và được sử dụng để biểu thị trạng thái đang tải hoặc xử lý trong trò chơi.
/// </summary>
[IgnoredLoad("RenderObject")]
public sealed class LoadingSpinner : RenderObject
{
    private const System.Single MaxAlpha = 255f;
    private const System.Single BaseScale = 0.6f;
    private const System.Single FadeSpeed = 300f;
    private const System.Single RotationSpeed = 150f;
    private const System.Single ScaleOscillation = 0.02f;

    private System.Single _angle = 0f;
    private System.Single _alpha = 0f;
    private System.Boolean _fadingIn = true;
    private System.Byte _currentAlpha = 0;

    private readonly RectangleShape _bg;
    private readonly Sprite _iconSprite;

    /// <summary>
    /// Khởi tạo một <see cref="LoadingSpinner"/> mới, đặt nó ở vị trí trung tâm màn hình với nền và biểu tượng.
    /// </summary>
    public LoadingSpinner()
    {
        SetZIndex(ZIndex.Overlay.ToInt()); // Đặt độ ưu tiên vẽ cao nhất để luôn hiển thị trên cùng
        Reveal(); // Bắt đầu hiển thị vòng quay

        // Kích thước màn hình
        Vector2f screenSize = new(GameEngine.ScreenSize.X, GameEngine.ScreenSize.Y);

        _bg = new RectangleShape(screenSize)
        {
            // Nền đen ban đầu trong suốt
            FillColor = new Color(0, 0, 0, 0),
            Position = default
        };

        // Tải texture biểu tượng
        Texture iconTexture = Assets.UiTextures.Load("icons/15");

        // Làm mịn texture
        iconTexture.Smooth = true;

        _iconSprite = new Sprite(iconTexture)
        {
            Origin = new Vector2f(iconTexture.Size.X * 0.5f, iconTexture.Size.Y * 0.5f), // Đặt gốc ở trung tâm biểu tượng
            Position = new Vector2f(screenSize.X * 0.5f, screenSize.Y * 0.5f), // Đặt vị trí ở trung tâm màn hình
            Scale = new Vector2f(BaseScale, BaseScale), // Tỷ lệ ban đầu
            Color = new Color(255, 255, 255, 0) // Biểu tượng trắng, ban đầu trong suốt
        };
    }

    /// <summary>
    /// Cập nhật trạng thái của vòng quay, bao gồm hiệu ứng mờ dần, xoay và dao động kích thước.
    /// </summary>
    /// <param name="deltaTime">Thời gian trôi qua kể từ khung hình trước (giây).</param>
    public override void Update(System.Single deltaTime)
    {
        this.UpdateAlpha(deltaTime); // Cập nhật độ trong suốt

        // Cập nhật góc xoay
        _angle += deltaTime * RotationSpeed;
        if (_angle >= 360f)
        {
            _angle -= 360f;
        }

        _iconSprite.Rotation = _angle; // Áp dụng góc xoay cho biểu tượng

        // Dao động kích thước (sử dụng sóng sin)
        System.Single scale = BaseScale + (System.MathF.Sin(_angle * 0.0174533f /* chuyển sang radian */) * ScaleOscillation);
        _iconSprite.Scale = new Vector2f(scale, scale); // Áp dụng tỷ lệ mới
    }

    /// <summary>
    /// Vẽ nền và biểu tượng của vòng quay lên mục tiêu hiển thị.
    /// </summary>
    /// <param name="target">Mục tiêu hiển thị (RenderTarget) để vẽ.</param>
    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_bg);
        target.Draw(_iconSprite);
    }

    /// <summary>
    /// Ném ngoại lệ vì không hỗ trợ lấy đối tượng Drawable trực tiếp. Sử dụng <see cref="Render(RenderTarget)"/> thay thế.
    /// </summary>
    /// <returns>Không trả về giá trị, luôn ném ngoại lệ.</returns>
    /// <exception cref="System.NotSupportedException">Luôn được ném khi gọi phương thức này.</exception>
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Sử dụng Render() thay vì GetDrawable().");

    /// <summary>
    /// Cập nhật độ trong suốt của nền và biểu tượng theo thời gian.
    /// </summary>
    /// <param name="deltaTime">Thời gian trôi qua kể từ khung hình trước (giây).</param>
    private void UpdateAlpha(System.Single deltaTime)
    {
        if (!_fadingIn)
        {
            return; // Không cập nhật nếu đã đạt alpha tối đa
        }

        _alpha += deltaTime * FadeSpeed; // Tăng độ trong suốt
        if (_alpha >= MaxAlpha)
        {
            _alpha = MaxAlpha; // Giới hạn alpha tối đa
            _fadingIn = false; // Dừng hiệu ứng mờ dần
        }

        System.Byte newAlpha = (System.Byte)_alpha; // Chuyển đổi sang byte

        if (_currentAlpha == newAlpha)
        {
            return; // Không cập nhật nếu alpha không thay đổi
        }

        _currentAlpha = newAlpha; // Lưu giá trị alpha mới

        // Cập nhật alpha cho màu nền
        Color bgColor = _bg.FillColor;
        bgColor.A = newAlpha;
        _bg.FillColor = bgColor;

        // Cập nhật alpha cho màu biểu tượng
        Color iconColor = _iconSprite.Color;
        iconColor.A = newAlpha;
        _iconSprite.Color = iconColor;
    }
}