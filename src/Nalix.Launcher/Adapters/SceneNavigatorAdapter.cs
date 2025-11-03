using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Adapters;

// Adapter engine: wrap SceneManager & GraphicsEngine
internal sealed class SceneNavigatorAdapter : ISceneNavigator
{
    public void Change(System.String sceneName) => SceneManager.ChangeScene(sceneName);
    public void CloseWindow() => GraphicsEngine.CloseWindow();
}
