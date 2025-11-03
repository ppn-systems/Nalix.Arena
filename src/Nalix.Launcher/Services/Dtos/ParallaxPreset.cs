// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Launcher.Services.Dtos;

public sealed class ParallaxPreset
{
    public System.Int32 Variant { get; init; }
    public System.Collections.Generic.IReadOnlyList<Layer> Layers { get; init; } = [];

    public sealed class Layer
    {
        public System.String TexturePath { get; init; } = "";
        public System.Single Speed { get; init; }
        public System.Boolean Repeat { get; init; }
    }
}
