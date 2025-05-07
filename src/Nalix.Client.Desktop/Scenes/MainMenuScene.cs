using Nalix.Graphics;
using Nalix.Graphics.Parallax;
using Nalix.Graphics.Render;
using Nalix.Graphics.Scene;
using Nalix.Graphics.Tools;
using SFML.Graphics;
using SFML.Window;
using System;

namespace Nalix.Client.Desktop.Scenes;

internal class MainMenuScene : Scene
{
    public MainMenuScene() : base(NameScene.MainMenu) => this.CreateScene();

    protected override void LoadObjects()
    {
        // Add the parallax object to the scene
        this.AddObject(new ParallaxLayer());
        // Add the SettingsIcon which contains the settings icon
        this.AddObject(new SettingsIcon());
    }

    #region Private Class

    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxPlayer _parallax;

        public ParallaxLayer()
        {
            _parallax = new ParallaxPlayer(GameLoop.ScreenSize);

            _parallax.AddLayer(Assets.BgTextures.Load("7.png"), 0f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("6.png"), 25f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("5.png"), 30f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("4.png"), 35f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("3.png"), 40f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("2.png"), 45f, true);
            _parallax.AddLayer(Assets.BgTextures.Load("1.png"), 50f, true);
        }

        public override void Update(float deltaTime) => _parallax.Update(deltaTime);

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            _parallax.Draw(target);
        }
    }

    private class SettingsIcon : RenderObject
    {
        private readonly Sprite _settingsIcon;

        public SettingsIcon()
        {
            Texture texture = Assets.UITextures.Load("1.png");

            ImageCutter image = new(texture, 16, 16);
            _settingsIcon = image.CutIconAt(3, 5);
            _settingsIcon.Scale = new SFML.System.Vector2f(3.5f, 3.5f);

            FloatRect bounds = _settingsIcon.GetGlobalBounds();
            _settingsIcon.Position = new SFML.System.Vector2f(
                GameLoop.ScreenSize.X - bounds.Width - 20,
                20
            );

            this.SetZIndex(1);
        }

        public override void Update(float deltaTime)
        {
            if (Input.IsKeyDown(Keyboard.Key.S))
            {
                SceneManager.ChangeScene(NameScene.Settings);
            }

            if (Input.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_settingsIcon.GetGlobalBounds().Contains(Input.GetMousePosition()))
                {
                    SceneManager.ChangeScene(NameScene.Settings);
                }
            }
        }

        public override void Render(RenderTarget target)
        {
            if (Visible)
            {
                target.Draw(_settingsIcon);

                // DEBUG: Draw border around the icon
                var bounds = _settingsIcon.GetGlobalBounds();
                RectangleShape debugRect = new(new SFML.System.Vector2f(bounds.Width, bounds.Height))
                {
                    Position = _settingsIcon.Position,
                    OutlineColor = Color.Red,
                    OutlineThickness = 2,
                    FillColor = Color.Transparent
                };
                target.Draw(debugRect);
            }
        }

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }

    #endregion Private Class
}