using Nalix.Desktop.Enums;
using Nalix.Desktop.Objects.Controls;
using Nalix.Desktop.Objects.Notifications;
using Nalix.Framework.Randomization;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Desktop.Scenes.Menu;

/// <summary>
/// Cảnh chính hiển thị sau khi người chơi kết nối thành công.
/// Bao gồm nền parallax và menu.
/// </summary>
internal class MainScene : Scene
{
    #region Ctor

    public MainScene() : base(SceneNames.Main) { }

    #endregion Ctor

    #region Scene lifecycle

    protected override void LoadObjects()
    {
        AddObject(new ParallaxLayer());                              // nền
        AddObject(new TwelveIcon());                                 // góc 12+
        AddObject(new Menu());                                       // menu
        AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", 200f));
    }

    #endregion Scene lifecycle

    #region Private Types

    /// <summary>Menu chính: LOGIN / Settings / Credits / Exit</summary>
    [IgnoredLoad("RenderObject")]
    private class Menu : RenderObject
    {
        #region Colors

        private static readonly Color PanelDark = new(36, 36, 36);     // #242424
        private static readonly Color PanelHover = new(58, 58, 58);     // #3A3A3A
        private static readonly Color PanelAlt = new(46, 46, 46);     // #2E2E2E
        private static readonly Color PanelAltHv = new(74, 74, 74);     // #4A4A4A

        private static readonly Color TextWhite = Color.White;         // #FFFFFF
        private static readonly Color TextSoft = new(220, 220, 220);  // #DCDCDC
        private static readonly Color TextNeon = new(255, 255, 102);  // #FFFF66

        private static readonly Color ExitNormal = new(255, 180, 180);
        private static readonly Color ExitHover = new(255, 120, 120);

        #endregion Colors

        #region UI Controls

        private readonly StretchableButton _login;
        private readonly StretchableButton _settings;
        private readonly StretchableButton _credits;
        private readonly StretchableButton _exit;
        private readonly StretchableButton[] _buttons;

        #endregion UI Controls

        #region Layout config

        private const System.Single ButtonWidth = 380f;
        private const System.Single VerticalSpacing = 25f;

        #endregion Layout config

        #region Ctor

        public Menu()
        {
            SetZIndex(2);
            _login = NewButton("LOGIN");
            _settings = NewButton("Settings");
            _credits = NewButton("Credits");
            _exit = NewButton("Exit");
            _buttons = [_login, _settings, _credits, _exit];

            ApplyStyles();
            WireHandlers();
            PromoteZIndex();
            LayoutButtons(); // initial layout
        }

        #endregion Ctor

        #region Build helpers

        private static StretchableButton NewButton(System.String text)
            => new(text, ButtonWidth, "panels/005");

        #endregion Build helpers

        #region Styling

        private void ApplyStyles()
        {
            // LOGIN
            _ = _login.SetColors(PanelDark, PanelHover);
            _ = _login.SetTextColors(TextWhite, TextNeon);
            _login.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Settings
            _ = _settings.SetColors(PanelAlt, PanelAltHv);
            _ = _settings.SetTextColors(TextSoft, TextNeon);
            _settings.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Credits
            _ = _credits.SetColors(PanelDark, PanelHover);
            _ = _credits.SetTextColors(TextSoft, TextNeon);
            _credits.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // Exit
            _ = _exit.SetColors(PanelAlt, PanelAltHv);
            _ = _exit.SetTextColors(ExitNormal, ExitHover);
            _exit.SetTextOutline(new Color(0, 0, 0, 180), 2f);
        }

        #endregion Styling

        #region Handlers

        private void WireHandlers()
        {
            _login.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _login.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Login));

