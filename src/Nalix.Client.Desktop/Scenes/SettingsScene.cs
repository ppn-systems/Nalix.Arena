using Nalix.Graphics;
using Nalix.Graphics.Render;
using Nalix.Graphics.Scene;
using Nalix.Graphics.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Client.Desktop.Scenes;

public class SettingsScene : Scene
{
    public SettingsScene() : base(NameScene.Settings)
    {
    }

    protected override void LoadObjects()
    {
        // Load the settings object
        this.AddObject(new SettingsObject());
    }

    private class SettingsObject : RenderObject
    {
        private readonly Sprite _background;
        private readonly Sprite _settingsTable;
        private readonly Button _backButton;

        private float _backDelayTimer = 0f;
        private bool _isBackPressed = false;

        public SettingsObject()
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
        }

        public override void Update(float deltaTime)
        {
            if (Input.IsKeyDown(Keyboard.Key.B))
            {
                SceneManager.ChangeScene(NameScene.MainMenu);
            }
        }

        protected override Drawable GetDrawable() => _background;
    }
}