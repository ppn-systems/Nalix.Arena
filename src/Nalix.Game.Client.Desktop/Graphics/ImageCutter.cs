using SFML.Graphics;
using System.Collections.Generic;

namespace Nalix.Game.Client.Desktop.Graphics;

public class ImageCutter(Texture texture, int iconWidth, int iconHeight)
{
    private readonly Texture _texture = texture;  // Texture lớn chứa nhiều icon
    private readonly int _iconWidth = iconWidth;    // Chiều rộng của mỗi icon
    private readonly int _iconHeight = iconHeight;   // Chiều cao của mỗi icon

    public Texture Texture => _texture;

    /// <summary>
    /// Cắt tất cả các icon từ ảnh lớn và trả về một danh sách các sprite.
    /// </summary>
    public List<Sprite> CutAllIcons(int iconsPerRow, int iconsPerColumn)
    {
        List<Sprite> icons = [];

        // Vòng lặp để cắt tất cả các icon từ ảnh lớn
        for (int row = 0; row < iconsPerColumn; row++)
        {
            for (int col = 0; col < iconsPerRow; col++)
            {
                // Tính toán vị trí của icon trong ảnh lớn
                int x = col * _iconWidth;
                int y = row * _iconHeight;

                // Tạo sprite cho icon tại vị trí (x, y)
                Sprite iconSprite = CreateIcon(x, y);
                icons.Add(iconSprite);
            }
        }

        return icons;
    }

    public Sprite CutIconAt(int index, int iconsPerRow)
    {
        int row = index / iconsPerRow;
        int col = index % iconsPerRow;
        return CreateIcon(col * _iconWidth, row * _iconHeight);
    }

    public IntRect GetRectAt(int column, int row)
        => new(column * _iconWidth, row * _iconHeight, _iconWidth, _iconHeight);

    /// <summary>
    /// Tạo một sprite từ một phần nhỏ của texture (icon).
    /// </summary>
    private Sprite CreateIcon(int x, int y)
    {
        // Cắt một phần của texture dựa trên vị trí (x, y) và kích thước icon
        IntRect iconRect = new(x, y, _iconWidth, _iconHeight);
        Sprite iconSprite = new(_texture, iconRect);

        return iconSprite;
    }
}