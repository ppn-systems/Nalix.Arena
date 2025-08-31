namespace Nalix.Rendering.Physics;

[System.Flags]
public enum CollisionLayer : System.UInt32
{
    None = 0,
    Default = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    Ground = 1 << 3,
    Pickup = 1 << 4,
    All = 0xFFFF_FFFF
}
