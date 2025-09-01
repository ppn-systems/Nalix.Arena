namespace Nalix.Rendering.Effects.Transitions.Abstractions;

/// <summary>
/// Các kiểu hiệu ứng phủ khi chuyển cảnh.
/// </summary>
public enum TransitionStyle : System.Byte
{
    Fade,               // mờ dần
    WipeHorizontal,     // phủ từ trái -> phải (rồi mở ra)
    WipeVertical,       // phủ từ trên -> dưới (rồi mở ra)
    SlideCoverLeft,     // tấm phủ trượt từ trái vào rồi trượt ra
    SlideCoverRight,    // tấm phủ trượt từ phải vào rồi trượt ra
    ZoomIn,             // khung zoom từ nhỏ -> đầy màn, rồi thu lại
    ZoomOut             // khung zoom từ lớn -> 0, rồi phóng lại
}
