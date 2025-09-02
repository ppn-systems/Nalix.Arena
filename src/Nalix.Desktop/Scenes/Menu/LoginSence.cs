using Nalix.Desktop.Objects.Controls;
using Nalix.Logging;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Effects.Visual.UI;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Desktop.Scenes.Menu;

/// <summary>
/// Màn hình đăng nhập: nền parallax + Username/Password + nút LOGIN.
/// Enter/Click để submit, Esc để hủy.
/// </summary>
internal sealed class LoginSence : Scene
{
    public LoginSence() : base(SceneNames.Login) { }

    protected override void LoadObjects()
    {
        AddObject(new MainScene.ParallaxLayer());
        AddObject(new LoginUi());
    }

    #region Login UI

    [IgnoredLoad("RenderObject")]
    private sealed class LoginUi : RenderObject
    {
        #region Config

        private static readonly Vector2f PanelSize = new(520, 300);
        private static readonly Thickness Border = new(32, 32, 32, 32);
        private static readonly IntRect SrcRect = default;

        // Colors
        private static readonly Color BackdropColor = new(25, 25, 25, 110);
        private static readonly Color BgPanelColor = new(20, 20, 20, 235);
        private static readonly Color LabelColor = new(240, 240, 240);
        private static readonly Color TitleColor = Color.White;
        private static readonly Color FieldPanel = new(180, 180, 180);
        private static readonly Color FieldText = new(30, 30, 30);
        private static readonly Color BtnPanel = new(180, 180, 180);
        private static readonly Color BtnPanelHover = new(70, 70, 70);
        private static readonly Color BtnText = new(30, 30, 30);
        private static readonly Color BtnTextHover = new(255, 255, 255);
        private static readonly Color BackPanel = new(160, 160, 160);

        // Layout numbers
        private const Single TitleFont = 26f;
        private const Single LabelFont = 16f;
        private const Single FieldFont = 18f;
        private const Single FieldWidth = 340f;
        private const Single FieldHeight = 40f;
        private const Single TitleOffsetX = 10f;
        private const Single TitleOffsetY = 6f;
        private const Single LabelUserY = 70f;
        private const Single LabelPassY = 130f;
        private const Single FieldLeft = 140f;
        private const Single FieldUserTop = 60f;
        private const Single FieldPassTop = 120f;
        private const Single BtnRowY = 70f; // khoảng cách đáy panel -> hàng nút
        private const Single BtnWidth = 280f;
        private const Single LoginBtnExtraX = 150f; // như code gốc
        private const Single BackBtnOffsetLeft = -30f; // như code gốc (ra ngoài 1 chút)

        #endregion

        #region Fields

        // backdrop + panel nền
        private readonly RectangleShape _backdrop;
        private readonly NineSlicePanel _bgPanel;

        // visuals & controls
        private readonly Text _title, _uLabel, _pLabel;
        private readonly InputField _user;
        private readonly PasswordField _pass;
        private readonly StretchableButton _backBtn;
        private readonly StretchableButton _loginBtn;

        // assets
        private readonly Texture _panelTex;
        private readonly Font _font;

        // layout
        private readonly Vector2f _panelPos;

        #endregion

        #region Ctor

        public LoginUi()
        {
            SetZIndex(2);

            _panelPos = Centered(PanelSize);
            _font = Assets.Font.Load("1");
            _panelTex = Assets.UiTextures.Load("panels/004");

            _backdrop = BuildBackdrop();
            _bgPanel = BuildBackgroundPanel();

            (_title, _uLabel, _pLabel) = BuildTexts();
            (_user, _pass) = BuildFields();
            (_loginBtn, _backBtn) = BuildButtons();

            WireHandlers();
            DoInitialLayout();
        }

        #endregion

        #region Build helpers

        private static RectangleShape BuildBackdrop()
            => new((Vector2f)GameEngine.ScreenSize)
            {
                FillColor = BackdropColor,
                Position = new Vector2f(0, 0)
            };

        private NineSlicePanel BuildBackgroundPanel()
            => new NineSlicePanel(Assets.UiTextures.Load("panels/020"), Border, SrcRect)
                .SetSize(PanelSize * 1.3f)
                .SetPosition(_panelPos * 0.8f)
                .SetColor(BgPanelColor);

        private (Text title, Text u, Text p) BuildTexts()
        {
            var title = new Text("LOGIN", _font, (UInt32)TitleFont) { FillColor = TitleColor };
            var u = new Text("Username", _font, (UInt32)LabelFont) { FillColor = LabelColor };
            var p = new Text("Password", _font, (UInt32)LabelFont) { FillColor = LabelColor };
            return (title, u, p);
        }

