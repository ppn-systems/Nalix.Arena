using Nalix.Game.Presentation;
using Nalix.Game.Presentation.Utils;
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
using System.Collections.Generic;

namespace Nalix.Game.Presentation.Scenes;

internal class MainScene : Scene
{
    public MainScene() : base(SceneNames.Main)
        => MusicManager.Play("assets/sounds/0.wav");

    protected override void LoadObjects()
    {
        //MusicManager.Resume();
        // Add the parallax object to the scene
        AddObject(new ParallaxLayer());
        // Add the icon
        AddObject(new MusicIcon());
        AddObject(new SettingIcon());

        // Add the animation
        // 1, 2, 5, 4, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1
        SpriteAnimation animation1 = new("4.png", [1, 2, 5, 4, 2, 2]);
        SpriteAnimation animation2 = new("4.png", [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0]);
        SpriteAnimation animation3 = new("4.png", [0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0]);

        animation1.SetPosition(new Vector2f(0, 660));
        animation2.SetPosition(new Vector2f(60, 660));
        animation3.SetPosition(new Vector2f(120, 660));

        AddObject(animation1);
        AddObject(animation2);
        AddObject(animation3);
    }

    #region Private Class

    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
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
    private class MusicIcon : RenderObject
    {
        private readonly Sprite _icon;
        private readonly Texture _texture1;
        private readonly Texture _texture2;
        private readonly Sound _clickSound;

        private bool _isPlaying = true;

        public MusicIcon()
        {
            SetZIndex(1);

            _isPlaying = true;

            // Load the settings icon
            _texture1 = Assets.UI.Load("icons/5.png");
            _texture2 = Assets.UI.Load("icons/6.png");

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

    [IgnoredLoad("RenderObject")]
    private class SettingIcon : RenderObject
    {
        private readonly Sprite _settingsIcon;
        private readonly Sound _clickSound;

        public SettingIcon()
        {
            SetZIndex(1);

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
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (Input.IsKeyDown(Keyboard.Key.S))
            {
                MusicManager.Pause();
                _clickSound.Play();
                SceneManager.ChangeScene(SceneNames.Settings);
            }

            if (Input.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_settingsIcon.GetGlobalBounds().Contains(Input.GetMousePosition()))
                {
                    MusicManager.Pause();
                    _clickSound.Play();
                    SceneManager.ChangeScene(SceneNames.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _settingsIcon;
    }

    [IgnoredLoad("RenderObject")]
    public class SpriteAnimation : RenderObject
    {
        private readonly Sprite _sprite;
        private readonly List<IntRect> _frames;
        private readonly float _frameDuration;

        private float _elapsedTime = 0f;
        private int _currentFrame = 0;

        public SpriteAnimation(string path, int[] columns)
        {
            SetZIndex(1);
            Texture texture = Assets.UI.Load(path);

            _frameDuration = 0.1f;
            _frames = FrameUtils.GenerateFrames(32, 32, columns);

            _sprite = new Sprite(texture)
            {
                TextureRect = _frames[0],
                Scale = new Vector2f(2f, 2f)
            };
        }

        public void SetPosition(Vector2f position) => _sprite.Position = position;

        public override void Update(float deltaTime)
        {
            _elapsedTime += deltaTime;

            if (_elapsedTime >= _frameDuration)
            {
                _elapsedTime -= _frameDuration;
                _currentFrame = (_currentFrame + 1) % _frames.Count;
                _sprite.TextureRect = _frames[_currentFrame];
            }
        }

        protected override Drawable GetDrawable() => _sprite;
    }

    #endregion Private Class
}