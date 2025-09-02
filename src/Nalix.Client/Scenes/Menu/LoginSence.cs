using Nalix.Client.Objects.Controls;
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

namespace Nalix.Client.Scenes.Menu;

/// <summary>
/// Màn hình đăng nhập: nền parallax + Username/Password + nút Login.
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
        // nền mờ + panel nền
        private readonly RectangleShape _backdrop;
        private readonly NineSlicePanel _bgPanel;

        // Visuals
        private readonly Text _title, _uLabel, _pLabel;
        private readonly InputField _user;
        private readonly PasswordField _pass;
        private readonly StretchableButton _backBtn;
        private readonly StretchableButton _loginBtn;

        // Layout
        private readonly Vector2f _panelSize = new(520, 300);
        private readonly Vector2f _panelPos;

        // 9-slice info
        private readonly Texture _panelTex;
        private readonly IntRect _srcRect = default;
        private readonly Thickness _border = new(32, 32, 32, 32);

        public LoginUi()
        {
            SetZIndex(2);

            // Center panel area
            _panelPos = new Vector2f(
                (GameEngine.ScreenSize.X - _panelSize.X) * 0.5f,
                (GameEngine.ScreenSize.Y - _panelSize.Y) * 0.5f);

            // Assets
            _panelTex = Assets.UiTextures.Load("panels/004");
            Font font = Assets.Font.Load("1");

            // NEW: backdrop tối nhẹ toàn màn
            _backdrop = new RectangleShape((Vector2f)GameEngine.ScreenSize)
            {
                FillColor = new Color(25, 25, 25, 110), // mờ 110/255
                Position = new Vector2f(0, 0)
            };

            // NEW: panel nền phía sau controls
            _bgPanel = new NineSlicePanel(Assets.UiTextures.Load("panels/020"), _border, _srcRect);
            // Nếu bạn dùng bản NineSlicePanel có SetSize/SetPosition:
            _ = _bgPanel
                .SetSize(_panelSize * 1.3f)
                .SetPosition(_panelPos * 0.8f)
                .SetColor(new Color(20, 20, 20, 235));

            // Title + labels
            _title = new Text("LOGIN", font, 26) { FillColor = new Color(255, 255, 255) };
            _uLabel = new Text("Username", font, 16) { FillColor = new Color(240, 240, 240) };
            _pLabel = new Text("Password", font, 16) { FillColor = new Color(240, 240, 240) };

            _title.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 6);
            _uLabel.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 70);
            _pLabel.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 130);

            // Fields
            _user = new InputField(
                _panelTex,
                _border,
                _srcRect,
                font, 18,
                new Vector2f(340, 40),
                new Vector2f(_panelPos.X + 140, _panelPos.Y + 60));

            _user.SetPanelColor(new Color(180, 180, 180));
            _user.SetTextColor(new Color(30, 30, 30));
            _user.Focused = true;

            _pass = new PasswordField(
                _panelTex,
                _border,
                _srcRect,
                font, 18,
                new Vector2f(340, 40),
                new Vector2f(_panelPos.X + 140, _panelPos.Y + 120));
            _pass.SetPanelColor(new Color(180, 180, 180));
            _pass.SetTextColor(new Color(30, 30, 30));

            // Button
            _loginBtn = new StretchableButton("Sign in", 280f);
            _loginBtn.SetColors(panelNormal: new Color(180, 180, 180), panelHover: new Color(70, 70, 70));
            _loginBtn.SetTextColors(textNormal: new Color(30, 30, 30), textHover: new Color(255, 255, 255));
            _loginBtn.SetZIndex(2);

            FloatRect btnBounds = _loginBtn.GetGlobalBounds();
            Single btnX = _panelPos.X + ((_panelSize.X - btnBounds.Width) * 0.5f);
            Single btnY = _panelPos.Y + _panelSize.Y - 70f;

            _loginBtn.SetPosition(new Vector2f(btnX + 150, btnY));
            _loginBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _loginBtn.RegisterClickHandler(Submit);

            _backBtn = new StretchableButton("Back", 280f);
            _backBtn.SetColors(panelNormal: new Color(160, 160, 160), panelHover: new Color(70, 70, 70));
            _backBtn.SetTextColors(textNormal: new Color(30, 30, 30), textHover: new Color(255, 255, 255));
            _backBtn.SetZIndex(2);

            // Place it at bottom-left of the login panel area (aligned with padding)
            Single backX = _panelPos.X - 30f;
            Single backY = _panelPos.Y + _panelSize.Y - 70f; // same Y as login button for a clean row
            _backBtn.SetPosition(new Vector2f(backX, backY));

            _backBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _backBtn.RegisterClickHandler(GoBack);
        }

        public override void Update(Single dt)
        {
            if (InputState.IsKeyPressed(Keyboard.Key.Tab))
            {
                Boolean toPass = _user.Focused;
                _user.Focused = !toPass;
                _pass.Focused = toPass;
            }

            if (InputState.IsKeyPressed(Keyboard.Key.Enter))
            {
                NLogix.Host.Instance.Info("Login: Enter pressed");
                if (_user.Focused) { _user.Focused = false; _pass.Focused = true; }
                else if (_pass.Focused) { Submit(); }
            }

            if (InputState.IsKeyPressed(Keyboard.Key.Escape))
            {
                SceneManager.ChangeScene(SceneNames.Main);
            }

            if (InputState.IsKeyPressed(Keyboard.Key.F2))
            {
                _pass.Toggle();
            }

            _user.Update(dt);
            _pass.Update(dt);

            _backBtn.Update(dt);
            _loginBtn.Update(dt);
        }

        public override void Render(RenderTarget target)
        {
            // Vẽ nền TRƯỚC
            target.Draw(_backdrop);
            target.Draw(_bgPanel);

            // Vẽ text + controls sau cùng
            target.Draw(_title);
            target.Draw(_uLabel);
            target.Draw(_pLabel);

            _user.Render(target);
            _pass.Render(target);

            _backBtn.Render(target);
            _loginBtn.Render(target);
        }

        protected override Drawable GetDrawable() => _title;

        /// <summary>
        /// Navigates back to the main scene.
        /// </summary>
        private void GoBack() => SceneManager.ChangeScene(SceneNames.Main);

        private void Submit()
        {
            _ = _user.Text;
            _ = _pass.Text;
            SceneManager.ChangeScene(SceneNames.Main);
        }
    }


    #endregion
}
