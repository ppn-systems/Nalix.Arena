using Nalix.Shared.Injection.DI;

namespace Nalix.Game.Client.Desktop.Scene;

internal class SceneManager : SingletonBase<SceneManager>
{
    public IScene Current { get; private set; }

    public void SwitchTo(IScene scene)
    {
        Current?.OnExit(); // Gọi OnExit của scene hiện tại
        Current = scene;   // Cập nhật scene hiện tại
        Current.OnEnter(); // Gọi OnEnter của scene mới
    }
}