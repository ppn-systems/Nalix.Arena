using Nalix.Portal;
using Nalix.Portal.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Nalix.Portal.Objects.Notifications;

/// <summary>
/// Đại diện cho một banner cuộn liên tục từ phải sang trái trên màn hình.
/// Hỗ trợ nhiều thông điệp liên tục.
/// </summary>
[IgnoredLoad("RenderObject")]
public class RollingBanner : RenderObject
{
    #region Constants

    /// <summary>Độ dịch Y của text trong banner (px).</summary>
    private const System.Single TextOffsetYPx = 4f;

    /// <summary>Chiều cao banner (px).</summary>
    private const System.Single BannerHeightPx = 32f;

    /// <summary>Khoảng cách giữa các thông điệp (px).</summary>
    private const System.Single TextGapPx = 50f;

    /// <summary>Kích thước font chữ (px).</summary>
    private const System.UInt32 FontSizePx = 18u;

    /// <summary>Màu chữ mặc định.</summary>
    private static readonly Color DefaultTextColor = new(255, 255, 255);

    /// <summary>Màu nền mặc định (đen, alpha 100).</summary>
    private static readonly Color BackgroundColor = new(0, 0, 0, 100);

    /// <summary>Hướng cuộn từ phải sang trái.</summary>
    private static readonly Vector2f ScrollDirection = new(-1f, 0f);

    #endregion

    #region Fields

    private readonly List<Text> _texts = [];
    private readonly System.Single _speedPxPerSec;
    private readonly RectangleShape _background;

    #endregion

    #region Ctors

    /// <summary>
    /// Khởi tạo một thể hiện mới của <see cref="RollingBanner"/> với danh sách thông điệp.
    /// </summary>
    /// <param name="messages">Danh sách thông điệp sẽ hiển thị trong banner.</param>
    /// <param name="speedPxPerSec">Tốc độ cuộn (px/giây).</param>
    public RollingBanner(List<System.String> messages, System.Single speedPxPerSec = 100f)
    {
        SetZIndex(ZIndex.Banner.ToInt());
        Reveal();

        _speedPxPerSec = speedPxPerSec;
        _background = CreateBackground();

        InitializeTexts(messages);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Cập nhật danh sách thông điệp mới và reset vị trí.
    /// </summary>
    public void SetMessages(List<System.String> messages)
    {
        _texts.Clear();
        InitializeTexts(messages);
    }

    #endregion

    #region Internal Logic

    public override void Update(System.Single deltaTime)
    {
        if (!Visible || _texts.Count == 0)
        {
            return;
        }

        ScrollTexts(deltaTime);
        RecycleTextsIfNeeded();
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_background);
        foreach (Text text in _texts)
        {
            target.Draw(text);
        }
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");

    #endregion

    #region Helpers

    /// <summary>
    /// Tạo nền banner với kích thước phù hợp màn hình.
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
    /// Khởi tạo danh sách text từ messages, sắp xếp theo chiều ngang.
    /// </summary>
    private void InitializeTexts(List<System.String> messages)
    {
        Font font = Assets.Font.Load("1");

        System.Single startX = GraphicsEngine.ScreenSize.X;
        foreach (System.String msg in messages)
        {
            Text text = CreateText(msg, font, startX);
            _texts.Add(text);

            startX += text.GetGlobalBounds().Width + TextGapPx;
        }
    }

    /// <summary>
    /// Tạo một đối tượng <see cref="Text"/> với style mặc định.
    /// </summary>
    private static Text CreateText(System.String message, Font font, System.Single startX)
    {
        return new Text(message, font, FontSizePx)
        {
            FillColor = DefaultTextColor,
            Position = new Vector2f(startX, GraphicsEngine.ScreenSize.Y - BannerHeightPx + TextOffsetYPx)
        };
    }

    /// <summary>
    /// Di chuyển toàn bộ text theo tốc độ.
    /// </summary>
    private void ScrollTexts(System.Single deltaTime)
    {
        System.Single displacement = _speedPxPerSec * deltaTime;
        for (System.Int32 i = 0; i < _texts.Count; i++)
        {
            _texts[i].Position += ScrollDirection * displacement;
        }
    }

    /// <summary>
    /// Nếu text đầu tiên đi hết màn hình thì đưa nó ra cuối danh sách.
    /// </summary>
    private void RecycleTextsIfNeeded()
    {
        Text first = _texts[0];
        if (first.Position.X + first.GetGlobalBounds().Width < 0)
        {
            Text last = _texts[^1];
            first.Position = new Vector2f(
                last.Position.X + last.GetGlobalBounds().Width + TextGapPx,
                first.Position.Y);

            _texts.RemoveAt(0);
            _texts.Add(first);
        }
    }

    #endregion
}
