using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Desktop.Objects.Controls;

/// <summary>
/// Nút co giãn dùng NineSlicePanel (1 ảnh), textHover đổi màu bằng tint.
/// </summary>
public class StretchableButton : RenderObject
{
    #region Config

    private const System.Single DefaultHeight = 50f;
    private const System.Single DefaultWidth = 0f;     // cho phép co theo text nếu width < min
    private const System.Single HorizontalPaddingDefault = 16f;
    private const System.UInt32 DefaultFontSize = 20;
    private static readonly Thickness DefaultSlice = new(32);
    private static readonly IntRect DefaultSrc = default;
    private const System.String DefaultTextureKey = "panels/031";

    #endregion

    #region Fields

    private readonly Text _label;
    private NineSlicePanel _panel;

    // state
    private System.Boolean _isHovered, _isPressed, _wasMousePressed;
    private System.Boolean _keyboardPressed;
    private System.Boolean _isEnabled = true;

    // layout
    private System.Single _buttonWidth;
    private System.Single _buttonHeight = DefaultHeight;
    private System.Single _horizontalPadding = HorizontalPaddingDefault;
    private FloatRect _totalBounds;
    private Vector2f _position = new(0, 0);

    // colors
    private Color _panelNormal = new(30, 30, 30);
    private Color _panelHover = new(60, 60, 60);
    private Color _panelDisabled = new(40, 40, 40, 180);

    private Color _textNormal = new(200, 200, 200);
    private Color _textHover = new(255, 255, 255);
    private Color _textDisabled = new(160, 160, 160, 200);

    private event System.Action OnClick;

    #endregion

    #region Ctor

    /// <param name="text">Nội dung</param>
    /// <param name="width">Chiều rộng mong muốn</param>
    /// <param name="textureKey">Key texture trong atlas</param>
    /// <param name="sourceRect">Subrect trong texture (để dùng atlas), để default là full</param>
    public StretchableButton(
        System.String text,
        System.Single width = 240f,
        System.String textureKey = DefaultTextureKey,
        IntRect sourceRect = default)
    {
        _panel = BuildPanel(textureKey, sourceRect);
        _label = BuildLabel(text);

        _buttonWidth = System.Math.Max(DefaultWidth, width);

        UpdateLayout();
        ApplyTint();
    }

    #endregion

    #region Build

    private static NineSlicePanel BuildPanel(System.String textureKey, IntRect sourceRect)
    {
        var tex = Assets.UiTextures.Load(textureKey);
        return new NineSlicePanel(tex, DefaultSlice, sourceRect == default ? DefaultSrc : sourceRect);
    }

    private static Text BuildLabel(System.String text)
    {
        var font = Assets.Font.Load("1");
        return new Text(text, font, DefaultFontSize) { FillColor = Color.Black };
    }

    #endregion

    #region Public API (fluent-friendly)

    public StretchableButton SetWidth(System.Single width)
    {
        _buttonWidth = width;
        UpdateLayout();
        return this;
    }
    public StretchableButton SetHeight(System.Single height)
    {
        _buttonHeight = height;
        UpdateLayout();
        return this;
    }
    public StretchableButton SetSize(System.Single width, System.Single height)
    {
        _buttonWidth = width;
        _buttonHeight = height;
        UpdateLayout();
        return this;
    }

    public StretchableButton SetText(System.String text)
    {
        _label.DisplayedString = text;
        UpdateLayout();
        return this;
    }
    public StretchableButton SetFontSize(System.UInt32 size)
    {
        _label.CharacterSize = size;
        UpdateLayout();
        return this;
    }
    public StretchableButton SetPadding(System.Single horizontalPadding)
    {
        _horizontalPadding = System.MathF.Max(0f, horizontalPadding);
        UpdateLayout();
        return this;
    }

    public void SetPosition(Vector2f position)
    {
        _position = position;
        UpdateLayout();
    }

