using Nalix.Portal.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Portal.Objects.Notifications;

/// <summary>
/// Hộp thông báo nhẹ (không có nút bấm). Vẽ panel 9-slice với văn bản tự động xuống dòng.
/// </summary>
[IgnoredLoad("RenderObject")]
public class Notification : RenderObject
{
    #region Constants

    /// <summary>Kích thước ký tự mặc định của văn bản (px).</summary>
    protected const System.Single TextCharSizePx = 20f;

    /// <summary>Khoảng đệm ngang (px).</summary>
    protected const System.Single HorizontalPaddingPx = 12f;

    /// <summary>Khoảng đệm dọc (px).</summary>
    protected const System.Single VerticalPaddingPx = 30f;

    /// <summary>Tỷ lệ Y khi đặt ở phía trên màn hình.</summary>
    private const System.Single TopYRatio = 0.10f;

    /// <summary>Tỷ lệ Y khi đặt ở phía dưới màn hình.</summary>
    private const System.Single BottomYRatio = 0.70f;

    /// <summary>Tỷ lệ chiều rộng tối đa so với màn hình.</summary>
    private const System.Single MaxWidthFraction = 0.85f;

    /// <summary>Ngưỡng trần chiều rộng (px).</summary>
    private const System.Single MaxWidthCapPx = 720f;

    /// <summary>Chiều cao panel khởi tạo (px).</summary>
    private const System.Single InitialPanelHeightPx = 64f;

    /// <summary>Chiều cao panel tối thiểu (px).</summary>
    private const System.Single MinPanelHeightPx = 162f;

    /// <summary>Chiều rộng bên trong tối thiểu (px).</summary>
    private const System.Single MinInnerWidthPx = 50f;

    #endregion

    #region Fields

    protected readonly Text _messageText;
    protected readonly NineSlicePanel _panel;
    protected readonly Thickness _border = new(32);

    protected Vector2f _textAnchor;

    #endregion

    #region Ctors

