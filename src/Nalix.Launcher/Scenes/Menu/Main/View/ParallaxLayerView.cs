// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Objects;
using SFML.Graphics;

namespace Nalix.Launcher.Scenes.Menu.Main.View;

// View: Parallax (bọc model ParallaxBackground)
[IgnoredLoad("RenderObject")]
internal sealed class ParallaxLayerView : RenderObject
{
    private readonly ParallaxBackground _parallax;
    private readonly IUiTheme _theme;

    public ParallaxLayerView(IUiTheme theme, ParallaxBackground parallax)
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        _parallax = parallax ?? throw new System.ArgumentNullException(nameof(parallax));
        SetZIndex(_theme.ParallaxZ);
    }

    public override void Update(System.Single dt) => _parallax.Update(dt);

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        _parallax.Draw(target);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");
}
