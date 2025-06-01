using Nalix.Graphics;
using Nalix.Graphics.Assets.Manager;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Rendering.Parallax;
using Nalix.Graphics.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Scenes;

internal class MainScene : Scene
{
    public MainScene() : base(SceneNames.Main)
    { }

    protected override void LoadObjects()
    {
        // Add the parallax object to the scene
        AddObject(new ParallaxLayer());

        // Add the icon
        AddObject(new MusicIcon());
        AddObject(new SettingIcon());
    }

    #region Private Class

    public class Menu : RenderObject
    {
        private readonly Sprite _menuSprite;

        public Menu()
        {
            base.SetZIndex(1);
        }

        protected override Drawable GetDrawable() => _menuSprite;
    }

    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
            base.SetZIndex(1);

            _parallax = new ParallaxBackground(GameEngine.ScreenSize);

            _parallax.AddLayer(Assets.Bg.Load("7.png"), 00f, true);
            _parallax.AddLayer(Assets.Bg.Load("6.png"), 25f, true);
            _parallax.AddLayer(Assets.Bg.Load("5.png"), 30f, true);
            _parallax.AddLayer(Assets.Bg.Load("4.png"), 35f, true);
            _parallax.AddLayer(Assets.Bg.Load("3.png"), 40f, true);
            _parallax.AddLayer(Assets.Bg.Load("2.png"), 45f, true);
            _parallax.AddLayer(Assets.Bg.Load("1.png"), 50f, true);
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

    [IgnoredLoad("RenderObject")]
    internal class MusicIcon : RenderObject
    {
        private readonly Sprite _icon;
        private readonly Texture _texture1;
        private readonly Texture _texture2;
        private readonly Sound _clickSound;

        private bool _isPlaying = true;

        public MusicIcon(bool overturn = false)
        {
            base.SetZIndex(2);

            _isPlaying = true;

            // Load the settings icon
            if (overturn)
            {
                _texture1 = Assets.UI.Load("icons/5.png");
                _texture2 = Assets.UI.Load("icons/6.png");
            }
            else
            {
                _texture1 = Assets.UI.Load("icons/6.png");
                _texture2 = Assets.UI.Load("icons/5.png");
            }

            _icon = new Sprite(_texture1)
            {
                Scale = new Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _icon.GetGlobalBounds();
            _icon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, 60);

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (InputState.IsKeyDown(Keyboard.Key.M))
            {
                _clickSound.Play();

                if (_isPlaying)
                {
                    _isPlaying = false;
                    _icon.Texture = _texture2;

                    MusicManager.Pause();
                }
                else
                {
                    _isPlaying = true;
                    _icon.Texture = _texture1;

                    MusicManager.Resume();
                }
            }

            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_icon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _clickSound.Play();

                    if (_isPlaying)
                    {
                        _isPlaying = false;
                        _icon.Texture = _texture2;

                        MusicManager.Pause();
                    }
                    else
                    {
                        _isPlaying = true;
                        _icon.Texture = _texture1;

                        MusicManager.Resume();
                    }
                }
            }
        }

        protected override Drawable GetDrawable() => _icon;
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
            Texture texture = Assets.UI.Load("icons/3.png");

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

            MusicManager.Play("assets/sounds/0.wav");
            MusicManager.Pause();
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

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

    #endregion Private Class
}