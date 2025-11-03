// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Rendering.Attributes;

namespace Nalix.Launcher.Scenes.Menu.Main.Model;

[IgnoredLoad("RenderObject")]
internal sealed class MainMenuModel
{
    // trạng thái disable button khi đang loading...
    public System.Boolean IsBusy { get; set; }
}