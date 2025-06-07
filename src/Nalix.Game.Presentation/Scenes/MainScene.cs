using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Rendering.Parallax;
using Nalix.Graphics.Scenes;
using Nalix.Network.Package;
using Nalix.Shared.Net;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Game.Presentation.Scenes;

internal class MainScene : Scene
{
    public MainScene() : base(SceneNames.Main)
    {
    }

    protected override void LoadObjects()
    {
        // Add the parallax object to the scene
        AddObject(new ParallaxLayer());

        // Add the icon
        AddObject(new SettingIcon());
    }

    #region Private Class

    [IgnoredLoad("RenderObject")]
    public class Menu : RenderObject
    {
        public Menu()
        {
            base.SetZIndex(1);
        }

        public override void Update(float deltaTime)
        {
        }

        public override void Render(RenderTarget target)
        {
        }

        protected override Drawable GetDrawable() => null;
    }

    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
            base.SetZIndex(1);

            _parallax = new ParallaxBackground(GameEngine.ScreenSize);

            _parallax.AddLayer(Assets.UiTextures.Load("bg/1"), 00f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/2"), 25f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/3"), 30f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/4"), 35f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/5"), 40f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/6"), 45f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/7"), 50f, true);
        }

        public override void Update(float deltaTime) => _parallax.Update(deltaTime);

        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            _parallax.Draw(target);
        }
    }

    [IgnoredLoad("RenderObject")]
    private class SettingIcon : RenderObject
    {
        private readonly Sound _clickSound;
        private readonly Sprite _settingsIcon;

        public SettingIcon()
        {
            base.SetZIndex(2);

            // Load the settings icon
            Texture texture = Assets.UiTextures.Load("icons/3.png");

            _settingsIcon = new Sprite(texture)
            {
                Scale = new Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _settingsIcon.GetGlobalBounds();
            _settingsIcon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, -10);

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (!NetClient<Packet>.Instance.IsConnected)
            {
                // If already connected, go to the main scene
                SceneManager.ChangeScene(SceneNames.Network);
            }

            if (InputState.IsKeyDown(Keyboard.Key.S))
            {
                _clickSound.Play();
                SceneManager.ChangeScene(SceneNames.Settings);
            }

            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_settingsIcon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _clickSound.Play();
                    SceneManager.ChangeScene(SceneNames.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _settingsIcon;
    }

    [IgnoredLoad("RenderObject")]
    private class Logo : RenderObject
    {
        private readonly Sprite _logoSprite;

        public Logo()
        {
            base.SetZIndex(3);
            // Load the logo texture
            Texture logoTexture = Assets.UiTextures.Load("logo.png");
            _logoSprite = new Sprite(logoTexture)
            {
                Scale = new Vector2f(2f, 2f),
                Position = new Vector2f(
                    (GameEngine.ScreenSize.X / 2) - logoTexture.Size.X,
                    (GameEngine.ScreenSize.Y / 2) - logoTexture.Size.Y),
            };
        }

        public override void Update(float deltaTime)
        {
            // No specific update logic for the logo
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            target.Draw(_logoSprite);
        }

        protected override Drawable GetDrawable() => _logoSprite;
    }

    #endregion Private Class
}