    public StretchableButton SetColors(Color? panelNormal = null, Color? panelHover = null, Color? panelDisabled = null)
    {
        if (panelNormal.HasValue)
        {
            _panelNormal = panelNormal.Value;
        }

        if (panelHover.HasValue)
        {
            _panelHover = panelHover.Value;
        }

        if (panelDisabled.HasValue)
        {
            _panelDisabled = panelDisabled.Value;
        }

        ApplyTint();
        return this;
    }

    public StretchableButton SetTextColors(Color? textNormal = null, Color? textHover = null, Color? textDisabled = null)
    {
        if (textNormal.HasValue)
        {
            _textNormal = textNormal.Value;
        }

        if (textHover.HasValue)
        {
            _textHover = textHover.Value;
        }

        if (textDisabled.HasValue)
        {
            _textDisabled = textDisabled.Value;
        }

        ApplyTint();
        return this;
    }

    public StretchableButton SetEnabled(System.Boolean enabled) { _isEnabled = enabled; ApplyTint(); return this; }

    public StretchableButton SetTexture(System.String textureKey, IntRect sourceRect = default)
    {
        _panel = BuildPanel(textureKey, sourceRect);
        UpdateLayout();
        ApplyTint();
        return this;
    }

    public void SetTextOutline(Color outlineColor, System.Single thickness)
    {
        _label.OutlineColor = outlineColor;
        _label.OutlineThickness = thickness;
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

        // hover state
        if (_isHovered != (isOver && _isEnabled))
        {
            _isHovered = isOver && _isEnabled;
            ApplyTint();
        }

        // mouse press/click
        if (_isEnabled)
        {
            if (isOver && isDown && !_wasMousePressed)
            {
                _isPressed = true;
            }
            else if (_isPressed && !isDown && isOver) { FireClick(); _isPressed = false; }
            else if (!isDown)
            {
                _isPressed = false;
            }
        }
        _wasMousePressed = isDown;

        // keyboard (Enter/Space) when hovered — tiện cho pad/KB nav
        System.Boolean keyDown = InputState.IsKeyPressed(Keyboard.Key.Enter) || InputState.IsKeyPressed(Keyboard.Key.Space);
        if (_isEnabled && _isHovered)
        {
            if (keyDown && !_keyboardPressed) { _keyboardPressed = true; }
            else if (!keyDown && _keyboardPressed) { _keyboardPressed = false; FireClick(); }
        }
        else
        {
            _keyboardPressed = false;
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel); // NineSlicePanel là Drawable
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
        System.Single minTextWidth = tb.Width + (_horizontalPadding * 2f);

        System.Single totalWidth = System.Math.Max(_buttonWidth, System.Math.Max(DefaultWidth, minTextWidth));
        System.Single totalHeight = System.Math.Max(_buttonHeight, DefaultHeight);

        _ = _panel.SetPosition(_position).SetSize(new Vector2f(totalWidth, totalHeight));
        _panel.Layout(); // áp transform/scale

        _totalBounds = new FloatRect(_position.X, _position.Y, totalWidth, totalHeight);
        CenterLabel(totalWidth, totalHeight);
    }

    private void CenterLabel(System.Single totalWidth, System.Single totalHeight)
    {
        var tb = _label.GetLocalBounds();
        System.Single x = _position.X + ((totalWidth - tb.Width) * 0.5f) - tb.Left;
        System.Single y = _position.Y + ((totalHeight - tb.Height) * 0.5f) - tb.Top + 8f;
        _label.Position = new Vector2f(x, y);
    }

    #endregion

    #region Helpers

    private void ApplyTint()
    {
        if (!_isEnabled)
        {
            _ = _panel.SetColor(_panelDisabled);
            _label.FillColor = _textDisabled;
            return;
        }

        _ = _panel.SetColor(_isHovered ? _panelHover : _panelNormal);
        _label.FillColor = _isHovered ? _textHover : _textNormal;
    }

    private void FireClick() => OnClick?.Invoke();

    #endregion
}
