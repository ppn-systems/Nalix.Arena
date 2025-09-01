// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Rendering.Effects.Visual.UI;

/// <summary>
/// Lightweight single-line text input built atop your <c>NineSlicePanel</c>.
/// </summary>
/// <remarks>
/// <para>
/// - Click to focus; caret blinks when focused.<br/>
/// - Typing supports A–Z, 0–9, a few punctuation keys (space, '.' ',' '-' '\''), Backspace/Delete with key-repeat.<br/>
/// - Text scrolls to ensure the caret (at end) is always visible inside the box width.<br/>
/// - Rendering order: panel → text → caret (if focused &amp; visible).<br/>
/// </para>
/// <para>
/// Ô nhập 1 dòng: click để focus, caret nhấp nháy, hỗ trợ gõ cơ bản, giữ Backspace/Delete để xóa nhanh,
/// và auto cuộn để luôn thấy caret ở cuối.
/// </para>
/// </remarks>
public class InputField : RenderObject
{
    #region ===== Constants & Defaults =====

    private const System.Single DefaultPaddingX = 16f;
    private const System.Single DefaultPaddingY = 6f;

    private const System.Single CaretBlinkPeriod = 0.5f; // (VN) Chu kỳ nháy caret
    private const System.Single KeyRepeatFirstDelay = 0.35f;
    private const System.Single KeyRepeatNextDelay = 0.05f;

    #endregion

    #region ===== Visuals & Layout =====

    private readonly NineSlicePanel _panel;
    private readonly Text _text;        // used for drawing
    private readonly Text _measure;     // used for measuring width/pos exactly
    private readonly RectangleShape _caret;

    private FloatRect _hitBox;          // cached for mouse hit-test

    // (VN) Padding có thể chỉnh nếu muốn
    private Vector2f _padding = new(DefaultPaddingX, DefaultPaddingY);

    private readonly System.UInt32 _fontSize;
    private System.Single _caretWidth = 1f;

    #endregion

    #region ===== State =====

    private readonly System.Text.StringBuilder _buffer = new();
    private System.Boolean _focused;
    private System.Boolean _caretVisible;
    private System.Single _caretTimer;

    private System.Single _repeatTimer;
    private System.Boolean _repeatBackspace;
    private System.Boolean _repeatDelete;
    private System.Boolean _prevBackspace;
    private System.Boolean _prevDelete;

    private System.Int32 _scrollStart; // start index of visible window (inclusive)

    #endregion

    #region ===== Options / Public Surface =====

    /// <summary>Maximum number of characters allowed; <c>null</c> means unlimited.</summary>
    public System.Int32? MaxLength { get; set; }

    /// <summary>Optional placeholder (shown when <see cref="Text"/> is empty and unfocused).</summary>
    public System.String Placeholder { get; set; } = System.String.Empty;

    /// <summary>If true, displayed text is replaced with bullets (but <see cref="Text"/> still holds real content).</summary>
    public System.Boolean PasswordMode { get; set; }

    /// <summary>Raised whenever <see cref="Text"/> changes.</summary>
    public event System.Action<System.String> OnChanged;

    /// <summary>Raised when user presses Enter while focused.</summary>
    public event System.Action<System.String> OnSubmit;

    /// <summary>Gets or sets the current text content.</summary>
    public System.String Text
    {
        get => _buffer.ToString();
        set
        {
            _ = _buffer.Clear().Append(value ?? System.String.Empty);
            ClampToMaxLength();
            ResetScrollAndCaret();
            OnChanged?.Invoke(_buffer.ToString());
        }
    }

    /// <summary>Gets or sets whether the field is focused.</summary>
    public System.Boolean Focused
    {
        get => _focused;
        set
        {
            _focused = value;
            _caretVisible = value;
            _caretTimer = 0f;
        }
    }

    /// <summary>Text position is derived from panel position + padding.</summary>
    public Vector2f Position
    {
        get => _panel.Position;
        set
        {
            _panel.Position = value;
            _panel.Layout();
            RelayoutText();
            UpdateHitBox();
            UpdateCaretImmediate();
        }
    }

