using Nalix.Game.Presentation.Enums;
using Nalix.Game.Presentation.Objects;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Scenes;
using Nalix.Network.Package;
using Nalix.Shared.Net;
using SFML.Graphics;

namespace Nalix.Game.Presentation.Scenes.Main;

public class ConnectionScene : Scene
{
    public ConnectionScene() : base(SceneNames.Connection)
    {
    }

    protected override void LoadObjects()
    {
        AddObject(new LoadingSpinner());
        AddObject(new ConnectionHandler());
    }

    [IgnoredLoad("RenderObject")]
    private class ConnectionHandler : RenderObject
    {
        private enum ConnectState
        {
            Waiting,
            Trying,
            Success,
            Failed
        }

        private readonly NotificationBox _box = new("Connecting to the server...", Side.Top);
        private ConnectState _state = ConnectState.Waiting;
        private int _attempt = 0;
        private float _timer = 0f;
        private const float RetryDelay = 2f; // seconds

        public ConnectionHandler()
        {
            base.Reveal(); // Start the connection process
        }

        public override void Update(float deltaTime)
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
                        NetClient<Packet>.Instance.ConnectAsync(10000).ConfigureAwait(false);
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
                    SceneManager.ChangeScene(SceneNames.Main);
                    break;

                case ConnectState.Failed:
                    _box.UpdateMessage(
                        "Connection failed after multiple attempts. " +
                        "Please check your network settings.");
                    // Chuyển trạng thái để không gọi lại
                    _state = (ConnectState)(-1); // final state
                    break;
            }
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;

            // Render the notification box
            _box.Render(target);
        }

        protected override Drawable GetDrawable() => null;
    }
}