// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;

namespace Nalix.Launcher.Services.Adapters;

internal sealed class SceneNavigatorAdapter : ISceneNavigator
{
    public void Change(System.String sceneName) => SceneManager.ChangeScene(sceneName);
    public void CloseWindow() => GraphicsEngine.CloseWindow();
}
