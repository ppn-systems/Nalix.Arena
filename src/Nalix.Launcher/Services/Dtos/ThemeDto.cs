// Copyright (c) 2025 PPN Corporation. All rights reserved.

using System.Drawing;

namespace Nalix.Launcher.Services.Dtos;

public sealed class ThemeDto
{
    // Màu UI
    public Color PanelDark { get; init; }
    public Color PanelHover { get; init; }
    public Color PanelAlt { get; init; }
    public Color PanelAltHover { get; init; }

    public Color TextWhite { get; init; }
    public Color TextSoft { get; init; }
    public Color TextNeon { get; init; }

    public Color ExitNormal { get; init; }
    public Color ExitHover { get; init; }

    public System.Int32 OverlayZ { get; init; }
    public System.Int32 UiTopZ { get; init; }
    public System.Int32 EffectTopZ { get; init; }
    public System.Int32 ParallaxZ { get; init; }
}
