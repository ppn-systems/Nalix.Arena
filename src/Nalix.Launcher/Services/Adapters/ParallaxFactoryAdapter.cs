// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Services.Abstractions;
using Nalix.Rendering.Effects.Parallax;
using SFML.System;

namespace Nalix.Launcher.Services.Adapters;

internal sealed class ParallaxFactoryAdapter : IParallaxFactory
{
    public ParallaxBackground Create(Vector2u screenSize, System.Int32 sceneVariant)
    {
        var p = new ParallaxBackground(screenSize);

        switch (sceneVariant)
        {
            case 1:
                // hang động
                p.AddLayer(Assets.UiTextures.Load("bg/cave/1"), 00f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/1"), 15f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/2"), 25f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/3"), 30f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/4"), 35f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/5"), 40f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/6"), 45f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/cave/7"), 50f, true);
                break;

            case 2:
                // city
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/1"), 00f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/2"), 35f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/3"), 40f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/4"), 45f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/5"), 50f, true);
                break;

            case 3:
                // city
                p.AddLayer(Assets.UiTextures.Load("bg/gc/1"), 00f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/7"), 10f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/gc/2"), 35f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/gc/3"), 40f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/gc/4"), 45f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/gc/5"), 50f, true);
                break;

            default:
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/1"), 00f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/2"), 35f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/3"), 40f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/4"), 45f, true);
                p.AddLayer(Assets.UiTextures.Load("bg/wcp/5"), 50f, true);
                break;
        }

        return p;
    }
}