using Nalix.Graphics;
using Nalix.Graphics.Assets;

namespace Nalix.Client.Desktop;

internal class Program
{
    public static readonly AssetManager Assets = new();

    private static void Main(string[] args)
    {
        GameLoop.OpenWindow();
    }
}