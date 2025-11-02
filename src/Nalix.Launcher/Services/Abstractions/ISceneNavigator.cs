// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Launcher.Services.Abstractions;

internal interface ISceneNavigator
{
    void Change(System.String sceneName);
    void CloseWindow();
}