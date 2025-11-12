// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using Nalix.Portal.Scenes.Menu.News.Controller;
using Nalix.Portal.Scenes.Menu.News.Model;
using Nalix.Portal.Scenes.Menu.News.View;
using Nalix.Portal.Services.Abstractions;
using Nalix.Rendering.Scenes;

namespace Nalix.Portal.Scenes.Menu.News;

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
