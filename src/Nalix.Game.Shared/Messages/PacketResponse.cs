namespace Nalix.Game.Shared.Messages;

public struct PacketResponse<T>
{
    public T Data;                      // Dữ liệu trả về (nếu có, generic)
    public ResponseCode Code;           // Mã trạng thái
    public System.String Message;       // Thông báo (thành công/lỗi)
}