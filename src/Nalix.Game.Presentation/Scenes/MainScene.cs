using Nalix.Game.Presentation.Objects;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Rendering.Parallax;
using Nalix.Graphics.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Game.Presentation.Scenes;

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
        base.AddObject(new ParallaxLayer());  // Hiệu ứng nền chuyển động nhiều lớp
        base.AddObject(new SettingIcon());    // Biểu tượng thiết lập (setting)
        base.AddObject(new TwelveIcon());    // Biểu tượng thiết lập (12+)
        base.AddObject(new Menu());
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
            base.SetZIndex(1); // Ưu tiên vẽ sau nền

            _login = new StretchableButton("Login", 240f);

            // Đặt vị trí tạm để force update layout (rất quan trọng)
            _login.SetPosition(new Vector2f(0, 0));

            // Gọi lại UpdateButtonSize nếu cần (không cần nếu SetPosition đã làm rồi)
            // _login.ForceUpdateSize();

            FloatRect bounds = _login.GetGlobalBounds(); // Bây giờ mới chính xác!

            Vector2u screenSize = GameEngine.ScreenSize;
            float posX = (screenSize.X - bounds.Width) / 2f;
            float posY = (screenSize.Y - bounds.Height) / 2f;

            _login.SetPosition(new Vector2f(posX, posY - 40)); // Vị trí chính thức

            _login.RegisterClickHandler(() => System.Console.WriteLine("Button clicked!"));

            // Log lại đúng kích thước
            bounds = _login.GetGlobalBounds();
            System.Console.WriteLine(
                $"Bounds [FloatRect] Left({bounds.Left}) Top({bounds.Top}) Width({bounds.Width}) Height({bounds.Height})");
            System.Console.WriteLine($"nameof(Menu) initialized at position: {posX}, {posY}");
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            // Nếu mất kết nối → trở về cảnh Network để kết nối lại
            //if (!NetClient<Packet>.Instance.IsConnected)
            //{
            //    SceneManager.ChangeScene(SceneNames.Network);
            //}
        }

        public override void Render(RenderTarget target)
        {
            _login.Render(target);
        }

        protected override Drawable GetDrawable() => null;
    }

    /// <summary>
    /// Lớp hiệu ứng nền parallax gồm nhiều lớp ảnh cuộn với tốc độ khác nhau.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class ParallaxLayer : RenderObject
    {
        private readonly ParallaxBackground _parallax;

        public ParallaxLayer()
        {
            base.SetZIndex(1);

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

        public override void Update(float deltaTime) => _parallax.Update(deltaTime);

        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use the Render() method instead of GetDrawable().");

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
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
            base.SetZIndex(2); // Luôn hiển thị phía trên các lớp nền

            // Tải texture biểu tượng thiết lập
            Texture texture = Assets.UiTextures.Load("icons/3");

            _icon = new Sprite(texture)
            {
                Scale = new Vector2f(2f, 2f),
                Color = new Color(255, 255, 180), // Tông vàng nhẹ
            };

            FloatRect bounds = _icon.GetGlobalBounds();
            SoundBuffer buffer = Assets.Sounds.Load("1.wav");

            // Canh phải trên màn hình
            _icon.Position = new Vector2f(GameEngine.ScreenSize.X - bounds.Width + 20, -10);

            // Âm thanh khi nhấn
            _sound = new Sound(buffer);
        }

        public override void Update(float deltaTime)
        {
            if (!Visible) return;

            // Phím tắt (key S) để vào phần thiết lập
            if (InputState.IsKeyDown(Keyboard.Key.S))
            {
                _sound.Play();
                SceneManager.ChangeScene(SceneNames.Settings);
            }

            // Click chuột trái vào biểu tượng thiết lập
            if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
            {
                if (_icon.GetGlobalBounds().Contains(InputState.GetMousePosition()))
                {
                    _sound.Play();
                    SceneManager.ChangeScene(SceneNames.Settings);
                }
            }
        }

        protected override Drawable GetDrawable() => _icon;
    }

    private class TwelveIcon : RenderObject
    {
        private readonly Sprite _icon;

        public TwelveIcon()
        {
            base.SetZIndex(2); // Luôn hiển thị phía trên các lớp nền

            // Tải texture biểu tượng 12+
            Texture texture = Assets.UiTextures.Load("icons/12+");

            _icon = new Sprite(texture)
            {
                Scale = new Vector2f(2f, 2f),
                //Color = new Color(255, 255, 180), // Tông vàng nhẹ
            };

            FloatRect bounds = _icon.GetGlobalBounds();

            // Canh phải trên màn hình
            _icon.Position = new Vector2f(GameEngine.ScreenSize.X + 20, -10);

        }

        protected override Drawable GetDrawable() => _icon;
    }
        

    #endregion Private Class
    }