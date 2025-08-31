using Nalix.Rendering.Objects;
using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>
/// Vận tốc/ trọng lực đơn giản cho SpriteObject/ColliderObject.
/// </summary>
public class RigidBody2D(ColliderObject owner) : SceneObject
{
    public Vector2f Velocity;
    public System.Single Gravity = 0f;      // ví dụ platformer: 980f;
    public System.Single Drag = 0f;         // cản (0..1)

    private readonly ColliderObject _owner = owner;

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Update(System.Single dt)
    {
        if (_owner == null || _owner.Paused || !_owner.Enabled)
        {
            return;
        }

        Velocity.Y += Gravity * dt;
        if (Drag > 0f)
        {
            Velocity *= 1f - System.Math.Clamp(Drag * dt, 0f, 0.99f);
        }

        // áp vị trí lên sprite
        var p = _owner.Sprite.Position;
        p += Velocity * dt;
        _owner.Sprite.Position = p;
    }
}
