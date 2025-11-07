using Nalix.Portal.Services.Abstractions;
using Nalix.Portal.Services.Dtos;
using System.Collections.Generic;

namespace Nalix.Portal.Adapters;

internal sealed class ParallaxPresetProviderAdapter : IParallaxPresetProvider
{
    public ParallaxPreset GetByVariant(System.Int32 v)
    {
        var layers = new List<ParallaxPreset.Layer>();

        switch (v)
        {
            case 1:
                layers.Add(new() { TexturePath = "bg/cave/1", Speed = 00f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/1"/* */, Speed = 15f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/2", Speed = 25f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/3", Speed = 30f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/4", Speed = 35f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/5", Speed = 40f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/6", Speed = 45f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/cave/7", Speed = 50f, Repeat = true });
                break;
            case 2:
                layers.Add(new() { TexturePath = "bg/wcp/1", Speed = 00f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/2", Speed = 35f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/3", Speed = 40f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/4", Speed = 45f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/5", Speed = 50f, Repeat = true });
                break;
            case 3:
                layers.Add(new() { TexturePath = "bg/gc/1", Speed = 00f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/7", Speed = 10f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/gc/2", Speed = 35f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/gc/3", Speed = 40f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/gc/4", Speed = 45f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/gc/5", Speed = 50f, Repeat = true });
                break;
            default:
                layers.Add(new() { TexturePath = "bg/wcp/1", Speed = 00f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/2", Speed = 35f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/3", Speed = 40f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/4", Speed = 45f, Repeat = true });
                layers.Add(new() { TexturePath = "bg/wcp/5", Speed = 50f, Repeat = true });
                break;
        }

        return new ParallaxPreset
        {
            Variant = v,
            Layers = layers
        };
    }
}
