using Nalix.Desktop.Enums;
using Nalix.Desktop.Objects.Indicators;
using Nalix.Desktop.Objects.Notifications;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote;
using Nalix.Shared.Injection;
using SFML.Graphics;

namespace Nalix.Desktop.Scenes.Network;

/// <summary>
/// Cảnh chịu trách nhiệm xử lý quá trình kết nối mạng trước khi vào trò chơi chính.
/// </summary>
public class ConnectScene : Scene
{
    /// <summary>
    /// Khởi tạo một cảnh mạng với tên được xác định trong <see cref="SceneNames.Network"/>.
    /// </summary>
    public ConnectScene() : base(SceneNames.Network)
    {
    }

    /// <summary>
    /// Tải các đối tượng cần thiết cho cảnh mạng, bao gồm hiệu ứng tải, trình xử lý kết nối và thông báo kết nối.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new LoadingSpinner());
        AddObject(new NetworkHandler());
        AddObject(new Notification("Connecting to the server...", Side.Top));
    }

    /// <summary>
    /// Đối tượng xử lý logic kết nối mạng, bao gồm thử lại và thông báo lỗi.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class NetworkHandler : RenderObject
    {
        private const System.Single RetryDelay = 3f;
        private const System.Int32 MaxAttempts = 3;

        private enum ConnectState { Waiting, Trying, Success, Failed, ShowFail, Done }

        private System.Int32 _attempt;
        private System.Single _timer;
        private ConnectState _state;

        // quản lý async
        private System.Threading.CancellationTokenSource _cts;
        private System.Threading.Tasks.Task _connectTask;

        public NetworkHandler()
        {
            _timer = 0f;
            _attempt = 4;
            _state = ConnectState.Waiting;
        }

        public void ForceTryNow()
        {
            if (_state is ConnectState.Waiting)
            {
                _timer = RetryDelay; // ép sang Trying ở frame kế
            }
        }

        public override void Update(System.Single dt)
        {
            _timer += dt;

            switch (_state)
            {
                case ConnectState.Waiting:
                    if (_timer >= RetryDelay)
                    {
                        _attempt++;
                        SceneManager.FindByType<Notification>()
                            ?.UpdateMessage($"Connecting… (attempt {_attempt}/{MaxAttempts})");

                        StartConnect();
                        _state = ConnectState.Trying;
                        _timer = 0f;
                    }
                    break;

                case ConnectState.Trying:
                    if (_connectTask == null)
                    {
                        break;
                    }

                    if (_connectTask.IsFaulted)
                    {
                        // lỗi kết nối
                        CleanupTask();
                        if (_attempt >= MaxAttempts)
                        {
                            SceneManager.FindByType<Notification>()
                                ?.UpdateMessage("Lost connection to the server. Please try again.");
                            _state = ConnectState.Failed;
                        }
                        else
                        {
                            _state = ConnectState.Waiting;
                            _timer = 0f;
                        }
                    }
                    else if (_connectTask.IsCompletedSuccessfully)
                    {
                        CleanupTask();
                        _state = ConnectState.Success;
                    }
                    break;

                case ConnectState.Success:
                    SceneManager.QueueDestroy(this);
                    SceneManager.ChangeScene(SceneNames.Login);
                    _state = ConnectState.Done;
                    break;

                case ConnectState.Failed:
                    // dừng lại, giữ thông báo; có nút Try now/Cancel cho user
                    if (SceneManager.FindByType<Notification>() != null)
                    {
                        _state = ConnectState.ShowFail;
                    }

                    SceneManager.FindByType<Notification>()?.Destroy();
                    ActionNotification box = new("Lost connection to the server...")
                    {
                        ButtonExtraOffsetY = 32f
                    };

                    // Khi bấm OK
                    box.RegisterAction(() =>
                    {
                        box.Destroy();
                        System.Environment.Exit(0);
                    });
                    box.Spawn();

                    break;

                case ConnectState.Done:
                    // no-op
                    break;
            }
        }

        private void StartConnect()
        {
            CleanupTask();
            _cts = new System.Threading.CancellationTokenSource();

            try
            {
                ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
                // chạy connect async và giữ Task lại để polling trong Update
                _connectTask = client.ConnectAsync(20000, _cts.Token);
            }
            catch (System.Exception)
            {
                // ctor DI fail, coi như lỗi ngay
                _connectTask = System.Threading.Tasks.Task.FromException(new System.Exception("DI failed"));
            }
        }

        private void CleanupTask()
        {
            try { _cts?.Cancel(); } catch { }
            _cts?.Dispose();
            _cts = null;
            _connectTask = null;
        }

        public override void BeforeDestroy() => CleanupTask();

        public override void Render(RenderTarget target) { }
        protected override Drawable GetDrawable() => null;
    }

}