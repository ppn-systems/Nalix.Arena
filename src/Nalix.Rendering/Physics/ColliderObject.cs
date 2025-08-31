using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>Sprite có collider AABB + metadata collision.</summary>
public abstract class ColliderObject : SpriteObject
{
    protected ColliderObject(Texture t) : base(t) { }
    protected ColliderObject(Texture t, IntRect r) : base(t, r) { }
    protected ColliderObject(Texture t, Vector2f p, Vector2f s, System.Single rot) : base(t, p, s, rot) { }
    protected ColliderObject(Texture t, IntRect r, Vector2f p, Vector2f s, System.Single rot) : base(t, r, p, s, rot) { }

    /// <summary>AABB world-space. Dùng global bounds của SFML (đã axis-aligned theo transform).</summary>
    public virtual AABB ColliderAABB
    {
        get
        {
            var gb = Sprite.GetGlobalBounds();
            return AABB.FromMinSize(gb.Left, gb.Top, gb.Width, gb.Height);
        }
    }

    public virtual CollisionLayer Layer { get; set; } = CollisionLayer.Default;
    public virtual CollisionLayer Mask { get; set; } = CollisionLayer.All;
    public virtual System.Boolean IsTrigger { get; set; } = false;

    /// <summary>Di chuyển khi resolve va chạm.</summary>
    public virtual void MoveBy(Vector2f delta) => Sprite.Position += delta;

    public virtual void OnCollisionEnter(ColliderObject other) { }
    public virtual void OnCollisionStay(ColliderObject other) { }
    public virtual void OnCollisionExit(ColliderObject other) { }

    protected override void Initialize() => CollisionManager.Register(this);
    public override void BeforeDestroy() => CollisionManager.Unregister(this);
}
