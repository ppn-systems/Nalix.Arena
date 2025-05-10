namespace Nalix.Game.Domain.Models.Maps;

public enum NpcState : byte
{
    Idle = 0,           // Đứng yên
    Sleeping = 1,       // Không hoạt động
    Hidden = 2,         // Không hiển thị trên map
}