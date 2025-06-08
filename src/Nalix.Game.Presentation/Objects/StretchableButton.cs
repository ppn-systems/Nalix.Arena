using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Objects;

/// <summary>
/// Button có 3 phần texture với phần giữa co giãn theo nội dung
/// </summary>
public class StretchableButton : RenderObject
{
    private static readonly float ButtonHeight = 50f;
    private static readonly float ButtonWidth = 320f;

    private readonly Sprite _leftPart;
    private readonly Sprite _middlePart;
    private readonly Sprite _rightPart;
    private readonly Text _buttonText;
    private Action _onClick;
    private FloatRect _totalBounds;

    private Vector2f _position = new(0, 0);

    // Chiều rộng tổng button (cả 3 phần)
    private float _buttonWidth;

    public StretchableButton(string text, float buttonWidth = 240f)
    {
        Texture left = Assets.UiTextures.Load("button/1");
        Texture midd = Assets.UiTextures.Load("button/2");
        Texture right = Assets.UiTextures.Load("button/3");

        left.Smooth = true;
        midd.Smooth = true;
        right.Smooth = true;

        _leftPart = new Sprite(left)
        {
            Scale = new Vector2f(0.5f, 0.5f)
        }
        ;
        _middlePart = new Sprite(midd)
        {
            Scale = new Vector2f(0.5f, 0.5f)
        }
        ;
        _rightPart = new Sprite(right)
        {
            Scale = new Vector2f(0.5f, 0.5f)
        }
        ;

        Font font = Assets.Font.Load("1");
        _buttonText = new Text(text, font, 20)
        {
            FillColor = Color.White
        };

        _buttonWidth = System.Math.Max(ButtonWidth, buttonWidth);

        this.UpdateButtonSize();
    }

    /// <summary>
    /// Đặt chiều rộng tổng của button (bao gồm cả 3 phần)
    /// </summary>
    public void SetButtonWidth(float width)
    {
        _buttonWidth = width;
        UpdateButtonSize();
    }

    private void UpdateButtonSize()
    {
        // Scale chiều cao cho cả 3 phần
        float leftScaleY = ButtonHeight / _leftPart.Texture.Size.Y;
        float middleScaleY = ButtonHeight / _middlePart.Texture.Size.Y;
        float rightScaleY = ButtonHeight / _rightPart.Texture.Size.Y;

        _leftPart.Scale = new Vector2f(1f, leftScaleY);
        _middlePart.Scale = new Vector2f(1f, middleScaleY);
        _rightPart.Scale = new Vector2f(1f, rightScaleY);

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

        // Đặt vị trí các phần
        _leftPart.Position = _position;
        _middlePart.Position = new Vector2f(_position.X + leftW, _position.Y);
        _rightPart.Position = new Vector2f(_middlePart.Position.X + _middlePart.GetGlobalBounds().Width, _position.Y);

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

    public override void Update(float deltaTime)
    {
        if (Mouse.IsButtonPressed(Mouse.Button.Left))
        {
            Vector2i mousePos = InputState.GetMousePosition();
            if (_totalBounds.Contains(mousePos.X, mousePos.Y))
            {
                _onClick?.Invoke();
            }
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        target.Draw(_leftPart);
        target.Draw(_middlePart);
        target.Draw(_rightPart);
        target.Draw(_buttonText);
    }

    public void RegisterClickHandler(Action handler)
    {
        _onClick += handler;
    }

    protected override Drawable GetDrawable()
    {
        throw new NotSupportedException("Sử dụng Render() thay vì GetDrawable()");
    }

    public void SetText(string newText)
    {
        _buttonText.DisplayedString = newText;
        UpdateButtonSize();
    }

    public void SetPosition(Vector2f position)
    {
        _position = position;
        UpdateButtonSize();
    }

    public FloatRect GetGlobalBounds() => _totalBounds;
}