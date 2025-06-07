using Nalix.Graphics;
using Nalix.Graphics.Assets.Manager;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Game.Presentation.Scenes.Systems;

public class SettingsScene : Scene
{
    public SettingsScene() : base(SceneNames.Settings)
    {
    }

    protected override void LoadObjects()
    {
        // Load the background
        AddObject(new Background());

        // Load the control
        AddObject(new CloseIcon());
    }

    [IgnoredLoad("RenderObject")]
    private class Background : RenderObject
    {
        private readonly Sprite _background;

        public Background()
        {
            SetZIndex(0); // Đặt ZIndex thấp hơn để nền được render trước.

            Texture bg = Assets.Bg.Load("0.png");

            float scaleX = (float)GameEngine.ScreenSize.X / bg.Size.X;
            float scaleY = (float)GameEngine.ScreenSize.Y / bg.Size.Y;

            _background = new Sprite(bg)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(scaleX, scaleY),
                Color = new Color(255, 255, 255, 180) // Màu trắng với alpha = 180 (mờ nhẹ)
            };
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;

            // Render nền
            target.Draw(_background);
        }

        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");
    }

    [IgnoredLoad("RenderObject")]
    private class Banner : RenderObject
    {
        private readonly Sprite _banner;

        public Vector2f PanelPosition => _banner.Position;

        public Vector2f PanelSize => new(
            _banner.Texture.Size.X * _banner.Scale.X,
            _banner.Texture.Size.Y * _banner.Scale.Y
        );

        public Banner()
        {
            SetZIndex(2);

            // Banner setup
            Texture panel = Assets.UI.Load("tiles/7.png");

            // Calculate the scale based on the screen size and the panel's original size
            float scaleFactor = System.Math.Min(GameEngine.ScreenSize.X / panel.Size.X, GameEngine.ScreenSize.Y / panel.Size.Y);

            // Scale the panel to fit within the screen while maintaining the aspect ratio
            Vector2f scale = new(scaleFactor * 2f, scaleFactor * 1.2f);

            // Center the panel on the screen
            float posX = (GameEngine.ScreenSize.X - (panel.Size.X * scale.X)) / 2f;
            float posY = (GameEngine.ScreenSize.Y - (panel.Size.Y * scale.Y)) / 2f;

            _banner = new Sprite(panel)
            {
                Scale = scale,
                Position = new Vector2f(posX, posY)
            };
        }

        protected override Drawable GetDrawable() => _banner;
    }

    [IgnoredLoad("RenderObject")]
    internal class CloseIcon : RenderObject
    {
        private readonly Sound _clickSound;
        private readonly Sprite _closeIcon;

        public CloseIcon()
        {
            SetZIndex(2);

            // Load the settings icon
            Texture texture = Assets.UI.Load("icons/1.png");

            _closeIcon = new Sprite(texture)
            {
                Scale = new Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _closeIcon.GetGlobalBounds();
            _closeIcon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, -10);

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);

            MusicManager.Play("assets/sounds/0.wav");
            MusicManager.Pause();
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (InputState.IsKeyDown(Keyboard.Key.S))
            {
                _clickSound.Play();
                SceneManager.ChangeScene(SceneNames.Main);
            }

            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_closeIcon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _clickSound.Play();
                    SceneManager.ChangeScene(SceneNames.Main);
                }
            }
        }

        protected override Drawable GetDrawable() => _closeIcon;
    }
}