    /// <summary>
    /// Khởi tạo một hộp thông báo chỉ có văn bản.
    /// </summary>
    /// <param name="initialMessage">Thông điệp ban đầu.</param>
    /// <param name="side">Vị trí hiển thị (Trên/Dưới).</param>
    public Notification(System.String initialMessage = "", Side side = Side.Top)
    {
        LoadAssets(out var font, out var frameTex);

        ComputeBasicLayout(side, out System.Single floatY, out System.Single targetWidth, out System.Single xCentered);

        _panel = CreatePanel(frameTex, xCentered, floatY, targetWidth);

        System.Single innerWidth = ComputeInnerWidth(targetWidth);
        _messageText = PrepareWrappedText(font, initialMessage, (System.UInt32)TextCharSizePx, innerWidth);

        System.Single textHeight = CenterTextOriginAndMeasure(_messageText);
        System.Single targetHeight = ComputeTargetHeight(textHeight);

        ResizeAndLayoutPanel(_panel, targetWidth, targetHeight);
        PositionTextInsidePanel(_panel, textHeight, out _textAnchor);

        RevealAndOrder();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Cập nhật thông điệp và giữ nguyên anchor point.
    /// </summary>
    public virtual void UpdateMessage(System.String newMessage)
    {
        System.Single innerWidth = ComputeInnerWidth(_panel.Size.X);

        System.String wrapped = WrapText(_messageText.Font, newMessage, _messageText.CharacterSize, innerWidth);
        _messageText.DisplayedString = wrapped;

        // Re-center origin cho bounds mới nhưng vẫn giữ anchor.
        var lb = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(lb.Left + lb.Width / 2f, lb.Top + lb.Height / 2f);
        _messageText.Position = _textAnchor;
    }

    #endregion

    #region Update/Render

    public override void Update(System.Single deltaTime)
    {
        if (!Visible)
        {
            return;
        }
        // Base notification: không có state update
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_messageText);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout Construction

    private static void LoadAssets(out Font font, out Texture frameTex)
    {
        font = Assets.Font.Load("1");
        frameTex = Assets.UiTextures.Load("transparent_center/010");
        frameTex.Smooth = false;
    }

    private static void ComputeBasicLayout(
        Side side,
        out System.Single floatY,
        out System.Single targetWidth,
        out System.Single xCentered)
    {
        System.Single ratio = side == Side.Bottom ? BottomYRatio : TopYRatio;
        System.Single screenW = GraphicsEngine.ScreenSize.X;

        System.Single rawWidth = screenW * MaxWidthFraction;
        targetWidth = System.MathF.Round(System.MathF.Min(rawWidth, MaxWidthCapPx));

        xCentered = System.MathF.Round((screenW - targetWidth) / 2f);
        floatY = GraphicsEngine.ScreenSize.Y * ratio;
    }

    private NineSlicePanel CreatePanel(Texture frameTex, System.Single x, System.Single y, System.Single width)
    {
        var p = new NineSlicePanel(frameTex, _border)
            .SetPosition(new Vector2f(x, y))
            .SetSize(new Vector2f(width, InitialPanelHeightPx));

        p.Layout();
        return p;
    }

    private static System.Single ComputeInnerWidth(System.Single targetWidth)
        => System.MathF.Max(MinInnerWidthPx, targetWidth - 2f * HorizontalPaddingPx);

    private static Text PrepareWrappedText(Font font, System.String message, System.UInt32 charSize, System.Single innerWidth)
        => new(WrapText(font, message, charSize, innerWidth), font, charSize) { FillColor = Color.Black };

    private static System.Single CenterTextOriginAndMeasure(Text text)
    {
        FloatRect lb = text.GetLocalBounds();
        text.Origin = new Vector2f(lb.Left + lb.Width / 2f, lb.Top + lb.Height / 2f);
        return text.GetGlobalBounds().Height;
    }

    private static System.Single ComputeTargetHeight(System.Single textHeight)
    {
        System.Single h = VerticalPaddingPx + textHeight + VerticalPaddingPx;
        return System.MathF.Max(MinPanelHeightPx, System.MathF.Round(h));
    }

    private static void ResizeAndLayoutPanel(NineSlicePanel panel, System.Single width, System.Single height)
    {
        _ = panel.SetSize(new Vector2f(width, height));
        panel.Layout();
    }

    private void PositionTextInsidePanel(NineSlicePanel panel, System.Single textHeight, out Vector2f anchorOut)
    {
        System.Single innerLeft = System.MathF.Round(panel.Position.X + _border.Left + HorizontalPaddingPx);
        System.Single innerRight = System.MathF.Round(panel.Position.X + panel.Size.X - _border.Right - HorizontalPaddingPx);
        System.Single innerCenterX = System.MathF.Round((innerLeft + innerRight) * 0.5f);
        System.Single innerTop = System.MathF.Round(panel.Position.Y + _border.Top + VerticalPaddingPx);

        _messageText.Position = new Vector2f(innerCenterX, innerTop + textHeight * 0.5f);
        anchorOut = _messageText.Position;
    }

    private void RevealAndOrder()
    {
        Reveal();
        SetZIndex(ZIndex.Notification.ToInt());
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Helper word-wrap: chia văn bản thành nhiều dòng dựa trên maxWidth.
    /// Tái sử dụng 1 instance <see cref="Text"/> để đo, tránh cấp phát thừa.
    /// </summary>
    protected static System.String WrapText(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
    {
        if (System.String.IsNullOrEmpty(text))
        {
            return System.String.Empty;
        }

        System.String result = "";
        System.String currentLine = "";
        System.String[] words = text.Split(' ');

        var measurer = new Text("", font, characterSize);

        for (System.Int32 i = 0; i < words.Length; i++)
        {
            System.String word = words[i];
            System.String testLine = currentLine.Length > 0 ? currentLine + " " + word : word;

            measurer.DisplayedString = testLine;
            if (measurer.GetLocalBounds().Width > maxWidth)
            {
                if (currentLine.Length > 0)
                {
                    result += currentLine + "\n";
                    currentLine = word;
                }
                else
                {
                    // Trường hợp từ đơn dài hơn maxWidth → buộc xuống dòng
                    result += word + "\n";
                    currentLine = System.String.Empty;
                }
            }
            else
            {
                currentLine = testLine;
            }
        }

        result += currentLine;
        return result;
    }

    /// <summary>
    /// Helper nội suy màu.
    /// </summary>
    protected static Color Lerp(Color a, Color b, System.Single t)
    {
        System.Byte L(System.Byte x, System.Byte y) => (System.Byte)(x + (y - x) * t);
        return new Color(L(a.R, b.R), L(a.G, b.G), L(a.B, b.B), L(a.A, b.A));
    }

    #endregion
}
