using Nalix.Graphics;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Render;
using Nalix.Graphics.Scene;
using Nalix.Graphics.Tools;
using Nalix.Graphics.UI;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
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
        Panel panel = new();
        this.AddObject(panel);

        // Load the control
        this.AddObject(new ControlPanel(panel.PanelPosition, panel.PanelSize));
    }

    [NotLoadable("RenderObject")]
    private class Panel : RenderObject
    {
        private readonly Sprite _background;
        private readonly Sprite _panel;

        public Vector2f PanelPosition => _panel.Position;

        public Vector2f PanelSize => new(
            _panel.Texture.Size.X * _panel.Scale.X,
            _panel.Texture.Size.Y * _panel.Scale.Y
        );

        public Panel()
        {
            this.SetZIndex(0);

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

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }

    [NotLoadable("RenderObject")]
    private class ControlPanel : RenderObject
    {
        private readonly Button _back;

        private float _backDelay = 0f;
        private bool _clickBack = false;

        // Update the ControlPanel constructor to use the Button.Clicked event instead of the inaccessible OnClick property.
        public ControlPanel(Vector2f panelPosition, Vector2f panelSize)
        {
            this.SetZIndex(1);

            Vector2f buttonSize = new(
                panelSize.X * 0.18f,
                panelSize.Y * 0.11f
            );

            Vector2f buttonPos = new(
                panelPosition.X + panelSize.X * 0.15f,
                panelPosition.Y + panelSize.Y - panelSize.Y * 0.2f
            );

            // Assuming a Font instance is required for the Button constructor
            Font font = Assets.Font.Load("4.ttf"); // Replace with the actual font path or name
            _back = new Button(font, "Back", buttonPos, buttonSize);

            // Subscribe to the Clicked event to handle the button click
            _back.Clicked += () => _clickBack = true;

            Texture texture = Assets.UITextures.Load("2.png");
            ImageCutter cutter = new(texture, 96, 32);

            //  +--------+--------+
            //  | (0, 0) | (1, 0) |   ← row 0
            //  + -------+--------+
            //  | (0, 1) | (1, 1) |   ← row 1
            //  + -------+--------+
            IntRect normal = cutter.GetRectAt(0, 0);
            IntRect pressed = cutter.GetRectAt(1, 0);
            _back.SetTexture(texture, normal, pressed);

            SoundBuffer sound = Assets.Sounds.Load("1.wav");

            _back.SetSounds(new Sound(sound));
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            // Handle the click for the 'Back' button
            var mousePos = Input.GetMousePosition();
            bool isMouseDown = Mouse.IsButtonPressed(Mouse.Button.Left);

            // Chỉ xử lý click một lần duy nhất khi người dùng nhấn chuột trái
            if (isMouseDown)
            {
                _back.HandleClick(Mouse.Button.Left, mousePos);
            }

            // Hover logic tách riêng để tránh gây nhầm là click
            _back.Update(mousePos);

            if (_clickBack)
            {
                SceneManager.ChangeScene(NameScene.MainMenu);
            }
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            target.Draw(_back);
        }

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }
}