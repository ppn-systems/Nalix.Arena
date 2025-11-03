using Nalix.Common.Protocols;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Framework.Injection;
using Nalix.Framework.Randomization;
using Nalix.Launcher.Enums;
using Nalix.Launcher.Objects.Notifications;
using Nalix.Launcher.Scenes.Menu.Login.Model;
using Nalix.Launcher.Scenes.Menu.Login.View;
using Nalix.Launcher.Scenes.Menu.Main.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Logging;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Input;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote;
using Nalix.SDK.Remote.Extensions;
using Nalix.Shared.Messaging.Controls;
using SFML.Window;

using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Launcher.Scenes.Menu.Login.Controller;

[IgnoredLoad("RenderObject")]
internal sealed class LoginSceneController(LoginView view, IThemeProvider theme, ISceneNavigator nav, IParallaxPresetProvider parallaxPreset)
{
    private readonly IParallaxPresetProvider _parallaxPresets = parallaxPreset ?? throw new System.ArgumentNullException(nameof(parallaxPreset));
    private readonly IThemeProvider _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
    private readonly ISceneNavigator _nav = nav ?? throw new System.ArgumentNullException(nameof(nav));
    private readonly LoginView _view = view ?? throw new System.ArgumentNullException(nameof(view));
    private readonly Notification _notification = new("Welcome! Please log in.", Side.Top);
    private ParallaxLayerView _parallaxLayerView;
    private readonly LoginModel _model = new();

    private CancellationTokenSource _loginCts;
    private const System.Int32 CooldownMs = 600;
    private const System.Int32 ServerTimeoutMs = 4000;

    public void Compose(Scene scene)
    {
        _notification.Conceal();

        scene.AddObject(_view);
        scene.AddObject(_notification);

        // Model parallax + view
        System.Int32 v = SecureRandom.GetInt32(1, 4);
        var preset = _parallaxPresets.GetByVariant(v);
        _parallaxLayerView = new ParallaxLayerView(_theme.Current, preset);
        scene.AddObject(_parallaxLayerView);
        WireView();

        GraphicsEngine.OnUpdate += Update;
    }

    private void WireView()
    {
        _view.SubmitRequested += () => _ = TryLoginAsync();
        _view.BackRequested += () => SceneManager.ChangeScene(SceneNames.Main);
        _view.TogglePasswordRequested += _view.TogglePassword;
        _view.TabToggled += _ => { /* không cần xử lý thêm */ };
    }

    public void Update(System.Single dt)
    {
        // nếu mất kết nối thì về Network scene
        var client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
        if (!client.IsConnected)
        {
            GraphicsEngine.OnUpdate -= Update;
            _nav.Change(SceneNames.Network);
            return;
        }

        // ==== phím tắt ====
        if (InputState.IsKeyPressed(Keyboard.Key.Tab))
        {
            _view.OnTab();
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Enter))
        {
            _view.OnEnter();
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Escape))
        {
            _view.OnEscape();
        }

        if (InputState.IsKeyPressed(Keyboard.Key.F2))
        {
            _view.OnTogglePassword();
        }
    }

    private async Task TryLoginAsync()
    {
        if (_model.IsBusy)
        {
            return;
        }

        // debounce
        if ((System.DateTime.UtcNow - _model.LastSubmitAtUtc).TotalMilliseconds < CooldownMs)
        {
            return;
        }

        _model.LastSubmitAtUtc = System.DateTime.UtcNow;

        var client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
        if (!client.IsConnected)
        {
            ShowNote("Not connected to server.");
            _nav.Change(SceneNames.Network);
            GraphicsEngine.OnUpdate -= Update;
            return;
        }

        // local rate limit
        if (!_model.AllowSend())
        {
            ShowNote("Too many attempts. Please wait a moment.");
            return;
        }

        System.String user = _view.Username;
        System.String pass = _view.Password;
        if (System.String.IsNullOrWhiteSpace(user) || System.String.IsNullOrEmpty(pass))
        {
            ShowNote("Please enter username & password");
            return;
        }

        _loginCts?.Dispose();
        _loginCts = new CancellationTokenSource(ServerTimeoutMs);
        _model.IsBusy = true;
        _view.LockUi(true);

        try
        {
            // Build credentials packet (mã hoá + sequence id)
            var options = client.Options;
            var creds = new Credentials { Username = user, Password = pass };
            var login = new CredentialsPacket();
            login.Initialize((System.UInt16)OpCommand.LOGIN, creds);
            login.SequenceId = SecureRandom.NextUInt32();
            login = CredentialsPacket.Encrypt(login, options.EncryptionKey, options.EncryptionMode);

            await client.SendAsync(login, _loginCts.Token).ConfigureAwait(false);

            using var subs = client.Subscribe(
                client.On<Directive>(d => client.TryHandleDirectiveAsync(d, null, null, _loginCts.Token))
            );

            var ctrl = await client.AwaitPacketAsync<Directive>(
                predicate: c =>
                    c.SequenceId == login.SequenceId &&
                    (c.Type == ControlType.ACK || c.Type == ControlType.ERROR),
                timeoutMs: ServerTimeoutMs,
                ct: _loginCts.Token).ConfigureAwait(false);

            if (ctrl.Type == ControlType.ACK)
            {
                ShowNote("Welcome!");
                _nav.Change(SceneNames.Main);
                GraphicsEngine.OnUpdate -= Update;
                return;
            }

            var msg = MapErrorMessage(ctrl.Reason);
            ShowNote(msg);

            var backoff = MapBackoff(ctrl.Action);
            if (backoff is System.TimeSpan wait && wait > System.TimeSpan.Zero)
            {
                await Task.Delay(wait, _loginCts.Token).ConfigureAwait(false);
            }

            if (ctrl.Action == ProtocolAction.DO_NOT_RETRY)
            {
                // khoá nút login tuỳ chính sách
                _view.LockUi(true);
            }
            else if (ctrl.Action == ProtocolAction.REAUTHENTICATE)
            {
                _view.FocusPass();
            }
        }
        catch (System.OperationCanceledException)
        {
            ShowNote("Login cancelled or timed out.");
        }
        catch (System.TimeoutException)
        {
            ShowNote("Login timeout. Please try again.");
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error("LOGIN exception", ex);
            ShowNote("Login failed due to an error.");
        }
        finally
        {
            _model.IsBusy = false;
            _view.LockUi(false);
        }
    }

    // ===== helpers =====

    private void ShowNote(System.String msg)
    {
        _notification.Reveal();
        _notification.UpdateMessage(msg);
    }

    private static System.String MapErrorMessage(ProtocolCode code) => code switch
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

    private static System.TimeSpan? MapBackoff(ProtocolAction action) => action switch
    {
        ProtocolAction.BACKOFF_RETRY => System.TimeSpan.FromSeconds(3),
        ProtocolAction.REAUTHENTICATE => System.TimeSpan.Zero,
        ProtocolAction.DO_NOT_RETRY => null,
        _ => null
    };
}
