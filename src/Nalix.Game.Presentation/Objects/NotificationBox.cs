using Nalix.Game.Presentation.Enums;
using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Presentation.Objects;

public class NotificationBox : RenderObject
{
    private Action _onAcceptClicked;
    private readonly Text _messageText;
    private readonly Sprite _acceptButtonSprite;
    private readonly NotificationBoxBackground _background;

    public NotificationBox(
        Vector2f position,
        Vector2f size,
        string initialMessage = "",
        Sprite acceptButtonSprite = null)
    {
        base.SetZIndex(ZIndex.Notification.ToInt()); // ưu tiên hiển thị trên cùng
        base.Reveal();

        Font font = Assets.Font.Load("1.ttf");

        // Background
        _background = new NotificationBoxBackground(position, size);

        // Text thông báo
        _messageText = new Text(initialMessage, font, 20)
        {
            FillColor = Color.White,
            Position = new Vector2f(position.X + 20, position.Y + 20),
            // Giới hạn chiều rộng (có thể tự xử lý wrap nếu cần)
        };

        // Nút đồng ý
        if (acceptButtonSprite != null)
        {
            _acceptButtonSprite = new Sprite(acceptButtonSprite.Texture)
            {
                Position = new Vector2f(position.X + size.X - acceptButtonSprite.Texture.Size.X - 20, position.Y + size.Y - acceptButtonSprite.Texture.Size.Y - 20),
                Scale = acceptButtonSprite.Scale,
            };
        }
        else
        {
            // Nếu không có sprite, tạo nút hình chữ nhật đơn giản
            _acceptButtonSprite = new Sprite
            {
                Texture = CreateSolidTexture((uint)100, (uint)40, new Color(0, 120, 0)),
                Position = new Vector2f(position.X + size.X - 120, position.Y + size.Y - 60)
            };
        }
    }

    /// <summary>
    /// Hiển thị thông báo mới với nội dung và callback khi nút đồng ý được nhấn.
    /// </summary>
    public void Show(string message, Action onAcceptClicked)
    {
        _messageText.DisplayedString = message;
        _onAcceptClicked = onAcceptClicked;
        base.Reveal();
    }

