using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Text;

namespace Nalix.Rendering.Effects.Visual.UI;

/// <summary>Ô nhập ký tự dùng NineSlicePanel của bạn làm nền.</summary>
public class TextField : RenderObject
{
    private readonly NineSlicePanel _panel;       // panel 9-slice (Drawable)
    private readonly Text _text;                  // SFML text để vẽ & đo
    private readonly RectangleShape _caret;       // caret nhấp nháy

    private readonly Single _paddingX = 10f;
    private readonly Single _paddingY = 6f;
    private readonly UInt32 _fontSize;

    protected readonly StringBuilder _buffer = new();

    private Boolean _focused;
    private Boolean _caretVisible;
    private Single _caretTimer;

    // cache rect vẽ để hit-test chuột
    private FloatRect _hitBox;

    public TextField(Texture panelTexture, Thickness border, IntRect sourceRect,
                     Font font, UInt32 fontSize,
                     Vector2f size, Vector2f position)
    {
        _fontSize = fontSize;

        // Panel của bạn (Drawable)
        _panel = new NineSlicePanel(panelTexture, border, sourceRect)
        {
            Position = position,
            Size = EnsureMinSize(size, border)
        };
        _panel.Layout();

        _text = new Text("", font, _fontSize)
        {
            FillColor = new Color(30, 30, 30),
            Position = new Vector2f(
                position.X + _paddingX,
                position.Y + ((_panel.Size.Y - _fontSize) / 2f) - 2f)
        };

        _caret = new RectangleShape(new Vector2f(1f, _fontSize))
        {
            FillColor = _text.FillColor
        };

        UpdateHitBox();
        UpdateCaretNow();

        SetZIndex(800);
    }

    #region Public API
    public String Text
    {
        get => _buffer.ToString();
        set { _ = _buffer.Clear().Append(value ?? ""); UpdateCaretNow(); }
    }

    public Boolean Focused
    {
        get => _focused;
        set { _focused = value; _caretVisible = value; _caretTimer = 0f; }
    }

    public Vector2f Position
    {
        get => _panel.Position;
        set
        {
            _panel.Position = value;
            _panel.Layout();
            _text.Position = new Vector2f(
                value.X + _paddingX,
                value.Y + ((_panel.Size.Y - _fontSize) / 2f) - 2f);
            UpdateHitBox();
            UpdateCaretNow();
        }
    }

    public Vector2f Size
    {
        get => _panel.Size;
        set
        {
            _panel.Size = EnsureMinSize(value, _panel.Border);
            _panel.Layout();
            _text.Position = new Vector2f(
                _panel.Position.X + _paddingX,
                _panel.Position.Y + ((_panel.Size.Y - _fontSize) / 2f) - 2f);
            UpdateHitBox();
            UpdateCaretNow();
        }
    }

    public void SetPanelColor(Color color) => _panel.SetColor(color);
    public void SetTextColor(Color color) { _text.FillColor = color; _caret.FillColor = color; }
    #endregion

    #region Engine Hooks
    public override void Update(Single dt)
    {
        // Focus theo click
        if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            var mp = InputState.GetMousePosition();
            Focused = _hitBox.Contains(mp.X, mp.Y);
        }

        // Caret blink
        if (Focused)
        {
            _caretTimer += dt;
            if (_caretTimer >= 0.5f) { _caretVisible = !_caretVisible; _caretTimer = 0f; }
            HandleTyping();
        }

        // Cập nhật string vẽ & caret
        _text.DisplayedString = this.GetDisplayText();
        UpdateCaretNow();
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel); // NineSlicePanel của bạn là Drawable
        target.Draw(_text);
        if (Focused && _caretVisible)
        {
            target.Draw(_caret);
        }
    }

    // Bắt buộc override, nhưng không dùng (ta tự vẽ ở Render)
    protected override Drawable GetDrawable() => _text;

    protected virtual String GetDisplayText() => _buffer.ToString();

    #endregion

    #region Input & Caret
    private void HandleTyping()
    {
        Boolean shift = InputState.IsKeyDown(Keyboard.Key.LShift) || InputState.IsKeyDown(Keyboard.Key.RShift);

        // A..Z
        for (Keyboard.Key k = Keyboard.Key.A; k <= Keyboard.Key.Z; k++)
        {
            if (InputState.IsKeyPressed(k))
            {
                Char c = (Char)('a' + (k - Keyboard.Key.A));
                _ = _buffer.Append(shift ? Char.ToUpperInvariant(c) : c);
            }
        }

        // 0..9 (hàng số)
        for (Keyboard.Key k = Keyboard.Key.Num0; k <= Keyboard.Key.Num9; k++)
        {
            if (InputState.IsKeyPressed(k))
            {
                _ = _buffer.Append((Char)('0' + (k - Keyboard.Key.Num0)));
            }
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Space))
        {
            _ = _buffer.Append(' ');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Period))
        {
            _ = _buffer.Append('.');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Comma))
        {
            _ = _buffer.Append(',');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Hyphen))
        {
            _ = _buffer.Append('-');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Apostrophe))
        {
            _ = _buffer.Append('\'');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Backspace) && _buffer.Length > 0)
        {
            _ = _buffer.Remove(_buffer.Length - 1, 1);
        }

        // Enter/Tab: để scene bên ngoài xử lý (submit/switch field)
    }

    private void UpdateCaretNow()
    {
        // Vị trí caret = sau cuối text
        // Lưu ý: GetLocalBounds có thể có Left != 0 do glyph bearings
        var bounds = _text.GetLocalBounds();
        Single caretX = _text.Position.X + bounds.Left + bounds.Width + 1f;
        Single caretY = _text.Position.Y + 2f;
        _caret.Position = new Vector2f(caretX, caretY);
        _caret.Size = new Vector2f(1f, _fontSize);
    }
    #endregion

    #region Helpers
    private void UpdateHitBox()
    {
        var p = _panel.Position; var s = _panel.Size;
        _hitBox = new FloatRect(p.X, p.Y, s.X, s.Y);
    }

    private static Vector2f EnsureMinSize(Vector2f size, Thickness b)
    {
        Single minW = b.Left + b.Right + 1;
        Single minH = b.Top + b.Bottom + 1;
        return new Vector2f(MathF.Max(size.X, minW), MathF.Max(size.Y, minH));
    }
    #endregion
}
