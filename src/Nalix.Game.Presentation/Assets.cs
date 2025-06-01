using Nalix.Graphics.Assets;

namespace Nalix.Game.Presentation;

internal static class Assets
{
    public static readonly FontLoader Font = new("assets/fonts");

    public static readonly SfxLoader Sounds = new("assets/sounds");

    public static readonly TextureLoader UI = new("assets/ui");
    public static readonly TextureLoader Bg = new("assets/background");
}