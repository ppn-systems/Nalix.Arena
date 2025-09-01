using Nalix.Rendering.Objects;
using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>
/// Lightweight 2D rigid body for <see cref="ColliderObject"/> with velocity, gravity and linear drag.
/// </summary>
/// <remarks>
/// <para>
/// (VN) RigidBody đơn giản: có <b>vận tốc</b>, <b>trọng lực</b> và <b>cản tuyến tính</b>. 
/// Mỗi khung: cộng gia tốc trọng lực vào <see cref="Velocity"/>, áp dụng <see cref="Drag"/>, rồi dịch chuyển đối tượng.
/// </para>
/// <para>
/// Đơn vị mặc định: pixel &amp; giây. Ví dụ platformer: <see cref="Gravity"/> ≈ <c>980f</c>.
/// </para>
/// </remarks>
/// <remarks>
/// Creates a new rigid body bound to a <see cref="ColliderObject"/>.
/// </remarks>
/// <param name="owner">The collider object to move each frame.</param>
/// <exception cref="System.ArgumentNullException"></exception>
public class RigidBody2D(ColliderObject owner) : SceneObject
{
    #region ===== State =====

    /// <summary>
    /// Current velocity in world units per second.
    /// </summary>
    public Vector2f Velocity;

    /// <summary>
    /// Per-second gravity acceleration (downward, +Y). Example: <c>980f</c> for platformers.
    /// </summary>
    public System.Single Gravity = 0f;

    /// <summary>
    /// Linear drag factor per second (0..1). Applied as: <c>Velocity *= (1 - clamp(Drag * dt, 0, 0.99))</c>.
    /// </summary>
    public System.Single Drag = 0f;

    #endregion

    #region ===== Options =====

    /// <summary>
    /// Optional speed cap (units/s). Set to <c>null</c> for unlimited.
    /// </summary>
    public System.Single? MaxSpeed { get; set; }

    /// <summary>
    /// Freeze movement on X axis (useful for ladders, constraints, cutscenes).
    /// </summary>
    public System.Boolean FreezeX { get; set; }

    /// <summary>
    /// Freeze movement on Y axis.
    /// </summary>
    public System.Boolean FreezeY { get; set; }

    #endregion

    #region ===== Owner =====


    /// <summary>
    /// The collider owner driven by this rigid body.
    /// </summary>
    public ColliderObject Owner { get; } = owner ?? throw new System.ArgumentNullException(nameof(owner));

    #endregion

    #region ===== Public API (helpers) =====

    /// <summary>
    /// Adds a continuous force over time (i.e., modifies velocity by <paramref name="accel"/> * dt).
    /// </summary>
    /// <remarks>(VN) Lực dạng gia tốc: gọi mỗi khung để “đẩy” từ từ.</remarks>
    public void ApplyForce(Vector2f accel, System.Single dt)
        => Velocity += accel * dt;

    /// <summary>
    /// Adds an instantaneous velocity change (impulse).
    /// </summary>
    /// <remarks>(VN) Xung vận tốc: cộng trực tiếp vào <see cref="Velocity"/>.</remarks>
    public void AddImpulse(Vector2f dv)
        => Velocity += dv;

    /// <summary>
    /// Sets only the X component of velocity.
    /// </summary>
    public void SetVelocityX(System.Single vx)
        => Velocity = new Vector2f(vx, Velocity.Y);

    /// <summary>
    /// Sets only the Y component of velocity.
    /// </summary>
    public void SetVelocityY(System.Single vy)
        => Velocity = new Vector2f(Velocity.X, vy);

    #endregion

    #region ===== Update =====

    /// <summary>
    /// Integrates velocity with gravity &amp; drag and moves the owner each frame.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Update(System.Single dt)
    {
        // (VN) Bỏ qua nếu owner không còn hợp lệ/không hoạt động
        if (Owner == null || !Owner.Enabled)
        {
            return;
        }

        // 1) Gravity (downward, +Y)
        if (!FreezeY && Gravity != 0f)
        {
            Velocity.Y += Gravity * dt;
        }

        // 2) Linear drag (frame-rate independent)
        if (Drag > 0f)
        {
            System.Single k = 1f - System.Math.Clamp(Drag * dt, 0f, 0.99f);
            Velocity *= k;
        }

        // 3) Optional speed cap
        if (MaxSpeed.HasValue)
        {
            System.Single v2 = (Velocity.X * Velocity.X) + (Velocity.Y * Velocity.Y);
            System.Single max = MaxSpeed.Value;
            System.Single max2 = max * max;

            if (v2 > max2 && v2 > 0f)
            {
                System.Single invLen = 1f / System.MathF.Sqrt(v2);
                Velocity = new Vector2f(Velocity.X * invLen * max, Velocity.Y * invLen * max);
            }
        }

        // 4) Compose displacement
        Vector2f delta = new(
            FreezeX ? 0f : Velocity.X * dt,
            FreezeY ? 0f : Velocity.Y * dt
        );

        if (delta.X == 0f && delta.Y == 0f)
        {
            return;
        }

        // 5) Move via owner's hook (VN: tôn trọng override - có thể dịch rigidbody, tilemap, v.v.)
        Owner.MoveBy(delta);
    }

    #endregion
}
