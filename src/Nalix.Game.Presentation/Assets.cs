using Nalix.Graphics.Assets;

namespace Nalix.Game.Presentation;

internal static class Assets
{
    public static readonly FontLoader Font = new("assets/fonts");

    public static readonly SfxLoader Sounds = new("assets/sounds");

    public static readonly TextureLoader UiTextures = new("assets/ui");
}