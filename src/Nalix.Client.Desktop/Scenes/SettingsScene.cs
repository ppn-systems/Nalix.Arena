using Nalix.Graphics;
using Nalix.Graphics.Render;
using Nalix.Graphics.Scene;
using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Client.Desktop.Scenes;

public class SettingsScene : Scene
{
    public SettingsScene() : base(NameScene.Settings)
    {
    }

    protected override void LoadObjects()
    {
        // Load the settings object
        this.AddObject(new Panel());
    }

    private class Panel : RenderObject
    {
        private readonly Sprite _background;
        private readonly Sprite _panel;

        public Panel()
        {
            Texture bg = Assets.BgTextures.Load("0.png");

            float scaleX = (float)GameLoop.ScreenSize.X / bg.Size.X;
            float scaleY = (float)GameLoop.ScreenSize.Y / bg.Size.Y;

            _background = new Sprite(bg)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(scaleX, scaleY),
                Color = new Color(255, 255, 255, 180) // Màu trắng với alpha = 180 (mờ nhẹ)
            };

            // Panel setup
            Texture panel = Assets.UITextures.Load("3.png");

            // Calculate the scale based on the screen size and the panel's original size
            float scaleFactor = Math.Min(GameLoop.ScreenSize.X / panel.Size.X, GameLoop.ScreenSize.Y / panel.Size.Y);

            // Apply a reduction factor to make the panel smaller
            scaleFactor *= 0.9f; // Reduces the scale by 10%

            // Scale the panel to fit within the screen while maintaining the aspect ratio
            Vector2f scale = new(scaleFactor, scaleFactor);

            // Center the panel on the screen
            float posX = (GameLoop.ScreenSize.X - panel.Size.X * scale.X) / 2f;
            float posY = (GameLoop.ScreenSize.Y - panel.Size.Y * scale.Y) / 2f;

            _panel = new Sprite(panel)
            {
                Scale = scale,
                Position = new Vector2f(posX, posY)
            };
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;

            // Render the background
            target.Draw(_background);
            target.Draw(_panel);
        }

        protected override Drawable GetDrawable() => null;
    }

    private class ControlPanel : RenderObject
    {
        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }
}