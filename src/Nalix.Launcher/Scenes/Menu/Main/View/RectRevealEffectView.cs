// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Dtos;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu.Main.View;


// View: Hiệu ứng mở màn 4 dải đen (iris-open)
[IgnoredLoad("RenderObject")]
internal sealed class RectRevealEffectView : RenderObject
{
    private readonly RectangleShape _top;
    private readonly RectangleShape _bottom;
    private readonly RectangleShape _left;
    private readonly RectangleShape _right;

    private System.Single _t;
    private readonly System.Single _duration = 2f;
    private readonly Vector2f _startWindowSize;
    private readonly ThemeDto _theme;

    internal RectRevealEffectView(ThemeDto theme, System.Single startWidth = 100f, System.Single startHeight = 60f)
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));

        _startWindowSize = new Vector2f(startWidth, startHeight);

        _top = new RectangleShape();
        _bottom = new RectangleShape();
        _left = new RectangleShape();
        _right = new RectangleShape();

        var black = new Color(0, 0, 0, 255);
        _top.FillColor = black;
        _bottom.FillColor = black;
        _left.FillColor = black;
        _right.FillColor = black;

        SetZIndex(_theme.EffectTopZ);
    }

    protected override Drawable GetDrawable() => _top; // không dùng (chỉ để hợp lệ)

    public override void Update(System.Single deltaTime)
    {
        _t += deltaTime;
        System.Single t = System.Math.Clamp(_t / _duration, 0f, 1f);

        // Easing mượt
        System.Single ease = 1f - System.MathF.Pow(1f - t, 3f);

        var screen = GraphicsEngine.ScreenSize;
        System.Single sw = screen.X;
        System.Single sh = screen.Y;

        System.Single startHX = _startWindowSize.X * 0.5f;
        System.Single startHY = _startWindowSize.Y * 0.5f;

        System.Single targetHX = sw * 0.5f;
        System.Single targetHY = sh * 0.5f;

        System.Single hx = startHX + (targetHX - startHX) * ease;
        System.Single hy = startHY + (targetHY - startHY) * ease;

        System.Single leftBandW = System.MathF.Max(0f, sw * 0.5f - hx);
        System.Single rightBandW = leftBandW;
        System.Single topBandH = System.MathF.Max(0f, sh * 0.5f - hy);
        System.Single bottomBandH = topBandH;

        _top.Position = new Vector2f(0f, 0f);
        _top.Size = new Vector2f(sw, topBandH);

        _bottom.Position = new Vector2f(0f, sh - bottomBandH);
        _bottom.Size = new Vector2f(sw, bottomBandH);

        _left.Position = new Vector2f(0f, sh * 0.5f - hy);
        _left.Size = new Vector2f(leftBandW, 2f * hy);

        _right.Position = new Vector2f(sw - rightBandW, sh * 0.5f - hy);
        _right.Size = new Vector2f(rightBandW, 2f * hy);

        if (t >= 1f)
        {
            Destroy(); // mở hoàn toàn thì gỡ mask
        }
    }

    public override void Render(RenderTarget target)
    {
        target.Draw(_top);
        target.Draw(_bottom);
        target.Draw(_left);
        target.Draw(_right);
    }
}
