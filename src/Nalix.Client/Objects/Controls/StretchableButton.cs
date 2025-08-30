using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Client.Objects.Controls;

/// <summary>
/// A stretchable button composed of three textures: left, center, and right.
/// The center texture stretches to fit the desired width.
/// </summary>
public class StretchableButton : RenderObject
{
    private static readonly System.Single DefaultHeight = 50f;
    private static readonly System.Single DefaultWidth = 320f;

    private readonly Text _label;
    private ButtonVisual _normalVisual;
    private ButtonVisual _hoverVisual;

    private System.Boolean _isHovered = false;
    private System.Boolean _isPressed = false;
    private System.Boolean _wasMousePressed = false;

    private System.Single _buttonWidth;
    private FloatRect _totalBounds;
    private Vector2f _position = new(0, 0);

    private event System.Action OnClick;

    public StretchableButton(System.String text, System.Single width = 240f)
    {
        _hoverVisual = LoadVisual("button/4", "button/5", "button/6");
        _normalVisual = LoadVisual("button/1", "button/2", "button/3");

        Font font = Assets.Font.Load("1");
        _label = new Text(text, font, 20) { FillColor = Color.White };
        _buttonWidth = System.Math.Max(DefaultWidth, width);

        this.UpdateLayout();
    }

    public void SetWidth(System.Single width)
    {
        _buttonWidth = width;
        this.UpdateLayout();
    }

    public void SetText(System.String text)
    {
        _label.DisplayedString = text;
        this.UpdateLayout();
    }

    public void SetPosition(Vector2f position)
    {
        _position = position;
        this.UpdateLayout();
    }

    public void RegisterClickHandler(System.Action handler) => this.OnClick += handler;

    public void UnregisterClickHandler(System.Action handler) => this.OnClick -= handler;

    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        Vector2i mousePos = InputState.GetMousePosition();
        System.Boolean isMouseOver = _totalBounds.Contains(mousePos.X, mousePos.Y);
        System.Boolean isMousePressed = Mouse.IsButtonPressed(Mouse.Button.Left);

        _isHovered = isMouseOver;

        if (isMouseOver && isMousePressed && !_wasMousePressed)
        {
            _isPressed = true;
        }
        else if (_isPressed && !isMousePressed && isMouseOver)
        {
            this.OnClick?.Invoke();
            _isPressed = false;
        }
        else if (!isMousePressed)
        {
            _isPressed = false;
        }

        _wasMousePressed = isMousePressed;
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        ref ButtonVisual visual = ref (_isHovered ? ref _hoverVisual : ref _normalVisual);
        target.Draw(visual.Left);
        target.Draw(visual.Center);
        target.Draw(visual.Right);
        target.Draw(_label);
    }

    public FloatRect GetGlobalBounds() => _totalBounds;

    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    private void UpdateLayout()
    {
        System.Single leftWidth = _normalVisual.Left.TextureRect.Width;
        System.Single rightWidth = _normalVisual.Right.TextureRect.Width;

        System.Single minTextWidth = _label.GetLocalBounds().Width + 32;
        System.Single totalWidth = System.Math.Max(_buttonWidth, minTextWidth + leftWidth + rightWidth);

        System.Single middleWidth = totalWidth - leftWidth - rightWidth;

        // Set all sprites
        ConfigureVisual(ref _normalVisual, middleWidth, DefaultHeight, _position);
        ConfigureVisual(ref _hoverVisual, middleWidth, DefaultHeight, _position);

        _totalBounds = new FloatRect(
            _position.X,
            _position.Y,
            totalWidth,
            DefaultHeight
        );

        this.CenterLabel(totalWidth);
    }

    private void CenterLabel(System.Single totalWidth)
    {
        FloatRect textBounds = _label.GetLocalBounds();
        System.Single x = _position.X + (totalWidth - textBounds.Width) / 2f - textBounds.Left;
        System.Single y = _position.Y + (DefaultHeight - textBounds.Height) / 2f - textBounds.Top;
        _label.Position = new Vector2f(x, y);
    }

    private static void ConfigureVisual(ref ButtonVisual visual, System.Single middleWidth, System.Single height, Vector2f position)
    {
        System.Single leftScaleY = height / visual.Left.Texture.Size.Y;
        System.Single rightScaleY = height / visual.Right.Texture.Size.Y;
        System.Single centerScaleY = height / visual.Center.Texture.Size.Y;
        System.Single centerScaleX = middleWidth / visual.Center.Texture.Size.X;

        visual.Left.Scale = new Vector2f(1f, leftScaleY);
        visual.Center.Scale = new Vector2f(centerScaleX, centerScaleY);
        visual.Right.Scale = new Vector2f(1f, rightScaleY);

        System.Single leftW = visual.Left.GetGlobalBounds().Width;

        visual.Left.Position = position;
        visual.Center.Position = new Vector2f(position.X + leftW, position.Y);
        visual.Right.Position = new Vector2f(visual.Center.Position.X + visual.Center.GetGlobalBounds().Width, position.Y);
    }

    private static ButtonVisual LoadVisual(System.String left, System.String middle, System.String right)
    {
        Texture l = Assets.UiTextures.Load(left);
        Texture m = Assets.UiTextures.Load(middle);
        Texture r = Assets.UiTextures.Load(right);

        l.Smooth = m.Smooth = r.Smooth = true;

        return new ButtonVisual
        {
            Left = new Sprite(l),
            Center = new Sprite(m),
            Right = new Sprite(r),
        };
    }

    private struct ButtonVisual
    {
        public Sprite Left;
        public Sprite Center;
        public Sprite Right;
    }
}