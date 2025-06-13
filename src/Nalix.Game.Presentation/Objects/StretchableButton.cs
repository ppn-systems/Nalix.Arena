using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Objects;

/// <summary>
/// Lớp nút bấm có thể co giãn theo nội dung, gồm ba phần texture: trái, giữa và phải.
/// Phần giữa có thể mở rộng theo chiều rộng của nút.
/// </summary>
public class StretchableButton : RenderObject
{
    private static readonly float ButtonHeight = 50f; // Chiều cao mặc định của nút
    private static readonly float ButtonWidth = 320f; // Chiều rộng mặc định của nút

    // Các thành phần đồ họa của nút
    private readonly Text _buttonText; // Văn bản hiển thị trên nút

    private readonly Sprite _leftPart; // Phần trái của nút
    private readonly Sprite _middlePart; // Phần giữa của nút (co giãn)
    private readonly Sprite _rightPart; // Phần phải của nút
    private readonly Sprite _leftPartHover; // Phần trái khi di chuột vào
    private readonly Sprite _middlePartHover; // Phần giữa khi di chuột vào
    private readonly Sprite _rightPartHover; // Phần phải khi di chuột vào

    private bool _isHovered = false; // Trạng thái hover (di chuột vào)
    private bool _isPressed = false; // Trạng thái nhấn chuột
    private bool _wasMousePressed = false; // Trạng thái chuột đã nhấn trước đó

    private event Action _onClick; // Sự kiện được gọi khi nút được nhấn
    private FloatRect _totalBounds; // Tổng vùng chứa của nút

    private Vector2f _position = new(0, 0); // Vị trí của nút

    private float _buttonWidth; // Chiều rộng tổng của nút (cả 3 phần)

    /// <summary>
    /// Khởi tạo một nút co giãn với nội dung văn bản và chiều rộng tùy chỉnh.
    /// </summary>
    /// <param name="text">Nội dung của nút.</param>
    /// <param name="buttonWidth">Chiều rộng mong muốn của nút.</param>
    public StretchableButton(string text, float buttonWidth = 240f)
    {
        // Load các texture cho các phần của nút
        Texture left = Assets.UiTextures.Load("button/1");
        Texture midd = Assets.UiTextures.Load("button/2");
        Texture right = Assets.UiTextures.Load("button/3");

        // Load texture khi di chuột vào
        Texture leftHover = Assets.UiTextures.Load("button/4");
        Texture middHover = Assets.UiTextures.Load("button/5");
        Texture rightHover = Assets.UiTextures.Load("button/6");

        // Kích hoạt chế độ làm mịn cho texture
        left.Smooth = midd.Smooth = right.Smooth = true;
        leftHover.Smooth = middHover.Smooth = rightHover.Smooth = true;

        // Khởi tạo sprite cho nút bình thường
        _leftPart = new Sprite(left) { Scale = new Vector2f(0.5f, 0.5f) };
        _middlePart = new Sprite(midd) { Scale = new Vector2f(0.5f, 0.5f) };
        _rightPart = new Sprite(right) { Scale = new Vector2f(0.5f, 0.5f) };

        // Khởi tạo sprite cho trạng thái hover
        _leftPartHover = new Sprite(leftHover) { Scale = new Vector2f(0.5f, 0.5f) };
        _middlePartHover = new Sprite(middHover) { Scale = new Vector2f(0.5f, 0.5f) };
        _rightPartHover = new Sprite(rightHover) { Scale = new Vector2f(0.5f, 0.5f) };

        // Khởi tạo văn bản hiển thị trên nút
        Font font = Assets.Font.Load("1");
        _buttonText = new Text(text, font, 20) { FillColor = Color.White };

        // Thiết lập chiều rộng tổng của nút
        _buttonWidth = Math.Max(ButtonWidth, buttonWidth);

        // Cập nhật kích thước của nút
        UpdateButtonSize();
    }

    /// <summary>
    /// Đặt chiều rộng của nút, cập nhật lại kích thước nếu cần.
    /// </summary>
    public void SetButtonWidth(float width)
    {
        _buttonWidth = width;
        UpdateButtonSize();
    }

    /// <summary>
    /// Cập nhật trạng thái của nút mỗi khung hình.
    /// </summary>
    public override void Update(float deltaTime)
    {
        if (!Visible) return;

        Vector2i mousePos = InputState.GetMousePosition();
        bool isMouseOver = _totalBounds.Contains(mousePos.X, mousePos.Y);
        bool isMousePressed = Mouse.IsButtonPressed(Mouse.Button.Left);

        _isHovered = isMouseOver;

        if (isMouseOver && isMousePressed && !_wasMousePressed)
        {
            _isPressed = true;
        }
        else if (_isPressed && !isMousePressed && isMouseOver)
        {
            _onClick?.Invoke();
            _isPressed = false;
        }
        else if (!isMousePressed)
        {
            _isPressed = false;
        }

        _wasMousePressed = isMousePressed;
    }

