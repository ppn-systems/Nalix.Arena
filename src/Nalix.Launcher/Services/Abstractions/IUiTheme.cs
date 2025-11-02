// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Launcher.Services.Abstractions;

internal interface IUiTheme
{
    SFML.Graphics.Color PanelDark { get; }
    SFML.Graphics.Color PanelHover { get; }
    SFML.Graphics.Color PanelAlt { get; }
    SFML.Graphics.Color PanelAltHover { get; }

    SFML.Graphics.Color TextWhite { get; }
    SFML.Graphics.Color TextSoft { get; }
    SFML.Graphics.Color TextNeon { get; }

    SFML.Graphics.Color ExitNormal { get; }
    SFML.Graphics.Color ExitHover { get; }

    System.Int32 OverlayZ { get; }
    System.Int32 UiTopZ { get; }
    System.Int32 EffectTopZ { get; }
    System.Int32 ParallaxZ { get; }
}
