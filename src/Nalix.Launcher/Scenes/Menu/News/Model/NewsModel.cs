// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Rendering.Attributes;

namespace Nalix.Launcher.Scenes.Menu.News.Model;

/// <summary>
/// Simple model for News scene state (extendable).
/// </summary>
[IgnoredLoad("RenderObject")]
internal sealed class NewsModel
{
    public System.Boolean IsRevealDone { get; set; }
}