        private (InputField user, PasswordField pass) BuildFields()
        {
            var user = new InputField(_panelTex, Border, SrcRect, _font, (UInt32)FieldFont,
                                      new Vector2f(FieldWidth, FieldHeight),
                                      new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldUserTop));
            user.SetPanelColor(FieldPanel);
            user.SetTextColor(FieldText);
            user.Focused = true;

            var pass = new PasswordField(_panelTex, Border, SrcRect, _font, (UInt32)FieldFont,
                                         new Vector2f(FieldWidth, FieldHeight),
                                         new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldPassTop));
            pass.SetPanelColor(FieldPanel);
            pass.SetTextColor(FieldText);

            return (user, pass);
        }

        private static (StretchableButton login, StretchableButton back) BuildButtons()
        {
            var login = new StretchableButton("Sign in", BtnWidth)
                .SetColors(BtnPanel, BtnPanelHover)
                .SetTextColors(BtnText, BtnTextHover);
            login.SetZIndex(2);

            var back = new StretchableButton("Back", BtnWidth)
                .SetColors(BackPanel, BtnPanelHover)
                .SetTextColors(BtnText, BtnTextHover);
            back.SetZIndex(2);

            return (login, back);
        }

        private void WireHandlers()
        {
            _loginBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _loginBtn.RegisterClickHandler(Submit);

            _backBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _backBtn.RegisterClickHandler(GoBack);
        }

        #endregion

        #region Layout

        private void DoInitialLayout()
        {
            // Title & labels
            _title.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + TitleOffsetY);
            _uLabel.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + LabelUserY);
            _pLabel.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + LabelPassY);

            // Fields: đã set khi build

            // Buttons (giữa đáy panel)
            var r = _loginBtn.GetGlobalBounds();
            Single btnBaseX = _panelPos.X + ((PanelSize.X - r.Width) * 0.5f);
            Single btnBaseY = _panelPos.Y + PanelSize.Y - BtnRowY;

            _loginBtn.SetPosition(new Vector2f(btnBaseX + LoginBtnExtraX, btnBaseY));
            _backBtn.SetPosition(new Vector2f(_panelPos.X + BackBtnOffsetLeft, btnBaseY));
        }

        private static Vector2f Centered(Vector2f size)
            => new(
                (GameEngine.ScreenSize.X - size.X) * 0.5f,
                (GameEngine.ScreenSize.Y - size.Y) * 0.5f
            );

        #endregion

        #region Input

        public override void Update(Single dt)
        {
            HandleTabToggle();
            HandleEnter();
            HandleEscape();
            HandleOtherShortcuts();

            _user.Update(dt);
            _pass.Update(dt);
            _backBtn.Update(dt);
            _loginBtn.Update(dt);
        }

        private void HandleTabToggle()
        {
            if (!InputState.IsKeyPressed(Keyboard.Key.Tab))
            {
                return;
            }

            Boolean toPass = _user.Focused;
            _user.Focused = !toPass;
            _pass.Focused = toPass;
        }

        private void HandleEnter()
        {
            if (!InputState.IsKeyPressed(Keyboard.Key.Enter))
            {
                return;
            }

            NLogix.Host.Instance.Info("LOGIN: Enter pressed");
            if (_user.Focused) { _user.Focused = false; _pass.Focused = true; }
            else if (_pass.Focused) { Submit(); }
        }

        private static void HandleEscape()
        {
            if (InputState.IsKeyPressed(Keyboard.Key.Escape))
            {
                SceneManager.ChangeScene(SceneNames.Main);
            }
        }

        private void HandleOtherShortcuts()
        {
            if (InputState.IsKeyPressed(Keyboard.Key.F2))
            {
                _pass.Toggle();
            }
        }

        #endregion

        #region Render

        public override void Render(RenderTarget target)
        {
            // nền
            target.Draw(_backdrop);
            target.Draw(_bgPanel);

            // text + controls
            target.Draw(_title);
            target.Draw(_uLabel);
            target.Draw(_pLabel);
            _user.Render(target);
            _pass.Render(target);
            _backBtn.Render(target);
            _loginBtn.Render(target);
        }

        protected override Drawable GetDrawable() => _title;

        #endregion

        #region Actions

        private static void GoBack() => SceneManager.ChangeScene(SceneNames.Main);

        private void Submit()
        {
            _ = _user.Text;
            _ = _pass.Text;
            SceneManager.ChangeScene(SceneNames.Main);
        }

        #endregion
    }

    #endregion
}
