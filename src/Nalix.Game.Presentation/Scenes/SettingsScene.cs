using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Scenes;
using Nalix.Graphics.UI.Elements;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Scenes;

public class SettingsScene : Scene
{
    public SettingsScene() : base(SceneNames.Settings)
    {
    }

    protected override void LoadObjects()
    {
        // Load the background
        AddObject(new Background());

        // Load the settings object
        Banner panel = new();
        AddObject(panel);

        // Load the control
        AddObject(new ButtonBack(panel.PanelPosition, panel.PanelSize));
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
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
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
            float scaleFactor = Math.Min(GameEngine.ScreenSize.X / panel.Size.X, GameEngine.ScreenSize.Y / panel.Size.Y);

            // Scale the panel to fit within the screen while maintaining the aspect ratio
            Vector2f scale = new(scaleFactor * 2f, scaleFactor * 1.2f);

            // Center the panel on the screen
            float posX = (GameEngine.ScreenSize.X - panel.Size.X * scale.X) / 2f;
            float posY = (GameEngine.ScreenSize.Y - panel.Size.Y * scale.Y) / 2f;

            _banner = new Sprite(panel)
            {
                Scale = scale,
                Position = new Vector2f(posX, posY)
            };
        }

        protected override Drawable GetDrawable() => _banner;
    }

    [IgnoredLoad("RenderObject")]
    private class ButtonBack : RenderObject
    {
        private readonly Button _back;

        private bool _clickBack = false;

        // Update the ButtonBack constructor to use the Button.Clicked event instead of the inaccessible OnClick property.
        public ButtonBack(Vector2f panelPosition, Vector2f panelSize)
        {
            SetZIndex(3);

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

            Texture normal = Assets.UI.Load("buttons/3.png");
            Texture pressed = Assets.UI.Load("buttons/4.png");

            _back.SetTexture(normal, pressed);

            SoundBuffer sound = Assets.Sounds.Load("1.wav");

            _back.SetSounds(new Sound(sound));
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            if (Input.IsKeyDown(Keyboard.Key.B))
            {
                SceneManager.ChangeScene(SceneNames.Main);
            }

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
                SceneManager.ChangeScene(SceneNames.Main);
            }
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            target.Draw(_back);
        }

        protected override Drawable GetDrawable() => _back;
    }
}