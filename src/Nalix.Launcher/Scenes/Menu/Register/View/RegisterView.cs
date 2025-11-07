using Nalix.Launcher.Objects.Controls;
using Nalix.Launcher.Scenes.Shared.View;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Effects.Visual.UI;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu.Register.View;

[IgnoredLoad("RenderObject")]
internal sealed class RegisterView : RenderObject, ICredentialsView
{
    // ===== events để Controller bắt =====
    public event System.Action SubmitRequested;
    public event System.Action BackRequested;
    public event System.Action TogglePasswordRequested;
    public event System.Action<System.Boolean> TabToggled; // true: user->pass, false: pass->user

    // ===== UI config =====
    private static readonly Vector2f PanelSize = new(520, 300);
    private static readonly Thickness Border = new(32, 32, 32, 32);
    private static readonly IntRect SrcRect = default;

    private static readonly Color BackdropColor = new(25, 25, 25, 110);
    private static readonly Color BgPanelColor = new(20, 20, 20, 235);
    private static readonly Color LabelColor = new(240, 240, 240);
    private static readonly Color WarnColor = Color.Red;
    private static readonly Color TitleColor = Color.White;
    private static readonly Color FieldPanel = new(180, 180, 180);
    private static readonly Color FieldText = new(30, 30, 30);
    private static readonly Color BtnPanel = new(180, 180, 180);
    private static readonly Color BtnPanelHover = new(70, 70, 70);
    private static readonly Color BtnText = new(30, 30, 30);
    private static readonly Color BtnTextHover = new(255, 255, 255);
    private static readonly Color BackPanel = new(160, 160, 160);

    private const System.Single WarnFont = 15f;
    private const System.Single TitleFont = 26f;
    private const System.Single LabelFont = 16f;
    private const System.Single FieldFont = 18f;
    private const System.Single FieldWidth = 340f;
    private const System.Single FieldHeight = 40f;
    private const System.Single TitleOffsetX = 10f;
    private const System.Single TitleOffsetY = 6f;
    private const System.Single LabelUserY = 70f;
    private const System.Single LabelPassY = 130f;
    private const System.Single FieldLeft = 140f;
    private const System.Single FieldUserTop = 60f;
    private const System.Single FieldPassTop = 120f;
    private const System.Single BtnRowY = 70f;
    private const System.Single BtnWidth = 280f;
    private const System.Single LoginBtnExtraX = 150f;
    private const System.Single BackBtnOffsetLeft = -30f;

    // ===== fields & assets =====
    private readonly RectangleShape _backdrop;
    private readonly NineSlicePanel _bgPanel;
    private readonly Text _title, _uLabel, _pLabel, _warn;
    private readonly InputField _user;
    private readonly PasswordField _pass;
    private readonly StretchableButton _backBtn;
    private readonly StretchableButton _loginBtn;
    private readonly Texture _panelTex;
    private readonly Font _font;
    private readonly Vector2f _panelPos;

    public RegisterView()
    {
        SetZIndex(2);

        _panelPos = Centered(PanelSize);
        _font = Assets.Font.Load("1");
        _panelTex = Assets.UiTextures.Load("panels/004");

        _backdrop = new RectangleShape((Vector2f)GraphicsEngine.ScreenSize) { FillColor = BackdropColor };

        _bgPanel = new NineSlicePanel(Assets.UiTextures.Load("panels/020"), Border, SrcRect)
            .SetSize(PanelSize * 1.3f)
            .SetPosition(_panelPos * 0.8f)
            .SetColor(BgPanelColor);

        _warn = new Text("", _font, (System.UInt32)WarnFont) { FillColor = WarnColor };
        _title = new Text("REGISTER", _font, (System.UInt32)TitleFont) { FillColor = TitleColor };
        _uLabel = new Text("Username", _font, (System.UInt32)LabelFont) { FillColor = LabelColor };
        _pLabel = new Text("Password", _font, (System.UInt32)LabelFont) { FillColor = LabelColor };

        _user = new InputField(
            _panelTex, Border, SrcRect, _font, (System.UInt32)FieldFont,
            new Vector2f(FieldWidth, FieldHeight),
            new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldUserTop))
        {
            Focused = true
        };

        _user.SetTextColor(FieldText);
        _user.SetPanelColor(FieldPanel);

