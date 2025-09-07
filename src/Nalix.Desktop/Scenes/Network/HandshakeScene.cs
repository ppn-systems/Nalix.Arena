using Nalix.Desktop.Objects.Indicators;
using Nalix.Rendering.Scenes;

namespace Nalix.Desktop.Scenes.Network;

public class HandshakeScene : Scene
{
    /// <summary>
    /// Khởi tạo một cảnh mạng với tên được xác định trong <see cref="SceneNames.Handshake"/>.
    /// </summary>
    public HandshakeScene() : base(SceneNames.Handshake)
    {
    }

    /// <summary>
    /// Tải các đối tượng cần thiết cho cảnh mạng, bao gồm hiệu ứng tải, trình xử lý kết nối và thông báo kết nối.
    /// </summary>
    protected override void LoadObjects() => AddObject(new LoadingSpinner());
}
