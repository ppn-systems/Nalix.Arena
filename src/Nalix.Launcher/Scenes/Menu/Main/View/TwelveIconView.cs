// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu.Main.View;

// View: Icon 12+ ở góc
[IgnoredLoad("RenderObject")]
internal sealed class TwelveIconView : RenderObject
{
    private readonly Sprite _icon;
    private readonly IUiTheme _theme;

    public TwelveIconView(IUiTheme theme)
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        SetZIndex(_theme.ParallaxZ + 1);

        Texture tex = Assets.UiTextures.Load("icons/12");
        tex.Smooth = false; // UI crisp
        _icon = new Sprite(tex)
        {
            Scale = new Vector2f(0.6f, 0.6f),
            Position = new Vector2f(0, 0)
        };
    }

    protected override Drawable GetDrawable() => _icon;
}
