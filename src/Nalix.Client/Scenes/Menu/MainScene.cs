using Nalix.Client.Enums;
using Nalix.Client.Objects.Controls;
using Nalix.Client.Objects.Notifications;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Nalix.Client.Scenes.Menu;

/// <summary>
/// Cảnh chính hiển thị sau khi người chơi kết nối thành công.
/// Bao gồm nền parallax và nút thiết lập.
/// </summary>
internal class MainScene : Scene
{
    private static readonly List<Sound> _activeSounds = [];

    public MainScene() : base(SceneNames.Main)
    {
    }

    /// <summary>
    /// Tải các đối tượng hiển thị trong cảnh chính.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new ParallaxLayer());  // Hiệu ứng nền chuyển động nhiều lớp
        //AddObject(new SettingIcon());    // Biểu tượng thiết lập (setting)
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
        private static readonly Color PanelDark = new(36, 36, 36);  // #242424
        private static readonly Color PanelHover = new(58, 58, 58);  // #3A3A3A
        private static readonly Color PanelAlt = new(46, 46, 46);  // #2E2E2E
        private static readonly Color PanelAltHv = new(74, 74, 74);  // #4A4A4A

        private static readonly Color TextWhite = Color.White;          // #FFFFFF
        private static readonly Color TextSoft = new(220, 220, 220);   // #DCDCDC
        private static readonly Color TextNeon = new(255, 255, 102);   // #FFFF66  (hover)
        private static readonly Color ExitNormal = new(255, 180, 180);   // đỏ nhạt
        private static readonly Color ExitHover = new(255, 120, 120);   // đỏ sáng

        private readonly StretchableButton _login;
        private readonly StretchableButton _settings;
        private readonly StretchableButton _credits;
        private readonly StretchableButton _exit;

        private readonly StretchableButton[] _buttons;

        public Menu()
        {
            SetZIndex(2);

            _login = new StretchableButton("Login", 380f, "panels/005");
            _settings = new StretchableButton("Settings", 380f, "panels/005");
            _credits = new StretchableButton("Credits", 380f, "panels/005");
            _exit = new StretchableButton("Exit", 380f, "panels/005");

            _buttons = [_login, _settings, _credits, _exit];

            // Login
            _login.SetColors(PanelDark, PanelHover);
            _login.SetTextColors(TextWhite, TextNeon);
            _login.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Settings (xen kẽ panel nhạt hơn + chữ xám nhạt)
            _settings.SetColors(PanelAlt, PanelAltHv);
            _settings.SetTextColors(TextSoft, TextNeon);
            _settings.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Credits (quay lại tông đậm cho nhịp thị giác)
            _credits.SetColors(PanelDark, PanelHover);
            _credits.SetTextColors(TextSoft, TextNeon);
            _credits.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Exit (accent đỏ nhạt)
            _exit.SetColors(PanelAlt, PanelAltHv);
            _exit.SetTextColors(ExitNormal, ExitHover);
            _exit.SetTextOutline(new Color(0, 0, 0, 180), 2f);

            // Đăng ký handler
            _login.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _login.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Login));

            _settings.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _settings.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Settings));

            _credits.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _credits.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Credits));

            _exit.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _exit.RegisterClickHandler(GameEngine.CloseWindow);

            foreach (var btn in _buttons)
            {
                btn.SetZIndex(ZIndex.Overlay.ToInt());
            }

            LayoutButtons();
        }

        private void LayoutButtons()
        {
            Vector2u screenSize = GameEngine.ScreenSize;
            System.Single totalHeight = 0f;
            System.Single spacing = 25f;

            // tính tổng chiều cao
            foreach (var btn in _buttons)
            {
                FloatRect bounds = btn.GetGlobalBounds();
                totalHeight += bounds.Height + spacing;
            }
            totalHeight -= spacing; // bỏ spacing cuối

            System.Single startY = (screenSize.Y - totalHeight) / 2f;

            // căn giữa theo X
            foreach (var btn in _buttons)
            {
                FloatRect bounds = btn.GetGlobalBounds();
                System.Single posX = (screenSize.X - bounds.Width) / 2f;
                btn.SetPosition(new Vector2f(posX, startY));
                startY += bounds.Height + spacing;
            }
        }

        public override void Update(System.Single deltaTime)
        {
            if (!Visible)
            {
                return;
            }

            foreach (var btn in _buttons)
            {
                btn.Update(deltaTime);
            }
        }

        public override void Render(RenderTarget target)
        {
            foreach (var btn in _buttons)
            {
                btn.Render(target);
            }
        }

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