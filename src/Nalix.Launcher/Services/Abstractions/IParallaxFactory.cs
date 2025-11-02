// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Rendering.Effects.Parallax;
using SFML.System;

namespace Nalix.Launcher.Services.Abstractions;

internal interface IParallaxFactory
{
    // Tạo model parallax theo biến thể (sceneVariant 1..4)
    ParallaxBackground Create(Vector2u screenSize, System.Int32 sceneVariant);
}
