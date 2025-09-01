using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>
/// Base sprite object that exposes an <see cref="AABB"/> collider and collision metadata (layer, mask, trigger).
/// </summary>
/// <remarks>
/// <para>
/// (VN) Sprite có collider AABB + metadata va chạm. Mặc định collider = <c>Sprite.GetGlobalBounds()</c>
/// (SFML đã trả về hộp bao <b>thẳng trục</b> của sprite sau mọi transform).
/// </para>
/// <para>
/// Có thể tinh chỉnh nhanh bằng <see cref="ColliderOffset"/> và <see cref="ColliderSizeOverride"/>.
/// Nếu cần logic phức tạp hơn, override <see cref="ComputeColliderAABB()"/>.
/// </para>
/// </remarks>
public abstract class ColliderObject : SpriteObject
{
    #region ===== Constructors (mirror SpriteObject) =====

    protected ColliderObject(Texture texture) : base(texture) { }
    protected ColliderObject(Texture texture, IntRect rect) : base(texture, rect) { }
    protected ColliderObject(Texture texture, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, position, scale, rotation) { }
    protected ColliderObject(Texture texture, IntRect rect, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, rect, position, scale, rotation) { }

    #endregion

    #region ===== Collider shape (AABB) =====

    /// <summary>
    /// Optional local-space offset applied to the collider.
    /// </summary>
    /// <remarks>
    /// (VN) Dịch chuyển hộp va chạm theo <b>toạ độ thế giới</b> (sau khi lấy global bounds).
    /// Hữu ích khi muốn “nới/thu” hoặc dời collider nhanh mà không cần override.
    /// </remarks>
    public Vector2f ColliderOffset { get; set; } = default;

    /// <summary>
    /// Optional override for collider size (world units). When set, replaces the width/height of the computed AABB.
    /// </summary>
    /// <remarks>(VN) Nếu null → dùng size từ <c>Sprite.GetGlobalBounds()</c>.</remarks>
    public Vector2f? ColliderSizeOverride { get; set; }

    /// <summary>
    /// World-space collider AABB.
    /// </summary>
    /// <remarks>
    /// (VN) Mặc định lấy từ <c>Sprite.GetGlobalBounds()</c> (đã AABB hoá theo transform),
    /// sau đó áp dụng <see cref="ColliderOffset"/> và <see cref="ColliderSizeOverride"/>.
    /// </remarks>
    public virtual AABB ColliderAABB => ComputeColliderAABB();

    /// <summary>
    /// Computes the world-space AABB for collision queries.
    /// </summary>
    /// <returns>The computed <see cref="AABB"/>.</returns>
    /// <remarks>
    /// (VN) Override hàm này nếu bạn cần hộp va chạm tính theo logic riêng (ví dụ: pad theo tỉ lệ, theo frame…).
    /// </remarks>
    protected virtual AABB ComputeColliderAABB()
    {
        // SFML trả FloatRect (Left,Top,Width,Height) sau mọi transform; đã là AABB theo trục
        FloatRect gb = Sprite.GetGlobalBounds();

        System.Single left = gb.Left + ColliderOffset.X;
        System.Single top = gb.Top + ColliderOffset.Y;

        System.Single width = ColliderSizeOverride?.X ?? gb.Width;
        System.Single height = ColliderSizeOverride?.Y ?? gb.Height;

        return AABB.FromMinSize(left, top, width, height);
    }

    #endregion

    #region ===== Collision metadata =====

    /// <summary>
    /// Collision layer this object belongs to.
    /// </summary>
    /// <remarks>(VN) Lớp của chính object – thường là bitflag.</remarks>
    public virtual CollisionLayer Layer { get; set; } = CollisionLayer.Default;

    /// <summary>
    /// Collision mask specifying which layers this object wants to interact with.
    /// </summary>
    /// <remarks>(VN) Mặt nạ: object quan tâm va chạm với layer nào (bitflag).</remarks>
    public virtual CollisionLayer Mask { get; set; } = CollisionLayer.All;

    /// <summary>
    /// If true, the collider is a trigger: detected but not physically resolved.
    /// </summary>
    /// <remarks>(VN) Trigger: chỉ báo va chạm (Enter/Stay/Exit), không đẩy tách.</remarks>
    public virtual System.Boolean IsTrigger { get; set; } = false;

    #endregion

    #region ===== Helpers =====

    /// <summary>
    /// Quickly checks if two objects are intended to collide based on their layers and masks.
    /// </summary>
    /// <remarks>(VN) Kiểm tra logic layer/mask 2 chiều.</remarks>
    public System.Boolean CanCollideWith(ColliderObject other)
        => other != null
           && (Mask & other.Layer) != 0
           && (other.Mask & Layer) != 0;

    /// <summary>
    /// Tests for AABB overlap if <see cref="CanCollideWith(ColliderObject)"/> is satisfied.
    /// </summary>
    /// <remarks>(VN) Tiện dụng cho prefilter nhanh trước khi nhờ <c>CollisionManager</c>.</remarks>
    public System.Boolean CollidesWith(ColliderObject other)
        => other != null
           && CanCollideWith(other)
           && ColliderAABB.Intersects(other.ColliderAABB);

    /// <summary>
    /// Moves the underlying sprite by <paramref name="delta"/> (used by collision resolution).
    /// </summary>
    /// <param name="delta">Delta displacement in world units.</param>
    /// <remarks>(VN) Hàm mặc định: dời sprite trực tiếp. Có thể override để dời rigidbody, v.v.</remarks>
    public virtual void MoveBy(Vector2f delta) => Sprite.Position += delta;

    #endregion

    #region ===== Collision callbacks =====

    /// <summary>
    /// Called on the first frame two colliders begin overlapping.
    /// </summary>
    /// <remarks>(VN) Lần đầu va chạm.</remarks>
    public virtual void OnCollisionEnter(ColliderObject other) { }

    /// <summary>
    /// Called every frame while two colliders keep overlapping.
    /// </summary>
    /// <remarks>(VN) Đang giữ va chạm.</remarks>
    public virtual void OnCollisionStay(ColliderObject other) { }

    /// <summary>
    /// Called on the first frame two colliders stop overlapping.
    /// </summary>
    /// <remarks>(VN) Rời va chạm.</remarks>
    public virtual void OnCollisionExit(ColliderObject other) { }

    #endregion

    #region ===== Lifecycle =====

    /// <summary>
    /// Registers this collider with the <c>CollisionManager</c>.
    /// </summary>
    /// <remarks>(VN) Tự động đăng ký khi object được khởi tạo.</remarks>
    protected override void Initialize() => CollisionManager.Register(this);

    /// <summary>
    /// Unregisters this collider from the <c>CollisionManager</c>.
    /// </summary>
    /// <remarks>(VN) Tự động huỷ đăng ký khi object bị destroy.</remarks>
    public override void BeforeDestroy() => CollisionManager.Unregister(this);

    #endregion
}
