using Nalix.Rendering.Attributes;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Client.Scenes.Menu;

/// <summary>
/// Cảnh thiết lập trò chơi, nơi người dùng có thể thực hiện các tùy chỉnh.
/// </summary>
public class SettingsScene : Scene
{
    /// <summary>
    /// Khởi tạo cảnh thiết lập với tên từ <see cref="SceneNames.Settings"/>.
    /// </summary>
    public SettingsScene() : base(SceneNames.Settings)
    {
    }

    /// <summary>
    /// Tải các đối tượng của cảnh thiết lập như nền và biểu tượng đóng.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new Background());
        AddObject(new CloseIcon());
    }

    /// <summary>
    /// Hiển thị nền mờ cho cảnh thiết lập.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class Background : RenderObject
    {
        private readonly Sprite _background;

        /// <summary>
        /// Khởi tạo đối tượng nền với kích thước và độ mờ thích hợp.
        /// </summary>
        public Background()
        {
            SetZIndex(0); // Hiển thị phía sau

            Texture bg = Assets.UiTextures.Load("bg/0");

            System.Single scaleX = (System.Single)GameEngine.ScreenSize.X / bg.Size.X;
            System.Single scaleY = (System.Single)GameEngine.ScreenSize.Y / bg.Size.Y;

            _background = new Sprite(bg)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(scaleX, scaleY),
                Color = new Color(255, 255, 255, 180) // Mờ nhẹ
            };
        }

        /// <summary>
        /// Vẽ nền lên màn hình.
        /// </summary>
        public override void Render(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            target.Draw(_background);
        }

        /// <summary>
        /// Không hỗ trợ sử dụng GetDrawable() cho lớp này.
        /// </summary>
        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");
    }

    /// <summary>
    /// Banner trung tâm hiển thị trên màn hình thiết lập.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class Banner : RenderObject
    {
        private readonly Sprite _banner;

        /// <summary>
        /// Lấy vị trí hiển thị của panel.
        /// </summary>
        public Vector2f PanelPosition => _banner.Position;

        /// <summary>
        /// Lấy kích thước hiển thị của panel sau khi scale.
        /// </summary>
        public Vector2f PanelSize => new(
            _banner.Texture.Size.X * _banner.Scale.X,
            _banner.Texture.Size.Y * _banner.Scale.Y
        );

        /// <summary>
        /// Khởi tạo banner với tỷ lệ và vị trí phù hợp màn hình.
        /// </summary>
        public Banner()
        {
            SetZIndex(2);

            Texture panel = Assets.UiTextures.Load("tiles/7.png");

            System.Single scaleFactor = System.Math.Min(
                GameEngine.ScreenSize.X / panel.Size.X,
                GameEngine.ScreenSize.Y / panel.Size.Y
            );

            Vector2f scale = new(scaleFactor * 2f, scaleFactor * 1.2f);

            System.Single posX = (GameEngine.ScreenSize.X - panel.Size.X * scale.X) / 2f;
            System.Single posY = (GameEngine.ScreenSize.Y - panel.Size.Y * scale.Y) / 2f;

            _banner = new Sprite(panel)
            {
                Scale = scale,
                Position = new Vector2f(posX, posY)
            };
        }

        /// <summary>
        /// Trả về sprite của banner để vẽ.
        /// </summary>
        protected override Drawable GetDrawable() => _banner;
    }

    /// <summary>
    /// Biểu tượng đóng (nút) ở góc màn hình, cho phép người chơi quay lại màn hình chính.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    internal class CloseIcon : RenderObject
    {
        private readonly Sound _sound;
        private readonly Sprite _icon;

        /// <summary>
        /// Khởi tạo nút đóng với hình ảnh và âm thanh tương ứng.
        /// </summary>
        public CloseIcon()
        {
            SetZIndex(2);

            Texture texture = Assets.UiTextures.Load("icons/1.png");

            _icon = new Sprite(texture)
            {
                Scale = new Vector2f(2f, 2f),
                Color = new Color(255, 255, 180),
            };

            FloatRect bounds = _icon.GetGlobalBounds();
            _icon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, -10);

            // Âm thanh click
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");
            _sound = new Sound(buffer);
        }

        /// <summary>
        /// Xử lý sự kiện phím và chuột để điều hướng trở về màn hình chính.
        /// </summary>
        public override void Update(System.Single deltaTime)
        {
            if (!Visible)
            {
                return;
            }

            if (InputState.IsKeyDown(Keyboard.Key.S))
            {
                _sound.Play();
                SceneManager.ChangeScene(SceneNames.Main);
            }

            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_icon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _sound.Play();
                    SceneManager.ChangeScene(SceneNames.Main);
                }
            }
        }

        /// <summary>
        /// Trả về sprite của nút đóng để vẽ.
        /// </summary>
        protected override Drawable GetDrawable() => _icon;
    }
}