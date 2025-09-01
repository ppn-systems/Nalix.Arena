using Nalix.Client.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Nalix.Client.Objects.Notifications;

/// <summary>
/// Đại diện cho một banner cuộn liên tục từ phải sang trái trên màn hình.
/// Hỗ trợ nhiều thông điệp liên tục.
/// </summary>
[IgnoredLoad("RenderObject")]
public class RollingBanner : RenderObject
{
    private const System.Single TextOffsetY = 4f;
    private const System.Single BannerHeight = 32f;
    private const System.Single TextGap = 50f; // khoảng cách giữa các thông điệp

    private static readonly Vector2f ScrollDir = new(-1f, 0f);

    private readonly List<Text> _texts = [];
    private readonly System.Single _speed;
    private readonly RectangleShape _background;

    /// <summary>
    /// Khởi tạo một thể hiện mới của <see cref="RollingBanner"/> với danh sách thông điệp.
    /// </summary>
    /// <param name="messages">Danh sách thông điệp sẽ hiển thị trong banner.</param>
    /// <param name="speed">Tốc độ cuộn (pixel/giây).</param>
    public RollingBanner(List<System.String> messages, System.Single speed = 100f)
    {
        SetZIndex(ZIndex.Banner.ToInt());
        Reveal();

        _speed = speed;
        Font font = Assets.Font.Load("1");

        // Khởi tạo các text liên tiếp nhau
        System.Single startX = GameEngine.ScreenSize.X;
        foreach (System.String msg in messages)
        {
            Text text = new(msg, font, 18)
            {
                FillColor = new Color(255, 255, 255),
                Position = new Vector2f(startX, GameEngine.ScreenSize.Y - BannerHeight + TextOffsetY)
            };

            _texts.Add(text);
            startX += text.GetGlobalBounds().Width + TextGap;
        }

        _background = new RectangleShape
        {
            FillColor = new Color(0, 0, 0, 100),
            Size = new Vector2f(GameEngine.ScreenSize.X, BannerHeight),
            Position = new Vector2f(0, GameEngine.ScreenSize.Y - BannerHeight),
        };
    }

    /// <summary>
    /// Cập nhật danh sách thông điệp mới và reset vị trí.
    /// </summary>
    public void SetMessages(List<System.String> messages)
    {
        _texts.Clear();
        Font font = Assets.Font.Load("1");

        System.Single startX = GameEngine.ScreenSize.X;
        foreach (System.String msg in messages)
        {
            Text text = new(msg, font, 18)
            {
                FillColor = new Color(255, 255, 255),
                Position = new Vector2f(startX, GameEngine.ScreenSize.Y - BannerHeight + TextOffsetY)
            };

            _texts.Add(text);
            startX += text.GetGlobalBounds().Width + TextGap;
        }
    }

    public override void Update(System.Single deltaTime)
    {
        if (!Visible || _texts.Count == 0)
        {
            return;
        }

        for (System.Int32 i = 0; i < _texts.Count; i++)
        {
            _texts[i].Position += ScrollDir * (_speed * deltaTime);
        }

        // Nếu text đầu tiên đi hết màn hình thì đưa nó ra cuối
        Text first = _texts[0];
        if (first.Position.X + first.GetGlobalBounds().Width < 0)
        {
            Text last = _texts[^1];
            first.Position = new Vector2f(last.Position.X + last.GetGlobalBounds().Width + TextGap, first.Position.Y);

            // Đưa phần tử đầu xuống cuối danh sách
            _texts.RemoveAt(0);
            _texts.Add(first);
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_background);
        foreach (Text t in _texts)
        {
            target.Draw(t);
        }
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");
}