    /// <summary>
    /// Vẽ nút lên màn hình, hiển thị đúng trạng thái (thường hoặc hover).
    /// </summary>
    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        if (!_isHovered)
        {
            target.Draw(_leftPart);
            target.Draw(_middlePart);
            target.Draw(_rightPart);
        }
        else
        {
            target.Draw(_leftPartHover);
            target.Draw(_middlePartHover);
            target.Draw(_rightPartHover);
        }

        target.Draw(_buttonText);
    }

    /// <summary>
    /// Đăng ký sự kiện khi nhấn nút.
    /// </summary>
    public void RegisterClickHandler(Action handler)
    {
        _onClick += handler;
    }

    /// <summary>
    /// Hủy đăng ký sự kiện khi nhấn nút.
    /// </summary>
    public void UnregisterClickHandler(Action handler)
    {
        _onClick -= handler;
    }

    /// <summary>
    /// Đặt nội dung mới cho nút, cập nhật kích thước nếu cần.
    /// </summary>
    public void SetText(string newText)
    {
        _buttonText.DisplayedString = newText;
        UpdateButtonSize();
    }

    /// <summary>
    /// Đặt vị trí của nút trên màn hình.
    /// </summary>
    public void SetPosition(Vector2f position)
    {
        _position = position;
        UpdateButtonSize();
    }

    /// <summary>
    /// Lấy kích thước toàn bộ của nút.
    /// </summary>
    public FloatRect GetGlobalBounds() => _totalBounds;

    /// <summary>
    /// Cập nhật lại kích thước của nút để phù hợp với nội dung và vị trí.
    /// </summary>
    private void UpdateButtonSize()
    {
        // Scale chiều cao cho cả 3 phần
        float leftScaleY = ButtonHeight / _leftPart.Texture.Size.Y;
        float middleScaleY = ButtonHeight / _middlePart.Texture.Size.Y;
        float rightScaleY = ButtonHeight / _rightPart.Texture.Size.Y;

        _leftPart.Scale = new Vector2f(1f, leftScaleY);
        _middlePart.Scale = new Vector2f(1f, middleScaleY);
        _rightPart.Scale = new Vector2f(1f, rightScaleY);

        // Scale cho hover sprites
        _leftPartHover.Scale = _leftPart.Scale;
        _middlePartHover.Scale = _middlePart.Scale;
        _rightPartHover.Scale = _rightPart.Scale;

        // Tính chiều rộng thực tế của trái/phải (sau khi scale)
        float leftW = _leftPart.GetGlobalBounds().Width;
        float rightW = _rightPart.GetGlobalBounds().Width;

        // Đảm bảo button đủ rộng cho text
        float minWidth = _buttonText.GetLocalBounds().Width + 32 + leftW + rightW;
        float totalWidth = Math.Max(_buttonWidth, minWidth);

        // Tính chiều rộng phần giữa (middle)
        float middleWidth = totalWidth - leftW - rightW;
        float middleScaleX = middleWidth / _middlePart.Texture.Size.X;
        _middlePart.Scale = new Vector2f(middleScaleX, middleScaleY);
        _middlePartHover.Scale = _middlePart.Scale;

        // Đặt vị trí các phần
        _leftPart.Position = _position;
        _middlePart.Position = new Vector2f(_position.X + leftW, _position.Y);
        _rightPart.Position = new Vector2f(_middlePart.Position.X + _middlePart.GetGlobalBounds().Width, _position.Y);

        _leftPartHover.Position = _leftPart.Position;
        _middlePartHover.Position = _middlePart.Position;
        _rightPartHover.Position = _rightPart.Position;

        // Tổng bounds
        _totalBounds = new FloatRect(
            _position.X, _position.Y,
            leftW + _middlePart.GetGlobalBounds().Width + rightW,
            ButtonHeight);

        // Căn giữa chữ
        float buttonWidth = _totalBounds.Width;
        float textX = _position.X + ((buttonWidth - _buttonText.GetLocalBounds().Width) / 2f) - _buttonText.GetLocalBounds().Left;
        float textY = _position.Y + ((ButtonHeight - _buttonText.GetLocalBounds().Height) / 2f) - _buttonText.GetLocalBounds().Top;
        _buttonText.Position = new Vector2f(textX, textY);
    }

    /// <summary>
    /// Lấy đối tượng Drawable (không được hỗ trợ, sử dụng Render() thay thế).
    /// </summary>
    /// <returns>Không trả về giá trị, luôn ném ngoại lệ.</returns>
    /// <exception cref="System.NotSupportedException">Ném ra khi phương thức này được gọi.</exception>
    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead.");
}