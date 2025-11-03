// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using Nalix.Launcher.Scenes.Menu.Main.Controller;
using Nalix.Launcher.Scenes.Menu.Main.Model;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Scenes.Menu.Main;

internal sealed class MainScene : Scene
{
    private readonly MainSceneController _controller;

    // Ctor mặc định: tự khởi tạo đầy đủ adapter/service để plug-in nhanh
    public MainScene()
        : base(SceneNames.Main)
    {
        _controller = new MainSceneController(
            new MainMenuModel(),
            InstanceManager.Instance.GetExistingInstance<ISceneNavigator>(),
            InstanceManager.Instance.GetExistingInstance<ISfxPlayer>(),
            InstanceManager.Instance.GetExistingInstance<IParallaxPresetProvider>(),
            InstanceManager.Instance.GetExistingInstance<IThemeProvider>());
    }

    // Ctor DI: nếu sau này bạn dùng container
    public MainScene(MainSceneController controller)
        : base(SceneNames.Main)
    {
        _controller = controller ?? throw new System.ArgumentNullException(nameof(controller));
    }

    protected override void LoadObjects()
    {
        // Controller chịu trách nhiệm lắp đầy đủ View/Model/Service
        _controller.Compose(this);
    }
}
