using Nalix.Launcher.Enums;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Input;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Launcher.Objects.Notifications;

/// <summary>
/// Hộp thông báo có nút hành động ở giữa.
/// Hỗ trợ hiệu ứng hover/click và tự động ẩn khi click.
/// </summary>
public sealed class ActionNotification : Notification
{
    #region Constants

    /// <summary>Khoảng cách dọc giữa text và button (px).</summary>
    private const System.Single VerticalGapPx = 6f;

    /// <summary>Kích thước chữ trên button (px).</summary>
    private const System.Single ButtonTextSizePx = 18f;

    /// <summary>Padding dọc của button (px).</summary>
    private const System.Single ButtonPadYPx = 6f;

    /// <summary>Scale style của button.</summary>
    private const System.Single ButtonScaleRatio = 0.5f;

    /// <summary>Chiều cao button tối thiểu (px).</summary>
    private const System.Single MinButtonHeightPx = 28f;

    /// <summary>Thời gian fade in (giây).</summary>
    private const System.Single HoverFadeInSec = 0.08f;

    /// <summary>Thời gian fade out (giây).</summary>
    private const System.Single HoverFadeOutSec = 0.10f;

    /// <summary>Chiều rộng nội bộ tối thiểu (px).</summary>
    private const System.Single MinInnerWidthPx = 50f;

    #endregion

    #region Fields

    private readonly NineSlicePanel _buttonPanel;
    private readonly Text _buttonText;

    private System.Boolean _isHovering;
    private System.Single _hoverAnim;

    private event System.Action OnClicked;

    // Visual colors
    private readonly Color _baseGray = new(220, 220, 220, 255);
    private readonly Color _hoverGray = new(120, 120, 120, 255);

    #endregion

    #region Ctors

    /// <summary>
    /// Khởi tạo một hộp thông báo có nút hành động ở giữa.
    /// </summary>
    public ActionNotification(System.String initialMessage = "", Side side = Side.Bottom, System.String buttonText = "OK")
        : base(initialMessage, side)
    {
        _buttonText = CreateButtonText(buttonText);
        _buttonPanel = CreateButtonPanel();
        SizeButtonToContent();
        PositionButton();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Offset Y thêm được áp dụng cho button sau khi layout.
    /// </summary>
    public System.Single ButtonExtraOffsetY { get; set; }

    /// <summary>
    /// Đăng ký action callback khi click button.
    /// </summary>
    public void RegisterAction(System.Action handler) => OnClicked += handler;

    /// <summary>
    /// Hủy đăng ký action callback.
    /// </summary>
    public void UnregisterAction(System.Action handler) => OnClicked -= handler;

    #endregion

    #region Internal Logic

    public override void UpdateMessage(System.String newMessage)
    {
        base.UpdateMessage(newMessage);
        PositionButton();
    }

    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        UpdateHoverState(deltaTime);
        UpdateButtonVisuals();
        HandleClick();
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        base.Render(target);
        target.Draw(_buttonPanel);
        target.Draw(_buttonText);
    }

    #endregion

    #region Layout

    private Text CreateButtonText(System.String caption)
    {
        var text = new Text(caption, _messageText.Font, (System.UInt32)ButtonTextSizePx)
        {
            FillColor = Color.Black
        };

        // Căn giữa origin để dễ đặt vào panel
        var lb = text.GetLocalBounds();
        text.Origin = new Vector2f(lb.Left + lb.Width / 2f, lb.Top + lb.Height / 2f - 10);
        return text;
    }

    private NineSlicePanel CreateButtonPanel()
    {
        Texture frame = Assets.UiTextures.Load("transparent_border/001");
        frame.Smooth = false;

        NineSlicePanel panel = new(frame, _border);
        panel.SetPosition(new Vector2f(0f, 0f))
             .SetSize(new Vector2f(200f, 64f))
             .Layout();

        return panel;
    }

    private (System.Single innerCenterX, System.Single innerWidth) ComputeInnerMetrics()
    {
        System.Single innerLeft = System.MathF.Round(_panel.Position.X + _border.Left + HorizontalPaddingPx);
        System.Single innerRight = System.MathF.Round(_panel.Position.X + _panel.Size.X - _border.Right - HorizontalPaddingPx);
        System.Single innerCenterX = System.MathF.Round((innerLeft + innerRight) / 2f);

        System.Single innerWidth = _panel.Size.X - (_border.Left + _border.Right) - (HorizontalPaddingPx * 2f);
        innerWidth = System.MathF.Max(innerWidth, MinInnerWidthPx);

        return (innerCenterX, innerWidth);
    }

    private void SizeButtonToContent()
    {
        var (_, innerWidth) = ComputeInnerMetrics();
        var lb = _buttonText.GetLocalBounds();

        System.Single targetW = System.MathF.Round(innerWidth);
        System.Single targetH = System.MathF.Round(lb.Height + (ButtonPadYPx * 2f) + _border.Top + _border.Bottom);
        targetH = System.MathF.Max(targetH, MinButtonHeightPx);

        _buttonPanel.SetSize(new Vector2f(targetW * (ButtonScaleRatio - 0.1f), targetH * ButtonScaleRatio))
                    .Layout();
    }

    private void PositionButton()
    {
        var (innerCenterX, _) = ComputeInnerMetrics();
        var textGB = _messageText.GetGlobalBounds();

        _messageText.Position = new Vector2f(_messageText.Position.X, _messageText.Position.Y - 20);
        System.Single buttonY = System.MathF.Round(textGB.Top + textGB.Height + VerticalGapPx) - 200;
        System.Single btnX = System.MathF.Round(innerCenterX - _buttonPanel.Size.X / 2f);

        _buttonPanel.SetPosition(new Vector2f(btnX, buttonY)).Layout();

        // Căn text vào giữa panel
        System.Single btnCenterX = System.MathF.Round(btnX + (_buttonPanel.Size.X / 2f));
        System.Single btnCenterY = System.MathF.Round(buttonY + (_buttonPanel.Size.Y / 2f));
        _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
    }

    #endregion

    #region Interaction

    private FloatRect GetButtonRect()
        => new(_buttonPanel.Position.X, _buttonPanel.Position.Y,
               _buttonPanel.Size.X, _buttonPanel.Size.Y);

    private void UpdateHoverState(System.Single dt)
    {
        Vector2i mouse = InputState.GetMousePosition();
        System.Boolean hover = GetButtonRect().Contains(mouse.X, mouse.Y);

        if (hover)
        {
            _isHovering = true;
            _hoverAnim += dt / HoverFadeInSec;
        }
        else
        {
            _isHovering = false;
            _hoverAnim -= dt / HoverFadeOutSec;
        }

        _hoverAnim = System.Math.Clamp(_hoverAnim, 0f, 1f);
    }

    private void UpdateButtonVisuals()
    {
        _ = _buttonPanel.SetColor(Lerp(_baseGray, _hoverGray, _hoverAnim));
        _buttonText.FillColor = Lerp(Color.Black, Color.White, _hoverAnim);
    }

    private void HandleClick()
    {
        if (_isHovering && InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            OnClicked?.Invoke();
            Conceal();
        }
    }

    #endregion

    #region Helpers

    private static new Color Lerp(Color a, Color b, System.Single t)
    {
        t = System.Math.Clamp(t, 0f, 1f);

        System.Byte LerpB(System.Byte x, System.Byte y) => (System.Byte)(x + ((y - x) * t));
        return new Color(
            LerpB(a.R, b.R),
            LerpB(a.G, b.G),
            LerpB(a.B, b.B),
            LerpB(a.A, b.A));
    }

    #endregion
}