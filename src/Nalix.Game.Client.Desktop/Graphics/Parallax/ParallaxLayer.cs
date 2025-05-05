using SFML.Graphics;
using SFML.System;

namespace Nalix.Game.Client.Desktop.Graphics.Parallax;

internal class ParallaxLayer
{
    private readonly Texture _texture;
    private readonly Sprite _sprite;
    private readonly float _scrollSpeed;
    private float _offset;

    public ParallaxLayer(string texturePath, float scrollSpeed, bool scaleToFit = false)
    {
        _texture = new Texture(texturePath)
        {
            Repeated = true // Cho phép lặp lại hình nền
        };

        _sprite = new Sprite(_texture)
        {
            // Kích thước mặc định của TextureRect
            TextureRect = new IntRect(0, 0, MainWindow.WindowWidth, MainWindow.WindowHeight),
        };

        // Nếu cần phóng to, ta sẽ tính tỷ lệ phóng to
        if (scaleToFit)
        {
            Vector2u targetSize = _texture.Size;

            // Tính toán tỷ lệ phóng to dựa trên kích thước màn hình
            float scaleX = (float)MainWindow.WindowWidth / targetSize.X;
            float scaleY = (float)MainWindow.WindowHeight / targetSize.Y;

            // Áp dụng tỷ lệ phóng to cho sprite
            _sprite.Scale = new Vector2f(scaleX, scaleY);
        }

        _scrollSpeed = scrollSpeed;
    }

    public void Update(float deltaTime)
    {
        _offset += _scrollSpeed * deltaTime;

        // Cuộn ngang: offset là tọa độ x bắt đầu hiển thị từ texture
        _sprite.TextureRect = new IntRect((int)_offset, 0, MainWindow.WindowWidth, MainWindow.WindowHeight);
    }

    public void Draw(RenderWindow window) => window.Draw(_sprite);
}