            _settings.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _settings.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Settings));

            _credits.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _credits.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Credits));

            _exit.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _exit.RegisterClickHandler(GameEngine.CloseWindow);
        }

        #endregion Handlers

        #region Layout

        private void LayoutButtons()
        {
            Vector2u screen = GameEngine.ScreenSize;

            // tổng chiều cao (bao gồm spacing giữa các nút)
            System.Single total = 0f;
            foreach (var b in _buttons)
            {
                total += b.GetGlobalBounds().Height + VerticalSpacing;
            }

            total -= VerticalSpacing; // bỏ khoảng cách cuối

            System.Single y = (screen.Y - total) / 2f;

            // căn giữa theo X
            foreach (var b in _buttons)
            {
                var r = b.GetGlobalBounds();
                System.Single x = (screen.X - r.Width) / 2f;
                b.SetPosition(new Vector2f(x, y));
                y += r.Height + VerticalSpacing;
            }
        }

        #endregion Layout

        #region Util

        private void PromoteZIndex()
        {
            foreach (var b in _buttons)
            {
                b.SetZIndex(ZIndex.Overlay.ToInt());
            }
        }

        #endregion Util

        #region Render loop

        public override void Update(System.Single dt)
        {
            if (!Visible)
            {
                return;
            }

            foreach (var b in _buttons)
            {
                b.Update(dt);
            }
        }

        public override void Render(RenderTarget target)
        {
            foreach (var b in _buttons)
            {
                b.Render(target);
            }
        }

        protected override Drawable GetDrawable() => null;

        #endregion Render loop
    }

    /// <summary>Nền parallax nhiều lớp</summary>
    [IgnoredLoad("RenderObject")]
    internal class ParallaxLayer : RenderObject
    {
        #region Fields

        private readonly ParallaxBackground _parallax;

        #endregion

        #region Ctor

        public ParallaxLayer()
        {
            SetZIndex(1);
            _parallax = new ParallaxBackground(GameEngine.ScreenSize);

            System.Int32 scene = SecureRandom.GetInt32(1, 4);

            switch (scene)
            {
                case 1:
                    // hang động
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/1"), 00f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/1"), 15f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/2"), 25f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/3"), 30f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/4"), 35f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/5"), 40f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/6"), 45f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/cave/7"), 50f, true);
                    break;
                case 2:
                    // city
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/1"), 00f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/2"), 35f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/3"), 40f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/4"), 45f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/5"), 50f, true);
                    break;
                case 3:
                    // city
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/gc/1"), 00f, true);

                    _parallax.AddLayer(Assets.UiTextures.Load("bg/7"), 10f, true);

                    _parallax.AddLayer(Assets.UiTextures.Load("bg/gc/2"), 35f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/gc/3"), 40f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/gc/4"), 45f, true);

                    _parallax.AddLayer(Assets.UiTextures.Load("bg/gc/5"), 50f, true);
                    break;
                default:
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/1"), 00f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/2"), 35f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/3"), 40f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/4"), 45f, true);
                    _parallax.AddLayer(Assets.UiTextures.Load("bg/wcp/5"), 50f, true);
                    break;
            }

            // xa -> gần
        }

        #endregion

        #region Render loop

        public override void Update(System.Single dt) => _parallax.Update(dt);
        public override void Render(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            _parallax.Draw(target);
        }
        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

        #endregion Render loop
    }

    /// <summary>Biểu tượng 12+ góc màn hình</summary>
    [IgnoredLoad("RenderObject")]
    private class TwelveIcon : RenderObject
    {
        #region Fields

        private readonly Sprite _icon;

        #endregion Fields

        #region Ctor

        public TwelveIcon()
        {
            SetZIndex(2);
            Texture tex = Assets.UiTextures.Load("icons/12");
            _icon = new Sprite(tex)
            {
                Scale = new Vector2f(0.6f, 0.6f),
                Position = new Vector2f(0, 0) // top-left
            };
        }

        #endregion Ctor

        #region Render

        protected override Drawable GetDrawable() => _icon;

        #endregion Render
    }

    #endregion Private Types
}
