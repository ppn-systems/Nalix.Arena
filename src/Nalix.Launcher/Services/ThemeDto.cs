// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Enums;
using Nalix.Launcher.Services.Abstractions;
using SFML.Graphics;

namespace Nalix.Launcher.Services;

internal sealed class ThemeDto : IUiTheme
{
    public Color PanelDark => new(36, 36, 36);
    public Color PanelHover => new(58, 58, 58);
    public Color PanelAlt => new(46, 46, 46);
    public Color PanelAltHover => new(74, 74, 74);

    public Color TextWhite => Color.White;
    public Color TextSoft => new(220, 220, 220);
    public Color TextNeon => new(255, 255, 102);

    public Color ExitNormal => new(255, 180, 180);
    public Color ExitHover => new(255, 120, 120);

    public System.Int32 OverlayZ => ZIndex.Overlay.ToInt();
    public System.Int32 UiTopZ => 8888;
    public System.Int32 EffectTopZ => 9999;
    public System.Int32 ParallaxZ => 1;
}
