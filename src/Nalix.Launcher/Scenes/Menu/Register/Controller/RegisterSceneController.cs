using Nalix.Common.Protocols;
using Nalix.Communication.Collections;
using Nalix.Communication.Enums;
using Nalix.Communication.Models;
using Nalix.Framework.Injection;
using Nalix.Framework.Randomization;
using Nalix.Launcher.Enums;
using Nalix.Launcher.Objects.Notifications;
using Nalix.Launcher.Scenes.Menu.Login.Model;
using Nalix.Launcher.Scenes.Menu.Main.View;
using Nalix.Launcher.Scenes.Menu.Register.View;
using Nalix.Launcher.Services.Abstractions;
using Nalix.Logging;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Input;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote;
using Nalix.SDK.Remote.Configuration;
using Nalix.SDK.Remote.Extensions;
using Nalix.Shared.Messaging.Controls;

namespace Nalix.Launcher.Scenes.Menu.Register.Controller;

[IgnoredLoad("RenderObject")]
internal sealed class RegisterSceneController(RegisterView view, IThemeProvider theme, ISceneNavigator nav, IParallaxPresetProvider parallaxPreset)
{
    private readonly IParallaxPresetProvider _parallaxPresets = parallaxPreset ?? throw new System.ArgumentNullException(nameof(parallaxPreset));
    private readonly IThemeProvider _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
    private readonly ISceneNavigator _nav = nav ?? throw new System.ArgumentNullException(nameof(nav));
    private readonly RegisterView _view = view ?? throw new System.ArgumentNullException(nameof(view));
    private readonly Notification _notification = new("Welcome! Please register.", Side.Top);
    private readonly LoginModel _model = new();

    private System.Threading.CancellationTokenSource _loginCts;
    private const System.Int32 CooldownMs = 600;
    private ParallaxLayerView _parallaxLayerView;
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
        if (InputState.IsKeyPressed(SFML.Window.Keyboard.Key.Tab))
        {
            _view.OnTab();
        }

        if (InputState.IsKeyPressed(SFML.Window.Keyboard.Key.Enter))
        {
            _view.OnEnter();
        }

        if (InputState.IsKeyPressed(SFML.Window.Keyboard.Key.Escape))
        {
            _view.OnEscape();
        }

        if (InputState.IsKeyPressed(SFML.Window.Keyboard.Key.F2))
        {
            _view.OnTogglePassword();
        }
    }

    private async System.Threading.Tasks.Task TryLoginAsync()
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

        ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
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
        _loginCts = new System.Threading.CancellationTokenSource(ServerTimeoutMs);
        _model.IsBusy = true;
        _view.LockUi(true);

        try
        {
            // Build credentials packet (mã hoá + sequence id)
            CredentialsPacket register = new();
            TransportOptions options = client.Options;
            Credentials creds = new() { Username = user, Password = pass };

            register.SequenceId = SecureRandom.NextUInt32();
            register.Initialize((System.UInt16)OpCommand.REGISTER, creds);

            await client.SendAsync(register, _loginCts.Token).ConfigureAwait(false);

            using CompositeSubscription subs = client.Subscribe(
                client.On<Directive>(d => client.TryHandleDirectiveAsync(d, null, null, _loginCts.Token))
            );

            Directive ctrl = await client.AwaitPacketAsync<Directive>(
                predicate: c =>
                    c.SequenceId == register.SequenceId &&
                    (c.Type == ControlType.ACK || c.Type == ControlType.ERROR),
                timeoutMs: ServerTimeoutMs,
                ct: _loginCts.Token
            ).ConfigureAwait(false);

            if (ctrl.Type == ControlType.ACK)
            {
                ShowNote("Welcome!");
                _nav.Change(SceneNames.Main);
                GraphicsEngine.OnUpdate -= Update;
                return;
            }

            System.String msg = MapErrorMessage(ctrl.Reason);
            ShowNote(msg);

            System.TimeSpan? backoff = MapBackoff(ctrl.Action);
            if (backoff is System.TimeSpan wait && wait > System.TimeSpan.Zero)
            {
                await System.Threading.Tasks.Task.Delay(wait, _loginCts.Token).ConfigureAwait(false);
            }

            if (ctrl.Action == ProtocolAction.DO_NOT_RETRY)
            {
                // khoá nút register tuỳ chính sách
                _view.LockUi(true);
            }
            else if (ctrl.Action == ProtocolAction.REAUTHENTICATE)
            {
                _view.FocusPass();
            }
        }
        catch (System.OperationCanceledException)
        {
            ShowNote("Register cancelled or timed out.");
        }
        catch (System.TimeoutException)
        {
            ShowNote("Register timeout. Please try again.");
        }
        catch (System.Exception ex)
        {
            NLogix.Host.Instance.Error("REGISTER exception", ex);
            ShowNote("Register failed due to an error.");
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

        _ = AutoHideAsync();
    }

    /// <summary>
    /// Hides the notification after a fixed delay.
    /// </summary>
    private async System.Threading.Tasks.Task AutoHideAsync()
    {
        await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(5));
        _notification.Conceal();
    }

    private static System.String MapErrorMessage(ProtocolCode code) => code switch
    {
        ProtocolCode.UNAUTHENTICATED => "Invalid username or password.",
        ProtocolCode.ACCOUNT_LOCKED => "Too many failed attempts. Please wait and try again.",
        ProtocolCode.ACCOUNT_SUSPENDED => "Your account is suspended.",
        ProtocolCode.VALIDATION_FAILED => "Please check your username and password again.",
        ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch.",
        ProtocolCode.CANCELLED => "Register cancelled.",
        ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
        _ => "Register failed."
    };

    private static System.TimeSpan? MapBackoff(ProtocolAction action) => action switch
    {
        ProtocolAction.BACKOFF_RETRY => System.TimeSpan.FromSeconds(3),
        ProtocolAction.REAUTHENTICATE => System.TimeSpan.Zero,
        ProtocolAction.DO_NOT_RETRY => null,
        _ => null
    };
}
