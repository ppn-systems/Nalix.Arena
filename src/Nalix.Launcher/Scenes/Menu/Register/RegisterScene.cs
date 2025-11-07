// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using Nalix.Launcher.Scenes.Menu.Register.Controller;
using Nalix.Launcher.Scenes.Menu.Register.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Scenes.Menu.Register;

internal sealed class RegisterScene : Scene
{
    public RegisterScene() : base(SceneNames.Register)
    {
    }

    protected override void LoadObjects()
    {
        RegisterSceneController controller = new(
            new RegisterView(),
            InstanceManager.Instance.GetExistingInstance<IThemeProvider>(),
            InstanceManager.Instance.GetExistingInstance<ISceneNavigator>(),
            InstanceManager.Instance.GetExistingInstance<IParallaxPresetProvider>()
        );

        controller.Compose(this);
    }
}