        _pass = new PasswordField(
            _panelTex, Border, SrcRect, _font, (System.UInt32)FieldFont,
            new Vector2f(FieldWidth, FieldHeight),
            new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldPassTop));

        _pass.SetTextColor(FieldText);
        _pass.SetPanelColor(FieldPanel);

        _loginBtn = new StretchableButton("Register", BtnWidth).SetColors(BtnPanel, BtnPanelHover)
                                                               .SetTextColors(BtnText, BtnTextHover);

        _backBtn = new StretchableButton("Back", BtnWidth).SetColors(BackPanel, BtnPanelHover)
                                                          .SetTextColors(BtnText, BtnTextHover);
        _backBtn.SetZIndex(2);
        _loginBtn.SetZIndex(2);

        WireHandlers();
        Layout();
    }

    private void WireHandlers()
    {
        _loginBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
        _loginBtn.RegisterClickHandler(() => SubmitRequested?.Invoke());

        _backBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
        _backBtn.RegisterClickHandler(() => BackRequested?.Invoke());
    }

    private void Layout()
    {
        _title.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + TitleOffsetY);
        _uLabel.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + LabelUserY);
        _pLabel.Position = new Vector2f(_panelPos.X + TitleOffsetX, _panelPos.Y + LabelPassY);
        _warn.Position = new Vector2f(_panelPos.X + TitleOffsetX - FieldHeight, _panelPos.Y + FieldPassTop + (FieldHeight * 2));

        var r = _loginBtn.GetGlobalBounds();
        System.Single btnBaseX = _panelPos.X + ((PanelSize.X - r.Width) * 0.5f);
        System.Single btnBaseY = _panelPos.Y + PanelSize.Y - BtnRowY;

        _loginBtn.SetPosition(new Vector2f(btnBaseX + LoginBtnExtraX, btnBaseY));
        _backBtn.SetPosition(new Vector2f(_panelPos.X + BackBtnOffsetLeft, btnBaseY));
    }

    private static Vector2f Centered(Vector2f size)
        => new((GraphicsEngine.ScreenSize.X - size.X) * 0.5f,
               (GraphicsEngine.ScreenSize.Y - size.Y) * 0.5f);

    // ===== API controller dùng =====
    public System.String Username => _user.Text?.Trim() ?? System.String.Empty;
    public System.String Password => _pass.Text ?? System.String.Empty;

    public void FocusUser() { _user.Focused = true; _pass.Focused = false; }
    public void FocusPass() { _pass.Focused = true; _user.Focused = false; }
    public System.Boolean IsUserFocused => _user.Focused;
    public System.Boolean IsPassFocused => _pass.Focused;

    public void LockUi(System.Boolean on)
    {
        _user.Enabled = !on;
        _pass.Enabled = !on;
        _backBtn.Enabled = !on;
        _loginBtn.Enabled = !on;
        _loginBtn.SetText(on ? "Register..." : "Register");
    }

    public override void Update(System.Single dt)
    {
        _user.Update(dt);
        _pass.Update(dt);
        _backBtn.Update(dt);
        _loginBtn.Update(dt);
    }

    public override void Render(RenderTarget target)
    {
        target.Draw(_backdrop);
        target.Draw(_bgPanel);
        target.Draw(_warn);
        target.Draw(_title);
        target.Draw(_uLabel);
        target.Draw(_pLabel);

        _user.Render(target);
        _pass.Render(target);
        _backBtn.Render(target);
        _loginBtn.Render(target);
    }

    protected override Drawable GetDrawable() => _title;

    // các hook keyboard sẽ do Controller gọi vào:
    public void OnEnter()
    {
        if (IsUserFocused)
        {
            FocusPass();
        }
        else if (IsPassFocused)
        {
            SubmitRequested?.Invoke();
        }
    }

    public void OnEscape() => BackRequested?.Invoke();

    public void OnTab()
    {
        System.Boolean toPass = _user.Focused;
        if (toPass)
        {
            FocusPass();
        }
        else
        {
            FocusUser();
        }

        TabToggled?.Invoke(toPass);
    }

    public void OnTogglePassword() => _pass.Toggle();

    public void ShowWarning(System.String msg) => _warn.DisplayedString = msg;
}
