using SFML.Graphics;
using SFML.System;

namespace Nalix.Game.Client.Desktop.Graphics;

internal class Camera
{
    #region Fields

    private readonly View _view;
    private Vector2f _position;
    private float _zoomLevel;
    private float _rotationAngle;

    #endregion Fields

    #region Properties

    public Vector2f Position
    {
        get => _position;
        set
        {
            _position = value;
            _view.Center = _position;
        }
    }

    public float ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            _zoomLevel = value;
            _view.Zoom(_zoomLevel);
        }
    }

    public float RotationAngle
    {
        get => _rotationAngle;
        set
        {
            _rotationAngle = value;
            _view.Rotation = _rotationAngle;
        }
    }

    #endregion Properties

    #region Constructor

    public Camera(float width, float height)
    {
        _view = new View(new FloatRect(0, 0, width, height)); // Tạo view với kích thước màn hình ban đầu
        _position = new Vector2f(width / 2, height / 2); // Camera ban đầu ở giữa màn hình
        _zoomLevel = 1f; // Mặc định zoom là 1
        _rotationAngle = 0f; // Mặc định góc xoay là 0 độ

        _view.Center = _position;
        _view.Zoom(_zoomLevel);
        _view.Rotation = _rotationAngle;
    }

    #endregion Constructor

    #region Methods

    // Cập nhật lại camera theo vị trí và các thay đổi của zoom/rotation
    public void Update(float deltaTime)
    {
        // Các logic khác như thay đổi vị trí camera theo key input, scroll zoom, v.v. có thể được thêm vào đây.
    }

    // Áp dụng camera cho window để hiển thị
    public void Apply(RenderWindow window)
    {
        window.SetView(_view);
    }

    // Di chuyển camera đến một vị trí mới
    public void Move(Vector2f delta)
    {
        _position += delta;
        _view.Center = _position;
    }

    // Thu phóng camera
    public void Zoom(float factor)
    {
        _zoomLevel *= factor;
        _view.Zoom(factor);
    }

    // Xoay camera một góc
    public void Rotate(float angle)
    {
        _rotationAngle += angle;
        _view.Rotation = _rotationAngle;
    }

    #endregion Methods
}