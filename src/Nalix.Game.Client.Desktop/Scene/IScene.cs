using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

internal interface IScene
{
    /// <summary>
    /// Gọi khi scene được khởi tạo.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Gọi khi scene bị thay thế.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Cập nhật logic mỗi frame.
    /// </summary>
    /// <param name="deltaTime">Thời gian trôi qua kể từ frame trước (tính bằng giây).</param>
    void Update(float deltaTime);

    /// <summary>
    /// Vẽ scene ra cửa sổ.
    /// </summary>
    void Draw(RenderWindow window);

    /// <summary>
    /// Xử lý input từ bàn phím.
    /// </summary>
    void HandleInput(KeyEventArgs e);

    /// <summary>
    /// Xử lý input từ chuột.
    /// </summary>
    void HandleMouseInput(MouseButtonEventArgs e);
}