    public override void Update(float deltaTime)
    {
        if (!Visible) return;

        // Kiểm tra input chuột cho nút đồng ý
        if (InputState.IsMouseButtonPressed(Mouse.Button.Left))
        {
            Vector2i mousePos = InputState.GetMousePosition();
            if (_acceptButtonSprite.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
            {
                _onAcceptClicked?.Invoke();
                base.Conceal();
            }
        }
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        _background.Render(target);
        target.Draw(_messageText);
        target.Draw(_acceptButtonSprite);
    }

    protected override Drawable GetDrawable()
        => throw new NotSupportedException("Use Render() instead.");

    // Helper tạo texture đơn sắc cho nút nếu bạn không dùng sprite
    private static Texture CreateSolidTexture(uint width, uint height, Color color)
    {
        Image img = new(width, height, color);
        return new Texture(img);
    }

    /// <summary>
    /// Một đối tượng hiển thị nền cho hộp thông báo với các phần cắt (slices) texture.
    /// Sử dụng các sprite để tạo giao diện có thể mở rộng dựa trên kích thước và vị trí.
    /// </summary>
    private sealed class NotificationBoxBackground : RenderObject // Đánh dấu sealed để tránh thừa kế không cần thiết
    {
        private readonly Vector2f _size;
        private readonly Vector2f _position;
        private readonly Sprite[] _slices;

        /// <summary>
        /// Khởi tạo nền hộp thông báo với vị trí và kích thước được chỉ định.
        /// </summary>
        /// <param name="position">Vị trí của hộp thông báo.</param>
        /// <param name="size">Kích thước của hộp thông báo.</param>
        public NotificationBoxBackground(Vector2f position, Vector2f size)
        {
            _size = size;
            _position = position;

            base.SetZIndex((int)ZIndex.Notification);

            // Load textures theo layout
            Texture texTopLeft = Assets.UI.Load("dialog/1");
            Texture texBottomLeft = Assets.UI.Load("dialog/2");
            Texture texTopCenter = Assets.UI.Load("dialog/5");
            Texture texBottomCenter = Assets.UI.Load("dialog/6");
            Texture texTopRight = Assets.UI.Load("dialog/3");
            Texture texBottomRight = Assets.UI.Load("dialog/4");

            _slices = new Sprite[6];

            // Khởi tạo sprites
            _slices[0] = new Sprite(texTopLeft);
            _slices[1] = new Sprite(texTopCenter);
            _slices[2] = new Sprite(texTopRight);
            _slices[3] = new Sprite(texBottomLeft);
            _slices[4] = new Sprite(texBottomCenter);
            _slices[5] = new Sprite(texBottomRight);

            this.LayoutSprites(); // Đặt vị trí và kích thước cho các phần cắt
        }

        /// <summary>
        /// Vẽ các phần cắt của nền hộp thông báo lên mục tiêu hiển thị.
        /// </summary>
        /// <param name="target">Mục tiêu hiển thị (RenderTarget).</param>
        public override void Render(RenderTarget target)
        {
            // Sử dụng foreach với mảng để tăng hiệu suất so với List
            foreach (Sprite slice in _slices)
            {
                if (slice != null)
                    target.Draw(slice);
            }
        }

        /// <summary>
        /// Ném ngoại lệ vì không hỗ trợ lấy đối tượng Drawable trực tiếp.
        /// </summary>
        /// <returns>Không trả về giá trị, luôn ném ngoại lệ.</returns>
        /// <exception cref="NotSupportedException">Luôn được ném khi gọi phương thức này.</exception>
        protected override Drawable GetDrawable() => throw new NotSupportedException();

        private void LayoutSprites()
        {
            // Lấy kích thước texture từng phần
            Vector2f sizeTL = (Vector2f)_slices[0].Texture.Size;
            Vector2f sizeTC = (Vector2f)_slices[1].Texture.Size;
            Vector2f sizeTR = (Vector2f)_slices[2].Texture.Size;
            Vector2f sizeBL = (Vector2f)_slices[3].Texture.Size;
            Vector2f sizeBC = (Vector2f)_slices[4].Texture.Size;
            Vector2f sizeBR = (Vector2f)_slices[5].Texture.Size;

            float x = _position.X;
            float y = _position.Y;
            float w = _size.X;
            float h = _size.Y;

            // --- Clamp kích thước tối thiểu để tránh scale âm ---
            float minWidthTop = sizeTL.X + sizeTR.X + 1f;
            float minWidthBottom = sizeBL.X + sizeBR.X + 1f;
            float minHeight = Math.Max(sizeTL.Y, sizeTR.Y) + Math.Max(sizeBL.Y, sizeBR.Y) + 1f;

            if (w < minWidthTop || w < minWidthBottom)
            {
                float requiredWidth = Math.Max(minWidthTop, minWidthBottom);
                Console.WriteLine($"⚠️ Width too small: {w}, clamped to {requiredWidth}");
                w = requiredWidth;
            }

            if (h < minHeight)
            {
                Console.WriteLine($"⚠️ Height too small: {h}, clamped to {minHeight}");
                h = minHeight;
            }

            // 1. Top-Left
            _slices[0].Position = new Vector2f(x, y);

            // 2. Top-Center (scale ngang)
            float topCenterWidth = Math.Max(1f, w - sizeTL.X - sizeTR.X);
            float scaleXTC = topCenterWidth / sizeTC.X;
            _slices[1].Position = new Vector2f(x + sizeTL.X, y);
            _slices[1].Scale = new Vector2f(scaleXTC, 1f);

            // 3. Top-Right
            _slices[2].Position = new Vector2f(x + w - sizeTR.X, y);

            // 4. Bottom-Left
            _slices[3].Position = new Vector2f(x, y + h - sizeBL.Y);

            // 5. Bottom-Center (scale ngang)
            float bottomCenterWidth = Math.Max(1f, w - sizeBL.X - sizeBR.X);
            float scaleXBC = bottomCenterWidth / sizeBC.X;
            _slices[4].Position = new Vector2f(x + sizeBL.X, y + h - sizeBC.Y);
            _slices[4].Scale = new Vector2f(scaleXBC, 1f);

            // 6. Bottom-Right
            _slices[5].Position = new Vector2f(x + w - sizeBR.X, y + h - sizeBR.Y);

            // Debug output
            Console.WriteLine($"✅ Layout OK - Position: ({x}, {y}), Size: ({w}, {h})");
            Console.WriteLine($"TopCenter ScaleX: {scaleXTC}, BottomCenter ScaleX: {scaleXBC}");
        }
    }
}