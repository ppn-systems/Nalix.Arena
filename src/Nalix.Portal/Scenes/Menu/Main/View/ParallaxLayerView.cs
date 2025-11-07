// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Portal;
using Nalix.Portal.Services.Dtos;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;

namespace Nalix.Portal.Scenes.Menu.Main.View;

// View: Parallax (bọc model ParallaxBackground)
[IgnoredLoad("RenderObject")]
internal sealed class ParallaxLayerView : RenderObject
{
    private readonly ParallaxPreset _parallax;
    private readonly ParallaxBackground _parallaxbg;
    private readonly ThemeDto _theme;

    public ParallaxLayerView(ThemeDto theme, ParallaxPreset parallax)
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        _parallax = parallax ?? throw new System.ArgumentNullException(nameof(parallax));

        _parallaxbg = new ParallaxBackground(GraphicsEngine.ScreenSize);
        for (System.Int32 i = 0; i < _parallax.Layers.Count; i++)
        {
            _parallaxbg.AddLayer(Assets.UiTextures.Load(_parallax.Layers[i].TexturePath), _parallax.Layers[i].Speed, _parallax.Layers[i].Repeat);
        }
        SetZIndex(_theme.ParallaxZ);
    }

    public override void Update(System.Single dt) => _parallaxbg.Update(dt);

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        _parallaxbg.Draw(target);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");
}