    /// <summary>Panel size; text area is inner size minus padding.</summary>
    public Vector2f Size
    {
        get => _panel.Size;
        set
        {
            _panel.Size = EnsureMinSize(value, _panel.Border);
            _panel.Layout();
            RelayoutText();
            UpdateHitBox();
            ResetScrollAndCaret();
        }
    }

    /// <summary>Padding (x,y) inside the panel.</summary>
    public Vector2f Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            RelayoutText();
            ResetScrollAndCaret();
        }
    }

    /// <summary>Width of the caret in pixels.</summary>
    public System.Single CaretWidth
    {
        get => _caretWidth;
        set
        {
            _caretWidth = System.MathF.Max(0.5f, value);
            UpdateCaretImmediate();
        }
    }

    /// <summary>Set the panel's tint color.</summary>
    public void SetPanelColor(Color color) => _panel.SetColor(color);

    /// <summary>Set the text/caret color together.</summary>
    public void SetTextColor(Color color)
    {
        _text.FillColor = color;
        _caret.FillColor = color;
    }

    #endregion

    #region ===== Construction =====

    /// <summary>
    /// Creates a new <see cref="InputField"/>.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="border">9-slice borders.</param>
    /// <param name="sourceRect">Texture rect.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    public InputField(
        Texture panelTexture, Thickness border, IntRect sourceRect,
        Font font, System.UInt32 fontSize,
        Vector2f size, Vector2f position)
    {
        _fontSize = fontSize;

        _panel = new NineSlicePanel(panelTexture, border, sourceRect)
        {
            Position = position,
            Size = EnsureMinSize(size, border)
        };
        _panel.Layout();

        // (VN) _measure chỉ dùng đo kích thước/khoảng cách glyph → tránh xê dịch do bearings
        _measure = new Text(System.String.Empty, font, _fontSize)
        {
            FillColor = new Color(30, 30, 30)
        };

        _text = new Text(System.String.Empty, font, _fontSize)
        {
            FillColor = new Color(30, 30, 30)
        };

        RelayoutText();

        _caret = new RectangleShape(new Vector2f(_caretWidth, _fontSize))
        {
            FillColor = _text.FillColor
        };

        UpdateHitBox();
        UpdateCaretImmediate();

        // (VN) Cho UI nổi lên một chút; tùy engine của bạn
        SetZIndex(800);
    }

    #endregion

    #region ===== Engine Hooks =====

    /// <inheritdoc/>
    public override void Update(System.Single dt)
    {
        // (VN) Click chuột để focus/unfocus
        if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            var mp = InputState.GetMousePosition();
            Focused = _hitBox.Contains(mp.X, mp.Y);
        }

        if (Focused)
        {
            // Caret blink
            _caretTimer += dt;
            if (_caretTimer >= CaretBlinkPeriod)
            {
                _caretVisible = !_caretVisible;
                _caretTimer = 0f;
            }

            HandleTypingCore(dt);
        }

        // Cập nhật phần text hiển thị (scroll cửa sổ nhìn)
        UpdateVisibleWindowAndString();
        UpdateCaretImmediate();
    }

    /// <inheritdoc/>
    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_text);

        if (Focused && _caretVisible)
        {
            target.Draw(_caret);
        }
    }

    /// <summary>
    /// Not used by engine (we render explicitly in <see cref="Render"/>), but must be provided.
    /// </summary>
    protected override Drawable GetDrawable() => _text;

    #endregion

    #region ===== Typing & Caret =====

    /// <summary>
    /// Handles key presses and key repeats for Backspace/Delete.
    /// (VN) Xử lý gõ phím & giữ phím để repeat.
    /// </summary>
    private void HandleTypingCore(System.Single dt)
    {
        System.Boolean shift = InputState.IsKeyDown(Keyboard.Key.LShift) || InputState.IsKeyDown(Keyboard.Key.RShift);

        // Submit: Enter
        if (InputState.IsKeyPressed(Keyboard.Key.Enter))
        {
            OnSubmit?.Invoke(_buffer.ToString());
        }

        // Letters A..Z
        for (Keyboard.Key k = Keyboard.Key.A; k <= Keyboard.Key.Z; k++)
        {
            if (InputState.IsKeyPressed(k))
            {
                System.Char c = (System.Char)('a' + (k - Keyboard.Key.A));
                AppendChar(shift ? System.Char.ToUpperInvariant(c) : c);
            }
        }

        // Digits 0..9 (row numbers)
        for (Keyboard.Key k = Keyboard.Key.Num0; k <= Keyboard.Key.Num9; k++)
        {
            if (InputState.IsKeyPressed(k))
            {
                AppendChar((System.Char)('0' + (k - Keyboard.Key.Num0)));
            }
        }

        // Basic punctuation
        if (InputState.IsKeyPressed(Keyboard.Key.Space))
        {
            AppendChar(' ');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Period))
        {
            AppendChar('.');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Comma))
        {
            AppendChar(',');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Hyphen))
        {
            AppendChar('-');
        }

        if (InputState.IsKeyPressed(Keyboard.Key.Apostrophe))
        {
            AppendChar('\'');
        }

        // Backspace/Delete: edge + repeat
        System.Boolean bsDown = InputState.IsKeyDown(Keyboard.Key.Backspace);
        if (bsDown && !_prevBackspace)
        {
            BackspaceOne();
            _repeatBackspace = true;
            _repeatTimer = KeyRepeatFirstDelay;
        }
        if (_repeatBackspace && bsDown)
        {
            _repeatTimer -= dt;
            if (_repeatTimer <= 0f)
            {
                BackspaceOne();
                _repeatTimer = KeyRepeatNextDelay;
            }
        }
        if (!bsDown)
        {
            _repeatBackspace = false;
        }

        _prevBackspace = bsDown;

        System.Boolean delDown = InputState.IsKeyDown(Keyboard.Key.Delete);
        if (delDown && !_prevDelete)
        {
            // caret is always at the end => treat Delete same as Backspace
            BackspaceOne();
            _repeatDelete = true;
            _repeatTimer = KeyRepeatFirstDelay;
        }
        if (_repeatDelete && delDown)
        {
            _repeatTimer -= dt;
            if (_repeatTimer <= 0f)
            {
                BackspaceOne();
                _repeatTimer = KeyRepeatNextDelay;
            }
        }
        if (!delDown)
        {
            _repeatDelete = false;
        }

        _prevDelete = delDown;
    }

    /// <summary>
    /// Updates caret position/size immediately to the end of visible text.
    /// (VN) Cập nhật vị trí/size caret ngay lập tức về cuối chuỗi hiển thị.
    /// </summary>
    private void UpdateCaretImmediate()
    {
        // bounds.Left may be non-zero due to glyph bearings
        var b = _text.GetLocalBounds();
        System.Single caretX = _text.Position.X + b.Left + b.Width + 1f;
        System.Single caretY = _text.Position.Y + 2f;

        _caret.Size = new Vector2f(_caretWidth, _fontSize);
        _caret.Position = new Vector2f(caretX, caretY);
    }

    #endregion

    #region ===== Visible Window / Scrolling =====

    /// <summary>
    /// Computes the portion of <see cref="GetDisplayText"/> that fits into the inner width,
    /// ensuring the tail (caret at end) remains visible. Then assigns to <see cref="_text"/>.
    /// </summary>
    private void UpdateVisibleWindowAndString()
    {
        System.String full = GetDisplayText();
        _measure.DisplayedString = full;

        System.Single innerWidth = _panel.Size.X - (_padding.X * 2f) - _caretWidth; // leave space for caret

        // Helper: width of substring [i..n)
        System.Single Width(System.UInt32 i, System.UInt32 n)
            => _measure.FindCharacterPos(n).X - _measure.FindCharacterPos(i).X;

        System.UInt32 n = (System.UInt32)full.Length;
        if (n == 0)
        {
            _scrollStart = 0;
            _text.DisplayedString = System.String.Empty;
            ApplyTextPosition();
            return;
        }

        // If full fits, reset scroll
        if (Width(0, n) <= innerWidth)
        {
            _scrollStart = 0;
            _text.DisplayedString = full;
            ApplyTextPosition();
            return;
        }

        // Ensure tail is visible: advance start until [start..n) fits
        while (Width((System.UInt32)_scrollStart, n) > innerWidth && _scrollStart < full.Length)
        {
            _scrollStart++;
        }

        // After deletions, try reveal more head if space allows
        while (_scrollStart > 0 && Width((System.UInt32)(_scrollStart - 1), n) <= innerWidth)
        {
            _scrollStart--;
        }

        _text.DisplayedString = full[_scrollStart..];
        ApplyTextPosition();
    }

    /// <summary>
    /// Returns what should be displayed: placeholder, masked password, or raw text.
    /// (VN) Chuỗi hiển thị: placeholder (nếu rỗng & không focus), password (•••), hoặc text thường.
    /// </summary>
    protected virtual System.String GetDisplayText()
    {
        if (_buffer.Length == 0 && !Focused && !System.String.IsNullOrEmpty(Placeholder))
        {
            return Placeholder;
        }

        if (PasswordMode && _buffer.Length > 0)
        {
            // Use bullet U+2022
            return new System.String('\u2022', _buffer.Length);
        }

        return _buffer.ToString();
    }

    #endregion

    #region ===== Helpers =====

    /// <summary>
    /// Append a char with MaxLength enforcement and change notification.
    /// </summary>
    private void AppendChar(System.Char c)
    {
        if (MaxLength.HasValue && _buffer.Length >= MaxLength.Value)
        {
            return;
        }

        _ = _buffer.Append(c);
        OnChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>Remove one char at end, if any; raises <see cref="OnChanged"/>.</summary>
    private void BackspaceOne()
    {
        if (_buffer.Length == 0)
        {
            return;
        }

        _ = _buffer.Remove(_buffer.Length - 1, 1);
        OnChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>Clamp current text to <see cref="MaxLength"/> if needed.</summary>
    private void ClampToMaxLength()
    {
        if (MaxLength.HasValue && _buffer.Length > MaxLength.Value)
        {
            _buffer.Length = MaxLength.Value;
        }
    }

    /// <summary>Recompute hit-box based on panel position &amp; size.</summary>
    private void UpdateHitBox()
    {
        var p = _panel.Position;
        var s = _panel.Size;
        _hitBox = new FloatRect(p.X, p.Y, s.X, s.Y);
    }

    /// <summary>Ensure panel size never violates border minimums.</summary>
    private static Vector2f EnsureMinSize(Vector2f size, Thickness b)
    {
        System.Single minW = b.Left + b.Right + 1f;
        System.Single minH = b.Top + b.Bottom + 1f;
        return new Vector2f(System.MathF.Max(size.X, minW), System.MathF.Max(size.Y, minH));
    }

    /// <summary>Reposition both measure/draw texts from panel position and padding.</summary>
    private void RelayoutText() => ApplyTextPosition();

    private void ApplyTextPosition()
    {
        System.Single textY = _panel.Position.Y + ((_panel.Size.Y - _fontSize) / 2f) - 2f;
        System.Single textX = _panel.Position.X + _padding.X;

        _text.Position = new Vector2f(textX, textY);
        _measure.Position = _text.Position; // measure should share same anchor
    }

    /// <summary>Reset scrolling window and caret visibility after large layout changes.</summary>
    private void ResetScrollAndCaret()
    {
        _scrollStart = 0;
        _caretVisible = true;
        _caretTimer = 0f;
        UpdateVisibleWindowAndString();
        UpdateCaretImmediate();
    }

    #endregion
}
