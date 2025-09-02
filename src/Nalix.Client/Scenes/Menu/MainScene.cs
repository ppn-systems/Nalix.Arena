using Nalix.Client.Enums;
using Nalix.Client.Objects.Controls;
using Nalix.Client.Objects.Notifications;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
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
/// Cảnh chính hiển thị sau khi người chơi kết nối thành công.
/// Bao gồm nền parallax và nút thiết lập.
/// </summary>
internal class MainScene : Scene
{
    public MainScene() : base(SceneNames.Main)
    {
    }

    /// <summary>
    /// Tải các đối tượng hiển thị trong cảnh chính.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new ParallaxLayer());  // Hiệu ứng nền chuyển động nhiều lớp
        AddObject(new SettingIcon());    // Biểu tượng thiết lập (setting)
        AddObject(new TwelveIcon());    // Biểu tượng thiết lập (12+)
        AddObject(new Menu());           // Menu chính với nút đăng nhập
        AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", 200f)); // Banner cuộn thông báo
    }

    #region Private Class

    /// <summary>
    /// Đối tượng Menu (hiện tại chưa xử lý gì – placeholder).
    /// </summary>
    [IgnoredLoad("RenderObject")]
    public class Menu : RenderObject
    {
        private readonly StretchableButton _login;

        public Menu()
        {
            SetZIndex(2); // Ưu tiên vẽ sau nền

            _login = new StretchableButton("Login", 320f);

            // Đặt vị trí tạm để force update layout 
            _login.SetPosition(new Vector2f(0, 0));
            FloatRect bounds = _login.GetGlobalBounds();

            Vector2u screenSize = GameEngine.ScreenSize;
            System.Single posX = (screenSize.X - bounds.Width) / 2f;
            System.Single posY = (screenSize.Y - bounds.Height) / 2f;

            _login.SetPosition(new Vector2f(posX, posY - 40));

            _login.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Login));
            _login.SetZIndex(ZIndex.Overlay.ToInt());
        }

        public override void Update(System.Single deltaTime)
        {
            if (!Visible)
            {
                return;
            }

            _login.Update(deltaTime);

            // Nếu mất kết nối → trở về cảnh Network để kết nối lại
            //if (!InstanceManager.Instance.GetOrCreateInstance<RemoteStreamClient>().IsConnected)
            //{
            //    SceneManager.ChangeScene(SceneNames.Network);
            //}
        }

        public override void Render(RenderTarget target) => _login.Render(target);

        protected override Drawable GetDrawable() => null;
    }

    /// <summary>
    /// Lớp hiệu ứng nền parallax gồm nhiều lớp ảnh cuộn với tốc độ khác nhau.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    internal class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
            SetZIndex(1);

            _parallax = new ParallaxBackground(GameEngine.ScreenSize);

            // Thêm các lớp nền từ xa đến gần (xa cuộn chậm, gần cuộn nhanh)
            _parallax.AddLayer(Assets.UiTextures.Load("bg/1"), 00f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/8"), 15f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/2"), 25f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/3"), 30f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/4"), 35f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/5"), 40f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/6"), 45f, true);
            _parallax.AddLayer(Assets.UiTextures.Load("bg/7"), 50f, true);
        }

        public override void Update(System.Single deltaTime) => _parallax.Update(deltaTime);

        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use the Render() method instead of GetDrawable().");

        public override void Render(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            _parallax.Draw(target);
        }
    }

    /// <summary>
    /// Biểu tượng thiết lập cho phép chuyển đến Scene Settings.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class SettingIcon : RenderObject
    {
        private readonly Sound _sound;
        private readonly Sprite _icon;

        public SettingIcon()
        {
            SetZIndex(2); // Luôn hiển thị phía trên các lớp nền

            // Tải texture biểu tượng thiết lập
            Texture texture = Assets.UiTextures.Load("icons/1");

            _icon = new Sprite(texture)
            {
                Scale = new Vector2f(0.6f, 0.6f),
                //Color = new Color(255, 255, 255), // Tông vàng nhẹ
            };

            FloatRect bounds = _icon.GetGlobalBounds();
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");

            // Canh phải trên màn hình
            _icon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width, 5);

            // Âm thanh khi nhấn
            _sound = new Sound(buffer);
        }

        public override void Update(System.Single deltaTime)
        {
            if (!Visible)
            {
                return;
            }

            // Click chuột trái vào biểu tượng thiết lập
            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_icon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _sound.Play();
                    _sound.Dispose();
                    SceneManager.ChangeScene(SceneNames.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _icon;
    }

    [IgnoredLoad("RenderObject")]
    private class TwelveIcon : RenderObject
    {
        private readonly Sprite _icon;

        public TwelveIcon()
        {
            SetZIndex(2); // Luôn hiển thị phía trên các lớp nền

            // Tải texture biểu tượng 12+
            Texture texture = Assets.UiTextures.Load("icons/12");

            _icon = new Sprite(texture)
            {
                Scale = new Vector2f(0.6f, 0.6f),
                // Canh phải trên màn hình
                Position = new Vector2f(0, 0)
            };
        }

        protected override Drawable GetDrawable() => _icon;
    }

    #endregion Private Class
}