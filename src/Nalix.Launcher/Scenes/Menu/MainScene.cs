// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Randomization;
using Nalix.Launcher.Enums;
using Nalix.Launcher.Objects.Controls;
using Nalix.Launcher.Objects.Notifications;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu;

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
        AddObject(new RectRevealEffect());                           // hiệu ứng mở đầu
        AddObject(new LauncherLogo());                               // logo game
        AddObject(new ParallaxLayer());                              // nền
        AddObject(new TwelveIcon());                                 // góc 12+
        AddObject(new Menu());                                       // menu
        AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", 200f));
    }

    #endregion Scene lifecycle

    #region Private Types

    /// <summary>
    /// Center top game logo object.
    /// Automatically scales to a safe width and preserves aspect ratio.
    /// </summary>
    private sealed class LauncherLogo : RenderObject
    {
        private readonly Sprite _logo;

        public LauncherLogo(System.String texturePath = "0")
        {
            Texture tex = Assets.UiTextures.Load(texturePath);
            tex.Smooth = true;

            _logo = new Sprite(tex);

            SetZIndex(8888); // always on top of UI
            Layout();
        }

        private void Layout()
        {
            var screen = GraphicsEngine.ScreenSize;
            System.Single sw = screen.X;

            FloatRect r = _logo.GetLocalBounds();
            System.Single texW = r.Width;

            // scale width ~ 45% screen width
            System.Single targetW = sw * 0.3f;
            System.Single scale = targetW / texW;
            _logo.Scale = new Vector2f(scale, scale);

            System.Single x = (sw - targetW) * 0.5f;
            const System.Single y = 35f; // top offset

            _logo.Position = new Vector2f(x, y);
        }

        public override void Update(System.Single dt) => Layout();

        protected override Drawable GetDrawable() => _logo;
    }

    /// <summary>
    /// Rectangle "iris-open" reveal effect using four black bands that retract to screen edges,
    /// creating an expanding rectangular window that reveals the scene underneath.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    public sealed class RectRevealEffect : RenderObject
    {
        private readonly RectangleShape _top;
        private readonly RectangleShape _bottom;
        private readonly RectangleShape _left;
        private readonly RectangleShape _right;

        private System.Single _t;                      // elapsed time
        private readonly System.Single _duration = 2f; // seconds
        private readonly Vector2f _startWindowSize; // the small starting window (w,h)

        public RectRevealEffect(System.Single startWidth = 100f, System.Single startHeight = 60f)
        {
            // Starting size of the inner "window" (visible area)
            _startWindowSize = new Vector2f(startWidth, startHeight);

            // Create four black bands
            _top = new RectangleShape();
            _bottom = new RectangleShape();
            _left = new RectangleShape();
            _right = new RectangleShape();

            var black = new Color(0, 0, 0, 255);
            _top.FillColor = black;
            _bottom.FillColor = black;
            _left.FillColor = black;
            _right.FillColor = black;

            SetZIndex(9999); // on top of everything
        }

        protected override Drawable GetDrawable() => _top; // not used (we override Render)

        public override void Update(System.Single deltaTime)
        {
            _t += deltaTime;
            System.Single t = System.Math.Clamp(_t / _duration, 0f, 1f);

            // Smooth ease-out
            System.Single ease = 1f - System.MathF.Pow(1f - t, 3f);

            // Compute current inner window half-size (hx, hy):
            var screen = GraphicsEngine.ScreenSize; // SFML Vector2u
            System.Single sw = screen.X;
            System.Single sh = screen.Y;

            System.Single startHX = _startWindowSize.X * 0.5f;
            System.Single startHY = _startWindowSize.Y * 0.5f;

            System.Single targetHX = sw * 0.5f;
            System.Single targetHY = sh * 0.5f;

            System.Single hx = startHX + ((targetHX - startHX) * ease);
            System.Single hy = startHY + ((targetHY - startHY) * ease);

            // Clamp to avoid negative bands when very close to full open
            System.Single leftBandW = System.MathF.Max(0f, (sw * 0.5f) - hx);
            System.Single rightBandW = leftBandW;
            System.Single topBandH = System.MathF.Max(0f, (sh * 0.5f) - hy);
            System.Single bottomBandH = topBandH;

            // Top band: covers [0, 0, sw, topBandH]
            _top.Position = new Vector2f(0f, 0f);
            _top.Size = new Vector2f(sw, topBandH);

            // Bottom band: covers [0, sh - bottomBandH, sw, bottomBandH]
            _bottom.Position = new Vector2f(0f, sh - bottomBandH);
            _bottom.Size = new Vector2f(sw, bottomBandH);

            // Left band: covers [0, sh/2 - hy, leftBandW, 2*hy]
            _left.Position = new Vector2f(0f, (sh * 0.5f) - hy);
            _left.Size = new Vector2f(leftBandW, 2f * hy);

            // Right band: covers [sw - rightBandW, sh/2 - hy, rightBandW, 2*hy]
            _right.Position = new Vector2f(sw - rightBandW, (sh * 0.5f) - hy);
            _right.Size = new Vector2f(rightBandW, 2f * hy);

            if (t >= 1f)
            {
                Destroy(); // fully revealed, remove mask
            }
        }

        public override void Render(RenderTarget target)
        {
            // Draw four bands on top of scene
            target.Draw(_top);
            target.Draw(_bottom);
            target.Draw(_left);
            target.Draw(_right);
        }
    }

    /// <summary>Menu chính: LOGIN / Register / News / Exit</summary>
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
        private readonly StretchableButton _register;
        private readonly StretchableButton _news;
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
            _register = NewButton("REGISTER");
            _news = NewButton("NEWS");
            _exit = NewButton("EXIT");
            _buttons = [_login, _register, _news, _exit];

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

            // Register
            _ = _register.SetColors(PanelAlt, PanelAltHv);
            _ = _register.SetTextColors(TextSoft, TextNeon);
            _register.SetTextOutline(new Color(0, 0, 0, 160), 2f);

            // News
            _ = _news.SetColors(PanelDark, PanelHover);
            _ = _news.SetTextColors(TextSoft, TextNeon);
            _news.SetTextOutline(new Color(0, 0, 0, 160), 2f);

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

            _register.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _register.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Register));

            _news.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _news.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.News));

            _exit.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _exit.RegisterClickHandler(GraphicsEngine.CloseWindow);
        }

        #endregion Handlers

        #region Layout

        private void LayoutButtons()
        {
            Vector2u screen = GraphicsEngine.ScreenSize;

            // tổng chiều cao (bao gồm spacing giữa các nút)
            System.Single total = 0f;
            foreach (var b in _buttons)
            {
                total += b.GetGlobalBounds().Height + VerticalSpacing;
            }

            total -= VerticalSpacing; // bỏ khoảng cách cuối

            System.Single y = (screen.Y - total) / 1.65f;

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
            _parallax = new ParallaxBackground(GraphicsEngine.ScreenSize);

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
            tex.Smooth = false; // crisp UI
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
