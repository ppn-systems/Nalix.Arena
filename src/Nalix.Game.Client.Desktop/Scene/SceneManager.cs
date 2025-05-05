namespace Nalix.Game.Client.Desktop.Scene;

internal class SceneManager
{
    public static IScene Current { get; private set; }

    public static void SwitchTo(IScene scene) => Current = scene;
}