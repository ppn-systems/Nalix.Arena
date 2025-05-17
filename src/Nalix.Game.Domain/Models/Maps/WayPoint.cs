namespace Nalix.Game.Domain.Models.Maps;

public sealed class WayPoint
{
    public short MinX { get; set; }        // Vị trí min X (trái)
    public short MinY { get; set; }        // Vị trí min Y (trên)
    public short MaxX { get; set; }        // Vị trí max X (phải)
    public short MaxY { get; set; }        // Vị trí max Y (dưới)

    public bool IsEnterable { get; set; }  // Có thể vào được không?

    public string Name { get; set; }       // Tên waypoint (dùng trong game, UI, hoặc để định danh)
    public short NextMapId { get; set; }   // ID bản đồ tiếp theo khi đi qua waypoint này (có thể là teleport)

    public WayPoint()
    {
        // Khởi tạo các giá trị mặc định nếu cần
        NextMapId = -1;
        Name = string.Empty;
        IsEnterable = true;
    }
}