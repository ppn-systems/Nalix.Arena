using Nalix.Graphics.Assets;

namespace Nalix.Client.Desktop;

internal static class Assets
{
    public static readonly FontLoader Font = new("assets/fonts");

    public static readonly TextureLoader UITextures = new("assets/ui");
    public static readonly TextureLoader BgTextures = new("assets/background");

    public static readonly SfxLoader Sounds = new("assets/sounds");
}