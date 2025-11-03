using System.Drawing;                    // cho ThemeDto
using Nalix.Launcher.Services.Abstractions;
using Nalix.Launcher.Services.Dtos;
using Nalix.Launcher.Enums;

namespace Nalix.Launcher.Adapters;

// Cấp ThemeDto từ cấu hình hiện tại
internal sealed class ThemeAdapter : IThemeProvider
{
    public ThemeDto Current { get; } = new ThemeDto
    {
        PanelDark = Color.FromArgb(36, 36, 36),
        PanelHover = Color.FromArgb(58, 58, 58),
        PanelAlt = Color.FromArgb(46, 46, 46),
        PanelAltHover = Color.FromArgb(74, 74, 74),

        TextWhite = Color.FromArgb(255, 255, 255),
        TextSoft = Color.FromArgb(220, 220, 220),
        TextNeon = Color.FromArgb(255, 255, 102),

        ExitNormal = Color.FromArgb(255, 180, 180),
        ExitHover = Color.FromArgb(255, 120, 120),

        OverlayZ = ZIndex.Overlay.ToInt(),
        UiTopZ = 8888,
        EffectTopZ = 9999,
        ParallaxZ = 1
    };
}
