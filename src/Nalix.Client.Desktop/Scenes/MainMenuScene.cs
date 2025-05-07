using Nalix.Graphics;
using Nalix.Graphics.Assets.Manager;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Rendering.Parallax;
using Nalix.Graphics.Scenes;
using Nalix.Graphics.Tools;
using SFML.Audio;
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

        MusicManager.Play("assets/sounds/0.wav");
    }

    #region Private Class

    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxPlayer _parallax;

        public ParallaxLayer()
        {
            _parallax = new ParallaxPlayer(GameEngine.ScreenSize);

            if (new Random().Next(0, 50) % 2 != 0)
            {
                _parallax.AddLayer(Assets.BgTextures.Load("7.png"), 0f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("6.png"), 25f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("5.png"), 30f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("4.png"), 35f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("3.png"), 40f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("2.png"), 45f, true);
                _parallax.AddLayer(Assets.BgTextures.Load("1.png"), 50f, true);
            }
            else
            {
                _parallax.AddLayer(Assets.BgTextures.Load("11.png"), 35f, false);
                _parallax.AddLayer(Assets.BgTextures.Load("10.png"), 40f, false);
                _parallax.AddLayer(Assets.BgTextures.Load("9.png"), 45f, false);
                _parallax.AddLayer(Assets.BgTextures.Load("8.png"), 50f, false);
            }
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
    private class SettingsIcon : RenderObject
    {
        private readonly Sprite _settingsIcon;
        private readonly Sound _clickSound;

        public SettingsIcon()
        {
            this.SetZIndex(1);

            Texture texture = Assets.UITextures.Load("1.png");

            ImageCutter image = new(texture, 16, 16);
            _settingsIcon = image.CutIconAt(3, 5);
            _settingsIcon.Scale = new SFML.System.Vector2f(3.5f, 3.5f);

            FloatRect bounds = _settingsIcon.GetGlobalBounds();
            _settingsIcon.Position = new SFML.System.Vector2f(
                GameEngine.ScreenSize.X - bounds.Width - 20,
                20
            );

            // Load click sound
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _clickSound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (Input.IsKeyDown(Keyboard.Key.S))
            {
                MusicManager.Stop();
                SceneManager.ChangeScene(NameScene.Settings);
            }

            if (Input.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_settingsIcon.GetGlobalBounds().Contains(Input.GetMousePosition()))
                {
                    MusicManager.Stop();
                    _clickSound.Play();
                    SceneManager.ChangeScene(NameScene.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _settingsIcon;
    }

    #endregion Private Class
}