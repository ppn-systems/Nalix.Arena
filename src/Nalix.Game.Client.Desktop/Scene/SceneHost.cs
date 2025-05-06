namespace Nalix.Game.Client.Desktop.Scene;

internal static class SceneHost
{
    public static IScene Current { get; private set; }

    public static void SwitchTo(IScene scene) => Current = scene;
}