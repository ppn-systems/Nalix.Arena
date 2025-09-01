using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Client.Objects.Controls;

/// <summary>
/// Nút co giãn dùng NineSlicePanel (1 ảnh), hover đổi màu bằng tint.
/// </summary>
public class StretchableButton : RenderObject
{
    private static readonly System.Single DefaultHeight = 50f;
    private static readonly System.Single DefaultWidth = 320f;
    private static readonly System.Single HorizontalPadding = 16f;

    private readonly Text _label;
    private readonly NineSlicePanel _panel;

    // state
    private System.Boolean _isHovered, _isPressed, _wasMousePressed;

    // layout
    private System.Single _buttonWidth;
    private FloatRect _totalBounds;
    private Vector2f _position = new(0, 0);

    // colors
    private Color _panelNormal = new(50, 50, 50);
    private Color _panelHover = new(70, 70, 70);

    private Color _textNormal = Color.Black;
    private Color _textHover = new(255, 255, 102);

    private event System.Action OnClick;

    /// <param name="text">Nội dung</param>
    /// <param name="width">Chiều rộng mong muốn</param>
    /// <param name="textureKey">Key texture trong atlas</param>
    /// <param name="sourceRect">Subrect trong texture (để dùng atlas), để default là full</param>
    public StretchableButton(
        System.String text,
        System.Single width = 240f,
        System.String textureKey = "panels/031",
        IntRect sourceRect = default)
    {
        var tex = Assets.UiTextures.Load(textureKey);
        _panel = new NineSlicePanel(tex, new Thickness(32), sourceRect);

        var font = Assets.Font.Load("1");
        _label = new Text(text, font, 20) { FillColor = Color.Black };

        _buttonWidth = System.Math.Max(DefaultWidth, width);
        UpdateLayout();
        ApplyHoverTint(false);
    }

    #region Public API

    public void SetWidth(System.Single width)
    {
        _buttonWidth = width;
        UpdateLayout();
    }

    public void SetText(System.String text)
    {
        _label.DisplayedString = text;
        UpdateLayout();
    }

    public void SetPosition(Vector2f position)
    {
        _position = position;
        UpdateLayout();
    }

    public void SetColors(Color? panelNormal = null, Color? panelHover = null)
    {
        if (panelNormal.HasValue)
        {
            _panelNormal = panelNormal.Value;
        }

        if (panelHover.HasValue)
        {
            _panelHover = panelHover.Value;
        }

        ApplyHoverTint(_isHovered);
    }

    public void RegisterClickHandler(System.Action handler) => OnClick += handler;
    public void UnregisterClickHandler(System.Action handler) => OnClick -= handler;

    public FloatRect GetGlobalBounds() => _totalBounds;

    #endregion

    #region Loop

    public override void Update(System.Single dt)
    {
        if (!Visible)
        {
            return;
        }

        var mousePos = InputState.GetMousePosition();
        System.Boolean isOver = _totalBounds.Contains(mousePos.X, mousePos.Y);
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        if (_isHovered != isOver)
        {
            _isHovered = isOver;
            ApplyHoverTint(_isHovered);
        }

        if (isOver && isDown && !_wasMousePressed)
        {
            _isPressed = true;
        }
        else if (_isPressed && !isDown && isOver)
        {
            OnClick?.Invoke();
            _isPressed = false;
        }
        else if (!isDown)
        {
            _isPressed = false;
        }

        _wasMousePressed = isDown;
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        // NineSlicePanel là Drawable => draw trực tiếp
        target.Draw(_panel);
        target.Draw(_label);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout

    private void UpdateLayout()
    {
        // đảm bảo đủ chỗ cho text + padding
        var tb = _label.GetLocalBounds();
        System.Single minTextWidth = tb.Width + (HorizontalPadding * 2f);

        System.Single totalWidth = System.Math.Max(_buttonWidth, System.Math.Max(DefaultWidth, minTextWidth));
        System.Single totalHeight = DefaultHeight;

        // set panel geometry theo NineSlicePanel API của bạn
        _panel.Position = _position;
        _panel.Size = new Vector2f(totalWidth, totalHeight);
        _panel.Layout(); // QUAN TRỌNG: phải gọi để áp transform/scale

        _totalBounds = new FloatRect(_position.X, _position.Y, totalWidth, totalHeight);
        CenterLabel(totalWidth, totalHeight);
    }

    private void CenterLabel(System.Single totalWidth, System.Single totalHeight)
    {
        var tb = _label.GetLocalBounds();
        System.Single x = _position.X + ((totalWidth - tb.Width) / 2f) - tb.Left;
        System.Single y = _position.Y + ((totalHeight - tb.Height) / 2f) - tb.Top;
        _label.Position = new Vector2f(x, y);
    }

    #endregion

    #region Helpers

    private void ApplyHoverTint(System.Boolean hovered)
    {
        _panel.SetColor(hovered ? _panelHover : _panelNormal);
        _label.FillColor = hovered ? _textHover : _textNormal;
    }

    #endregion
}
