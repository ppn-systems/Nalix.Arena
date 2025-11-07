// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using Nalix.Launcher.Scenes.Menu.News.Controller;
using Nalix.Launcher.Scenes.Menu.News.Model;
using Nalix.Launcher.Scenes.Menu.News.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Scenes.Menu.News;

internal sealed class NewsScene : Scene
{
    public NewsScene() : base(SceneNames.News) { }

    protected override void LoadObjects()
    {
        var controller = new NewsSceneController(
            new NewsView("divider/002"),
            new NewsModel(),
            InstanceManager.Instance.GetExistingInstance<IThemeProvider>(),
            InstanceManager.Instance.GetExistingInstance<ISceneNavigator>(),
            InstanceManager.Instance.GetExistingInstance<IParallaxPresetProvider>()
        );

        controller.Compose(this);
    }
}
