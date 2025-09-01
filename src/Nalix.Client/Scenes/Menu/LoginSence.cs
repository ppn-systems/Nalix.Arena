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
        // Visuals
        private readonly Text _title, _uLabel, _pLabel;
        private readonly InputField _user;
        private readonly PasswordField _pass;
        private readonly StretchableButton _loginBtn;

        // Layout
        private readonly Vector2f _panelSize = new(520, 300);
        private readonly Vector2f _panelPos;

        // 9-slice info cho InputField/PasswordField
        private readonly Texture _panelTex;
        private readonly Thickness _border = new(32, 32, 32, 32);
        private readonly IntRect _srcRect = default;

        public LoginUi()
        {
            SetZIndex(2);

            // Center panel area
            _panelPos = new Vector2f(
                (GameEngine.ScreenSize.X - _panelSize.X) * 0.5f,
                (GameEngine.ScreenSize.Y - _panelSize.Y) * 0.5f);

            // Assets
            _panelTex = Assets.UiTextures.Load("panels/004"); // texture panel 9-slice
            Font font = Assets.Font.Load("1");

            // Title + labels
            _title = new Text("LOGIN", font, 26) { FillColor = new Color(25, 25, 25) };
            _uLabel = new Text("Username", font, 16) { FillColor = new Color(60, 60, 60) };
            _pLabel = new Text("Password", font, 16) { FillColor = new Color(60, 60, 60) };

            _title.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 6);
            _uLabel.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 70);
            _pLabel.Position = new Vector2f(_panelPos.X + 10, _panelPos.Y + 130);

            // Fields (dùng NineSlicePanel của bạn bên trong)
            _user = new InputField(_panelTex, _border, _srcRect, font, 18,
                                  new Vector2f(340, 40),
                                  new Vector2f(_panelPos.X + 140, _panelPos.Y + 60));
            _user.SetPanelColor(new Color(245, 245, 245));
            _user.SetTextColor(new Color(30, 30, 30));
            _user.Focused = true;

            _pass = new PasswordField(_panelTex, _border, _srcRect, font, 18,
                                      new Vector2f(340, 40),
                                      new Vector2f(_panelPos.X + 140, _panelPos.Y + 120));
            _pass.SetPanelColor(new Color(245, 245, 245));
            _pass.SetTextColor(new Color(30, 30, 30));

            // Button (StretchableButton của bạn)
            _loginBtn = new StretchableButton("Sign in", 240f);
            _loginBtn.SetColors(panelNormal: new Color(50, 50, 50), panelHover: new Color(70, 70, 70));
            _loginBtn.SetZIndex(3);

            // đặt vị trí theo trung tâm panel
            FloatRect btnBounds = _loginBtn.GetGlobalBounds();
            System.Single btnX = _panelPos.X + ((_panelSize.X - btnBounds.Width) * 0.5f);
            System.Single btnY = _panelPos.Y + _panelSize.Y - 70f;
            _loginBtn.SetPosition(new Vector2f(btnX, btnY));

            // click -> submit
            _loginBtn.RegisterClickHandler(Submit);
        }

        public override void Update(System.Single dt)
        {
            // Tab: chuyển qua lại
            if (InputState.IsKeyPressed(Keyboard.Key.Tab))
            {
                System.Boolean toPass = _user.Focused;
                _user.Focused = !toPass;
                _pass.Focused = toPass;
            }

            // Enter: nếu đang ở Username -> nhảy xuống Password,
            //        nếu đang ở Password -> Submit
            if (InputState.IsKeyPressed(Keyboard.Key.Enter))
            {
                NLogix.Host.Instance.Info("Login: Enter pressed");
                if (_user.Focused)
                {
                    _user.Focused = false;
                    _pass.Focused = true;
                }
                else if (_pass.Focused)
                {
                    Submit();
                }
            }

            // Esc: hủy / quay lại
            if (InputState.IsKeyPressed(Keyboard.Key.Escape))
            {
                SceneManager.ChangeScene(SceneNames.Main);
            }

            // Toggle show password (F2 demo)
            if (InputState.IsKeyPressed(Keyboard.Key.F2))
            {
                _pass.Toggle();
            }

            // Forward updates
            _user.Update(dt);
            _pass.Update(dt);
            _loginBtn.Update(dt);
        }

        public override void Render(RenderTarget target)
        {
            // viền panel “ảo” (không cần panel lớn — chỉ các control đủ)
            // Vẽ text + controls
            target.Draw(_title);
            target.Draw(_uLabel);
            target.Draw(_pLabel);

            _user.Render(target);
            _pass.Render(target);
            _loginBtn.Render(target);
        }

        protected override Drawable GetDrawable() => _title;

        private void Submit()
        {
            _ = _user.Text;
            _ = _pass.Text;

            // TODO: validate/gọi API đăng nhập
            // … nếu ok:
            SceneManager.ChangeScene(SceneNames.Main);
        }
    }
    #endregion
}
