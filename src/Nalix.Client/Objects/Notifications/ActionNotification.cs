using Nalix.Client.Enums;
using Nalix.Rendering.Effects.Parallax;
using Nalix.Rendering.Input;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Client.Objects.Notifications;

/// <summary>
/// Notification box with an action button. Adds hover/click feedback and conceal on click.
/// </summary>
public sealed class ActionNotification : Notification
{
    private readonly NineSlicePanel _buttonPanel;
    private readonly Text _buttonText;

    private Boolean _isHovering;
    private Single _hoverAnim;

    private event Action OnClicked;

    /// <summary>
    /// Gets or sets an extra Y offset applied to the button after layout.
    /// Useful when you want to push the button a bit lower without affecting the text.
    /// </summary>
    public Single ButtonExtraOffsetY { get; set; } = 0f;

    private readonly Color _baseGray = new(220, 220, 220, 255);
    private readonly Color _hoverGray = new(120, 120, 120, 255);

    private const Single VerticalGap = 6f;

    /// <summary>
    /// Initializes a notification box with a centered action button.
    /// </summary>
    /// <param name="initialMessage">Initial message.</param>
    /// <param name="side">Top/Bottom placement.</param>
    /// <param name="buttonText">Caption of the button.</param>
    public ActionNotification(
        String initialMessage = "", Side side = Side.Bottom, String buttonText = "OK")
        : base(initialMessage, side)
    {
        // Build button content AFTER base has measured text/panel.
        Texture frameBut = Assets.UiTextures.Load("transparent_border/001");
        frameBut.Smooth = false;

        _buttonText = new Text(buttonText, _messageText.Font, 18) { FillColor = Color.Black };
        var lbBtn = _buttonText.GetLocalBounds();
        _buttonText.Origin = new Vector2f(lbBtn.Left + (lbBtn.Width / 2f), lbBtn.Top + (lbBtn.Height / 2f));

        // Button dimensions
        const Single btnPadY = 6f;
        const Single scale = 0.5f;

        Single innerLeft = MathF.Round(_panel.Position.X + _border.Left + HorizontalPadding);
        Single innerRight = MathF.Round(_panel.Position.X + _panel.Size.X - _border.Right - HorizontalPadding);
        Single innerCenterX = MathF.Round((innerLeft + innerRight) / 2f);

        Single innerWidth = _panel.Size.X - (_border.Left + _border.Right) - (HorizontalPadding * 2f);
        if (innerWidth < 50f)
        {
            innerWidth = 50f;
        }

        Single btnTargetWidth = MathF.Round(innerWidth);
        Single btnTargetHeight = MathF.Round(lbBtn.Height + (btnPadY * 2f) + _border.Top + _border.Bottom);
        btnTargetHeight = MathF.Max(btnTargetHeight, 28f);

        _buttonPanel = new NineSlicePanel(frameBut, _border)
        {
            Position = new Vector2f(0f, 0f),
            Size = new Vector2f(btnTargetWidth * (scale - 0.1f), btnTargetHeight * scale)
        };
        _buttonPanel.Layout();

        // Position the button below the text
        var textGB = _messageText.GetGlobalBounds();
        Single buttonY = MathF.Round(textGB.Top + textGB.Height + VerticalGap) + ButtonExtraOffsetY;

        Single btnX = MathF.Round(innerCenterX - (_buttonPanel.Size.X / 2f));
        _buttonPanel.Position = new Vector2f(btnX, buttonY);
        _buttonPanel.Layout();

        Single btnCenterX = MathF.Round(btnX + (_buttonPanel.Size.X / 2f));
        Single btnCenterY = MathF.Round(buttonY + (_buttonPanel.Size.Y / 2f));
        _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
    }

    /// <inheritdoc />
    public override void UpdateMessage(String newMessage)
    {
        base.UpdateMessage(newMessage);

        // Reposition button under the (possibly taller) text
        Single innerLeft = MathF.Round(_panel.Position.X + _border.Left + HorizontalPadding);
        Single innerRight = MathF.Round(_panel.Position.X + _panel.Size.X - _border.Right - HorizontalPadding);
        Single innerCenterX = MathF.Round((innerLeft + innerRight) / 2f);

        var textGB = _messageText.GetGlobalBounds();
        Single buttonY = MathF.Round(textGB.Top + textGB.Height + VerticalGap) + ButtonExtraOffsetY;

        Single btnX = MathF.Round(innerCenterX - (_buttonPanel.Size.X / 2f));
        _buttonPanel.Position = new Vector2f(btnX, buttonY);
        _buttonPanel.Layout();

        Single btnCenterX = MathF.Round(btnX + (_buttonPanel.Size.X / 2f));
        Single btnCenterY = MathF.Round(buttonY + (_buttonPanel.Size.Y / 2f));
        _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
    }

    /// <inheritdoc />
    public override void Update(Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }

        // Hover test
        var hoverRect = new FloatRect(
            _buttonPanel.Position.X,
            _buttonPanel.Position.Y,
            _buttonPanel.Size.X,
            _buttonPanel.Size.Y
        );

        Vector2i mouse = InputState.GetMousePosition();
        Boolean hover = hoverRect.Contains(mouse.X, mouse.Y);

        const Single fadeIn = 0.08f, fadeOut = 0.10f;
        if (hover)
        {
            _isHovering = true;
            _hoverAnim += deltaTime / fadeIn;
        }
        else
        {
            _isHovering = false;
            _hoverAnim -= deltaTime / fadeOut;
        }
        _hoverAnim = Math.Clamp(_hoverAnim, 0f, 1f);

        // Visual feedback
        _buttonPanel.SetColor(Lerp(_baseGray, _hoverGray, _hoverAnim));
        _buttonText.FillColor = Lerp(Color.Black, Color.White, _hoverAnim);

        // Click -> conceal
        if (_isHovering && InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            OnClicked?.Invoke();
            Conceal();
        }
    }

    /// <inheritdoc />
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

    public void RegisterAction(Action handler) => OnClicked += handler;
    public void UnregisterAction(Action handler) => OnClicked -= handler;
}
