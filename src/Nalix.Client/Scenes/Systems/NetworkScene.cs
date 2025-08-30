using Nalix.Client.Enums;
using Nalix.Client.Objects;
using Nalix.Common.Packets.Abstractions;
using Nalix.Logging.Extensions;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote.Core;
using Nalix.Shared.Injection;
using SFML.Graphics;

namespace Nalix.Client.Scenes.Systems;

/// <summary>
/// Cảnh chịu trách nhiệm xử lý quá trình kết nối mạng trước khi vào trò chơi chính.
/// </summary>
public class NetworkScene : Scene
{
    /// <summary>
    /// Khởi tạo một cảnh mạng với tên được xác định trong <see cref="SceneNames.Network"/>.
    /// </summary>
    public NetworkScene() : base(SceneNames.Network)
    {
    }

    /// <summary>
    /// Tải các đối tượng cần thiết cho cảnh mạng, bao gồm hiệu ứng tải, trình xử lý kết nối và thông báo kết nối.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new LoadingSpinner());
        AddObject(new NetworkHandler());
        AddObject(new NotificationBox("Connecting to the server...", Side.Top));
    }

    /// <summary>
    /// Đối tượng xử lý logic kết nối mạng, bao gồm thử lại và thông báo lỗi.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    private class NetworkHandler : RenderObject
    {
        private const System.Single RetryDelay = 3f; // thời gian chờ giữa các lần thử

        /// <summary>
        /// Trạng thái hiện tại của quá trình kết nối.
        /// </summary>
        private enum ConnectState
        {
            Waiting,
            Trying,
            Success,
            Failed
        }

        private System.Int32 _attempt;
        private System.Single _timer;
        private ConnectState _state;

        /// <summary>
        /// Khởi tạo đối tượng xử lý mạng với trạng thái ban đầu là chờ kết nối.
        /// </summary>
        public NetworkHandler()
        {
            _timer = 0f;
            _attempt = 0;

            _state = ConnectState.Waiting;
        }

        /// <summary>
        /// Cập nhật trạng thái kết nối theo thời gian. Gồm các bước chờ, thử kết nối, thành công hoặc thất bại.
        /// </summary>
        /// <param name="deltaTime">Thời gian trôi qua từ lần cập nhật trước (giây).</param>
        public override void Update(System.Single deltaTime)
        {
            _timer += deltaTime;

            switch (_state)
            {
                case ConnectState.Waiting:
                    if (_timer >= RetryDelay)
                    {
                        _state = ConnectState.Trying;
                        _timer = 0f;
                    }
                    break;

                case ConnectState.Trying:
                    try
                    {
                        _ = InstanceManager.Instance.GetOrCreateInstance<RemoteStreamClient<IPacket>>()
                                                .ConnectAsync(20000)
                                                .ConfigureAwait(false);

                        "Network attempt #{0} successful.".Info(_attempt.ToString());

                        _state = ConnectState.Success;
                    }
                    catch
                    {
                        _attempt++;
                        if (_attempt >= 3)
                        {
                            _state = ConnectState.Failed;
                        }
                        else
                        {
                            _state = ConnectState.Waiting;
                            _timer = 0f;
                        }
                    }
                    break;

                case ConnectState.Success:
                    SceneManager.QueueDestroy(this);
                    SceneManager.ChangeScene(SceneNames.Main);
                    break;

                case ConnectState.Failed:
                    SceneManager.FindByType<NotificationBox>()
                                .UpdateMessage("Lost connection to the server...");

                    _state = (ConnectState)(-1); // trạng thái kết thúc
                    break;
            }
        }

        /// <summary>
        /// Không cần vẽ gì trong đối tượng này vì nó chỉ xử lý logic.
        /// </summary>
        /// <param name="target">Đối tượng đích để render.</param>
        public override void Render(RenderTarget target)
        {
            // Không cần vẽ gì cho đối tượng này
        }

        /// <summary>
        /// Trả về Drawable null vì đối tượng này không có thành phần hiển thị.
        /// </summary>
        protected override Drawable GetDrawable() => null;
    }
}