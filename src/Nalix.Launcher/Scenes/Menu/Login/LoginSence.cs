// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using Nalix.Launcher.Scenes.Menu.Login.Controller;
using Nalix.Launcher.Scenes.Menu.Login.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Scenes.Menu.Login;

// Scene chỉ làm nhiệm vụ composition root
internal sealed class LoginScene : Scene
{
    private LoginSceneController _controller;

    public LoginScene() : base(SceneNames.Login)
    {
        // Defer controller and view creation to LoadObjects to avoid creating
        // UI objects during scene instantiation (which can cause premature initialization).
    }

    protected override void LoadObjects()
    {
        _controller = new LoginSceneController(
            new LoginView(),
            InstanceManager.Instance.GetExistingInstance<IThemeProvider>(),
            InstanceManager.Instance.GetExistingInstance<ISceneNavigator>(),
            InstanceManager.Instance.GetExistingInstance<IParallaxPresetProvider>()
        );

        _controller.Compose(this);
    }
}
