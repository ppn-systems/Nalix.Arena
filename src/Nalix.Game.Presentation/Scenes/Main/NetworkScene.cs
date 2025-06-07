using Nalix.Game.Presentation.Enums;
using Nalix.Game.Presentation.Objects;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Scenes;
using Nalix.Logging.Extensions;
using Nalix.Network.Package;
using Nalix.Shared.Net;
using SFML.Graphics;

namespace Nalix.Game.Presentation.Scenes.Main;

public class NetworkScene : Scene
{
    public NetworkScene() : base(SceneNames.Network)
    {
    }

    protected override void LoadObjects()
    {
        base.AddObject(new LoadingSpinner());
        base.AddObject(new NetworkHandler());
        base.AddObject(new NotificationBox("Connecting to the server...", Side.Top));
    }

    [IgnoredLoad("RenderObject")]
    private class NetworkHandler : RenderObject
    {
        private const float RetryDelay = 2f; // seconds

        private enum ConnectState
        {
            Waiting,
            Trying,
            Success,
            Failed
        }

        private int _attempt;
        private float _timer;
        private ConnectState _state;

        public NetworkHandler() // Accept NotificationBox in constructor
        {
            _attempt = 0;
            _timer = 0f;
            _state = ConnectState.Waiting;
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
                        NetClient<Packet>.Instance.Connect(20000);
                        NLogixFx.Info("Network attempt #{0} successful.", _attempt.ToString());

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
                    SceneManager.FindByType<NotificationBox>()
                                .UpdateMessage(
                                    "Network failed after multiple attempts. " +
                                    "Please check your network settings.");

                    _state = (ConnectState)(-1); // final state
                    break;
            }
        }

        public override void Render(RenderTarget target)
        {
            // No rendering needed for this object
        }

        protected override Drawable GetDrawable() => null;
    }
}