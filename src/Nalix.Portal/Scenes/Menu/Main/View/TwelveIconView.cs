// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Portal;
using Nalix.Portal.Services.Dtos;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Portal.Scenes.Menu.Main.View;

// View: Icon 12+ ở góc
[IgnoredLoad("RenderObject")]
internal sealed class TwelveIconView : RenderObject
{
    private readonly Sprite _icon;
    private readonly ThemeDto _theme;

    public TwelveIconView(ThemeDto theme)
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
