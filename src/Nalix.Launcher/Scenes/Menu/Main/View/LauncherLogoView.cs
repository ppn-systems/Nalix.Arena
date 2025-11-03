// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Dtos;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu.Main.View;

[IgnoredLoad("RenderObject")]
internal sealed class LauncherLogoView : RenderObject
{
    private readonly Sprite _logo;
    private readonly ThemeDto _theme;
    private Vector2u _lastSize;

    public LauncherLogoView(ThemeDto theme, System.String texturePath = "0")
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        var tex = Assets.UiTextures.Load(texturePath);
        tex.Smooth = true;
        _logo = new Sprite(tex);
        SetZIndex(_theme.UiTopZ);
        _lastSize = GraphicsEngine.ScreenSize;
        Relayout(_lastSize);
    }

    // Chỉ relayout khi kích thước màn hình thay đổi để tiết kiệm CPU
    public override void Update(System.Single dt)
    {
        var size = GraphicsEngine.ScreenSize;
        if (size != _lastSize)
        {
            _lastSize = size;
            Relayout(size);
        }
    }

    private void Relayout(Vector2u screen)
    {
        System.Single sw = screen.X;

        FloatRect r = _logo.GetLocalBounds();
        System.Single texW = r.Width;

        // Tỷ lệ chiều rộng ~ 30% màn hình (giữ đúng behaviour code cũ)
        System.Single targetW = sw * 0.3f;
        System.Single scale = targetW / texW;
        _logo.Scale = new Vector2f(scale, scale);

        System.Single x = (sw - targetW) * 0.5f;
        const System.Single y = 35f; // offset trên

        _logo.Position = new Vector2f(x, y);
    }

    protected override Drawable GetDrawable() => _logo;
}