using Nalix.Graphics;
using Nalix.Graphics.Assets.Manager;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Rendering.Parallax;
using Nalix.Graphics.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;

namespace Nalix.Client.Desktop.Scenes;

internal class MainMenuScene : Scene
{
    public MainMenuScene() : base(NameScene.MainMenu)
        => MusicManager.Play("assets/sounds/0.wav");

    protected override void LoadObjects()
    {
        MusicManager.Resume();

        // Add the parallax object to the scene
        this.AddObject(new ParallaxLayer());
        // Add the icon
        this.AddObject(new SettingIcon());
        this.AddObject(new MusicIcon());
    }

    #region Private Class

    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
            _parallax = new ParallaxBackground(GameEngine.ScreenSize);

            _parallax.AddLayer(Assets.Bg.Load("7.png"), 0f, true);
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
    private class SettingIcon : RenderObject
    {
        private readonly Sprite _settingsIcon;
        private readonly Sound _clickSound;

        public SettingIcon()
        {
            this.SetZIndex(1);

            // Load the settings icon
            Texture texture = Assets.UI.Load("icons/3.png");

            _settingsIcon = new Sprite(texture)
            {
                Scale = new SFML.System.Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _settingsIcon.GetGlobalBounds();
            _settingsIcon.Position = new SFML.System.Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, -10);

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (Input.IsKeyDown(Keyboard.Key.S))
            {
                MusicManager.Pause();
                SceneManager.ChangeScene(NameScene.Settings);
            }

            if (Input.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_settingsIcon.GetGlobalBounds().Contains(Input.GetMousePosition()))
                {
                    MusicManager.Pause();
                    _clickSound.Play();
                    SceneManager.ChangeScene(NameScene.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _settingsIcon;
    }

    [IgnoredLoad("RenderObject")]
    private class MusicIcon : RenderObject
    {
        private readonly Sprite _icon;
        private readonly Texture _texture1;
        private readonly Texture _texture2;
        private readonly Sound _clickSound;

        private bool _isPlaying = true;

        public MusicIcon()
        {
            this.SetZIndex(1);

            _isPlaying = true;

            // Load the settings icon
            _texture1 = Assets.UI.Load("icons/5.png");
            _texture2 = Assets.UI.Load("icons/6.png");

            _icon = new Sprite(_texture1)
            {
                Scale = new SFML.System.Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _icon.GetGlobalBounds();
            _icon.Position = new SFML.System.Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, 60);

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (Input.IsKeyDown(Keyboard.Key.M))
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

            if (Input.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_icon.GetGlobalBounds().Contains(Input.GetMousePosition()))
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

    #endregion Private Class
}