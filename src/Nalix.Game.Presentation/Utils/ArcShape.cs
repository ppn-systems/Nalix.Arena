using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Game.Presentation.Utils;

/// <summary>
/// Đại diện cho một hình cung (arc) có thể vẽ bằng SFML.
/// Hình cung được xác định bởi bán kính, góc bắt đầu, góc kết thúc và số lượng điểm để xấp xỉ.
/// </summary>
public class ArcShape : Drawable
{
    /// <summary>
    /// Mảng đỉnh dùng để lưu trữ các điểm xác định hình cung.
    /// Sử dụng kiểu nguyên thủy TriangleFan để vẽ hình cung đầy.
    /// </summary>
    private readonly VertexArray _vertices;

    /// <summary>
    /// Lấy hoặc đặt vị trí tâm của hình cung trong không gian 2D.
    /// </summary>
    public Vector2f Position { get; set; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp <see cref="ArcShape"/>.
    /// </summary>
    /// <param name="radius">Bán kính của hình cung.</param>
    /// <param name="startAngle">Góc bắt đầu của hình cung (tính bằng độ).</param>
    /// <param name="endAngle">Góc kết thúc của hình cung (tính bằng độ).</param>
    /// <param name="pointCount">Số lượng điểm dùng để xấp xỉ hình cung.</param>
    /// <param name="color">Màu sắc của hình cung.</param>
    public ArcShape(float radius, float startAngle, float endAngle, int pointCount, Color color)
    {
        _vertices = new VertexArray(PrimitiveType.TriangleFan, (uint)(pointCount + 2));
        _vertices[0] = new Vertex(new Vector2f(0, 0), color);

        float angleStep = (endAngle - startAngle) / pointCount;
        for (int i = 0; i <= pointCount; i++)
        {
            float angle = (float)(startAngle + (i * angleStep)) * (float)(Math.PI / 180f);
            float x = radius * (float)Math.Cos(angle);
            float y = radius * (float)Math.Sin(angle);
            _vertices[(uint)(i + 1)] = new Vertex(new Vector2f(x, y), color);
        }
    }

    /// <summary>
    /// Vẽ hình cung lên một mục tiêu hiển thị (RenderTarget) với các trạng thái hiển thị được chỉ định.
    /// </summary>
    /// <param name="target">Mục tiêu hiển thị để vẽ hình cung (ví dụ: cửa sổ hoặc texture).</param>
    /// <param name="states">Trạng thái hiển thị, bao gồm các biến đổi như dịch chuyển hoặc xoay.</param>
    public void Draw(RenderTarget target, RenderStates states)
    {
        states.Transform.Translate(Position);
        target.Draw(_vertices, states);
    }
}