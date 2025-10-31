// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Protocols;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Desktop.Objects.Controls;
using Nalix.Desktop.Objects.Notifications;
using Nalix.Framework.Injection;
using Nalix.Framework.Randomization;
using Nalix.Logging;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Effects.Visual.UI;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote;
using Nalix.SDK.Remote.Extensions;
using Nalix.Shared.Messaging.Controls;
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
        private const System.Single BtnRowY = 70f; // khoảng cách đáy panel -> hàng nút
        private const System.Single BtnWidth = 280f;
        private const System.Single LoginBtnExtraX = 150f; // như code gốc
        private const System.Single BackBtnOffsetLeft = -30f; // như code gốc (ra ngoài 1 chút)

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

        private Boolean _loggingIn;
        private DateTime _lastSubmitAt;
        private System.Threading.CancellationTokenSource _loginCts;

        private const Int32 CooldownMs = 600;       // Debounce
        private const Int32 ServerTimeoutMs = 4000; // Match [PacketTimeout(4000)]

        // Simple local rate limiter: allow 2 sends per 3 seconds
        private readonly System.Collections.Generic.Queue<DateTime> _loginSendTimes = new();

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
            var title = new Text("LOGIN", _font, (System.UInt32)TitleFont) { FillColor = TitleColor };
            var u = new Text("Username", _font, (System.UInt32)LabelFont) { FillColor = LabelColor };
            var p = new Text("Password", _font, (System.UInt32)LabelFont) { FillColor = LabelColor };
            return (title, u, p);
        }

        private (InputField user, PasswordField pass) BuildFields()
        {
            var user = new InputField(_panelTex, Border, SrcRect, _font, (System.UInt32)FieldFont,
                                      new Vector2f(FieldWidth, FieldHeight),
                                      new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldUserTop));
            user.SetPanelColor(FieldPanel);
            user.SetTextColor(FieldText);
            user.Focused = true;

            var pass = new PasswordField(_panelTex, Border, SrcRect, _font, (System.UInt32)FieldFont,
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
            _loginBtn.RegisterClickHandler(() => _ = TryLoginAsync());

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
            System.Single btnBaseX = _panelPos.X + ((PanelSize.X - r.Width) * 0.5f);
            System.Single btnBaseY = _panelPos.Y + PanelSize.Y - BtnRowY;

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

        public override void Update(System.Single dt)
        {
            if (!InstanceManager.Instance.GetOrCreateInstance<ReliableClient>().IsConnected)
            {
                SceneManager.ChangeScene(SceneNames.Network);
            }

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

            System.Boolean toPass = _user.Focused;
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
            else if (_pass.Focused) { _ = TryLoginAsync(); }
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

        private async System.Threading.Tasks.Task TryLoginAsync()
        {
            if (_loggingIn)
            {
                return;
            }

            if ((DateTime.UtcNow - _lastSubmitAt).TotalMilliseconds < CooldownMs)
            {
                return;
            }

            _lastSubmitAt = DateTime.UtcNow;

            var client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
            if (!client.IsConnected)
            {
                SceneManager.FindByType<Notification>()?.UpdateMessage("Not connected to server.");
                SceneManager.ChangeScene(SceneNames.Network);
                return;
            }

            // Local rate limit (match [PacketRateLimit(2, 03)])
            if (!AllowRateLimitedSend())
            {
                SceneManager.FindByType<Notification>()?.UpdateMessage("Too many attempts. Please wait a moment.");
                return;
            }

            String user = _user.Text?.Trim() ?? String.Empty;
            String pass = _pass.Text ?? String.Empty;
            if (String.IsNullOrWhiteSpace(user) || String.IsNullOrWhiteSpace(pass))
            {
                SceneManager.FindByType<Notification>()?.UpdateMessage("Please enter username & password");
                return;
            }

            // Encryption required → ensure handshake done (idempotent)
            _loginCts?.Dispose();
            _loginCts = new System.Threading.CancellationTokenSource(ServerTimeoutMs);
            _loggingIn = true;
            LockUi(true);

            try
            {
                // 1) Handshake if needed (idempotent)
                Boolean okHs = await client.HandshakeAsync(opCode: 1, timeoutMs: 3000, ct: _loginCts.Token).ConfigureAwait(false);
                if (!okHs)
                {
                    SceneManager.FindByType<Notification>()?.UpdateMessage("Handshake failed. Please retry.");
                    return;
                }

                // 2) Build CredentialsPacket (+Encrypt). Also ensure it carries a SequenceId.
                var options = client.Options;
                var creds = new Credentials { Username = user, Password = pass };
                var login = new CredentialsPacket();
                login.Initialize((UInt16)OpCommand.LOGIN, creds);
                login.SequenceId = SecureRandom.NextUInt32();
                login = CredentialsPacket.Encrypt(login, options.EncryptionKey, options.EncryptionMode);

                // Assign/obtain a SequenceId:
                // - If the packet implements IPacketSequenced and client assigns on SendAsync,
                //   capture returned seq. Otherwise, set before sending (e.g., login.SequenceId = SeqGenerator.Next()).
                // Below assumes SendAsync returns the effective seq:
                await client.SendAsync(login, _loginCts.Token).ConfigureAwait(false);

                // 3) In parallel, keep processing directives (THROTTLE/REDIRECT/NACK/NOTICE) while we wait.
                using var subs = client.Subscribe(
                    client.On<Directive>(d => client.TryHandleDirectiveAsync(d, null, null, _loginCts.Token))
                );

                // 4) Await correlated CONTROL (ACK or ERROR) with same SequenceId (timeout 4s)
                //    Use a single await with predicate matching both type and seq.
                var ctrl = await client.AwaitPacketAsync<Directive>(
                    predicate: c =>
                        c.SequenceId == login.SequenceId &&
                        (c.Type == ControlType.ACK || c.Type == ControlType.ERROR),
                    timeoutMs: ServerTimeoutMs,
                    ct: _loginCts.Token).ConfigureAwait(false);

                if (ctrl.Type == ControlType.ACK)
                {
                    SceneManager.FindByType<Notification>()?.UpdateMessage("Welcome!");
                    SceneManager.ChangeScene(SceneNames.Main); // hoặc CharacterSelect
                    return;
                }

                // ERROR path: decide UX based on code/action
                var msg = MapErrorMessage(ctrl.Reason);
                SceneManager.FindByType<Notification>()?.UpdateMessage(msg);

                var backoff = MapBackoff(ctrl.Action);
                if (backoff is TimeSpan wait && wait > TimeSpan.Zero)
                {
                    // Optional: gray out login button during backoff to align with ProtocolAction
                    await System.Threading.Tasks.Task.Delay(wait, _loginCts.Token).ConfigureAwait(false);
                }

                if (ctrl.Action == ProtocolAction.DO_NOT_RETRY)
                {
                    // Keep disabled or navigate back depending on policy
                    _user.Enabled = true; _pass.Enabled = true; _loginBtn.Enabled = false;
                }
                else if (ctrl.Action == ProtocolAction.REAUTHENTICATE)
                {
                    _pass.Focused = true;
                }
            }
            catch (System.OperationCanceledException)
            {
                SceneManager.FindByType<Notification>()?.UpdateMessage("Login cancelled or timed out.");
            }
            catch (System.TimeoutException)
            {
                SceneManager.FindByType<Notification>()?.UpdateMessage("Login timeout. Please try again.");
            }
            catch (System.Exception ex)
            {
                NLogix.Host.Instance.Error("LOGIN exception", ex);
                SceneManager.FindByType<Notification>()?.UpdateMessage("Login failed due to an error.");
            }
            finally
            {
                _pass.Destroy();
                _loggingIn = false;
                LockUi(false);
            }
        }

        private void LockUi(Boolean on)
        {
            _user.Enabled = !on;
            _pass.Enabled = !on;
            _backBtn.Enabled = !on;
            _loginBtn.Enabled = !on;
            _loginBtn.SetText(on ? "Signing in..." : "Sign in");
        }

        #endregion

        private static String MapErrorMessage(ProtocolCode code) => code switch
        {
            ProtocolCode.UNAUTHENTICATED => "Invalid username or password.",
            ProtocolCode.ACCOUNT_LOCKED => "Too many failed attempts. Please wait and try again.",
            ProtocolCode.ACCOUNT_SUSPENDED => "Your account is suspended.",
            ProtocolCode.VALIDATION_FAILED => "Please fill both username and password.",
            ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch.",
            ProtocolCode.CANCELLED => "Login cancelled.",
            ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
            _ => "Login failed."
        };

        // Optional backoff table for ProtocolAction
        private static TimeSpan? MapBackoff(ProtocolAction action) => action switch
        {
            ProtocolAction.BACKOFF_RETRY => TimeSpan.FromSeconds(3),
            ProtocolAction.REAUTHENTICATE => TimeSpan.Zero,  // immediate re-input
            ProtocolAction.DO_NOT_RETRY => null,           // block retries
            _ => null
        };

        private Boolean AllowRateLimitedSend()
        {
            var now = DateTime.UtcNow;
            // Trim entries older than 3s
            while (_loginSendTimes.Count > 0 && (now - _loginSendTimes.Peek()).TotalSeconds > 3)
            {
                _loginSendTimes.Dequeue();
            }

            if (_loginSendTimes.Count >= 2)
            {
                return false;
            }

            _loginSendTimes.Enqueue(now);
            return true;
        }
    }

    #endregion
}
