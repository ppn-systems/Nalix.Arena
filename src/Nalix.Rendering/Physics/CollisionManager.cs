
using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>
/// Broad-phase: spatial hash grid; Narrow-phase: AABB; 
/// Hỗ trợ layer/mask, trigger, và resolve chèn (push-out).
/// </summary>
public static class CollisionManager
{
    // === Tuning ===
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static System.Single CellSize = 256f;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static System.Boolean DoResolve = true;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static System.Boolean SymmetricMask = true;

    private static readonly System.Collections.Generic.HashSet<ColliderObject> _colliders = [];
    private static readonly System.Collections.Generic.HashSet<(System.Int32 A, System.Int32 B)> _activePairs = [];
    private static readonly System.Collections.Generic.HashSet<(System.Int32 A, System.Int32 B)> _thisFramePairs = [];
    private static readonly System.Collections.Generic.Dictionary<
        (System.Int32 cx, System.Int32 cy), System.Collections.Generic.List<ColliderObject>> _grid = [];

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Int32 Id(ColliderObject c) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(c);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Register(ColliderObject c) => _colliders.Add(c);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Unregister(ColliderObject c)
    {
        _ = _colliders.Remove(c);
        System.Int32 id = Id(c);
        _ = _activePairs.RemoveWhere(p => p.A == id || p.B == id);
    }

    [System.Runtime.CompilerServices.MethodImpl(
       System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Update(System.Single __)
    {
        // 1) Rebuild spatial grid
        _grid.Clear();
        foreach (var c in _colliders)
        {
            if (!c.Enabled || c.Paused || !c.Visible)
            {
                continue;
            }

            var r = c.ColliderAABB;
            if (r.Width <= 0 || r.Height <= 0)
            {
                continue;
            }

            CellBounds(in r, out System.Int32 minX, out System.Int32 minY, out System.Int32 maxX, out System.Int32 maxY);
            for (System.Int32 cy = minY; cy <= maxY; cy++)
            {
                for (System.Int32 cx = minX; cx <= maxX; cx++)
                {
                    var key = (cx, cy);
                    if (!_grid.TryGetValue(key, out var bucket))
                    {
                        bucket = [];
                        _grid[key] = bucket;
                    }
                    bucket.Add(c);
                }
            }
        }

        // 2) Narrow-phase trên từng bucket
        _thisFramePairs.Clear();

        foreach (var bucket in _grid.Values)
        {
            System.Int32 n = bucket.Count;
            for (System.Int32 i = 0; i < n; i++)
            {
                var a = bucket[i];
                if (!a.Enabled || a.Paused || !a.Visible)
                {
                    continue;
                }

                var ra = a.ColliderAABB;
                System.Int32 ia = Id(a);

                for (System.Int32 j = i + 1; j < n; j++)
                {
                    var b = bucket[j];
                    if (!b.Enabled || b.Paused || !b.Visible)
                    {
                        continue;
                    }

                    if (!MaskPass(a, b))
                    {
                        continue;
                    }

                    var rb = b.ColliderAABB;
                    if (!ra.Intersects(in rb))
                    {
                        continue;
                    }

                    // Chuẩn hóa cặp
                    System.Int32 ib = Id(b);
                    var pair = ia < ib ? (A: ia, B: ib) : (A: ib, B: ia);
                    if (!_thisFramePairs.Add(pair))
                    {
                        continue;
                    }

                    var A = ia == pair.A ? a : b;
                    var B = ia == pair.A ? b : a;

                    System.Boolean wasActive = _activePairs.Contains(pair);
                    if (!wasActive) { A.OnCollisionEnter(B); B.OnCollisionEnter(A); }
                    else { A.OnCollisionStay(B); B.OnCollisionStay(A); }

                    if (DoResolve && !A.IsTrigger && !B.IsTrigger)
                    {
                        ResolveMTV(A, B); // push-out 50/50
                    }
                }
            }
        }

        // 3) Exit events
        foreach (var pair in _activePairs)
        {
            if (!_thisFramePairs.Contains(pair))
            {
                var a = FindById(pair.A);
                var b = FindById(pair.B);
                if (a != null && b != null)
                {
                    a.OnCollisionExit(b);
                    b.OnCollisionExit(a);
                }
            }
        }

        _activePairs.Clear();
        foreach (var p in _thisFramePairs)
        {
            _ = _activePairs.Add(p);
        }
    }

    // === Helpers ===
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Boolean MaskPass(ColliderObject a, ColliderObject b)
    {
        System.Boolean ab = (a.Mask & b.Layer) != 0;
        if (!SymmetricMask)
        {
            return ab;
        }

        System.Boolean ba = (b.Mask & a.Layer) != 0;
        return ab && ba;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void CellBounds(in AABB r, out System.Int32 minX, out System.Int32 minY, out System.Int32 maxX, out System.Int32 maxY)
    {
        System.Single inv = 1f / CellSize;
        minX = (System.Int32)System.MathF.Floor(r.MinX * inv);
        minY = (System.Int32)System.MathF.Floor(r.MinY * inv);
        maxX = (System.Int32)System.MathF.Floor(r.MaxX * inv);
        maxY = (System.Int32)System.MathF.Floor(r.MaxY * inv);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static ColliderObject FindById(System.Int32 id)
    {
        foreach (var c in _colliders)
        {
            if (Id(c) == id)
            {
                return c;
            }
        }

        return null;
    }

    /// <summary>Đẩy rời nhau theo trục chèn ít nhất (Minimum Translation Vector – MTV), chia đều 50/50.</summary>
    private static void ResolveMTV(ColliderObject a, ColliderObject b)
    {
        // Lấy lại AABB mới nhất (sau khi OnCollisionEnter/Stay có thể đã thay đổi transform)
        var ra = a.ColliderAABB;
        var rb = b.ColliderAABB;

        System.Single overlapX = System.MathF.Min(ra.MaxX, rb.MaxX) - System.MathF.Max(ra.MinX, rb.MinX);
        System.Single overlapY = System.MathF.Min(ra.MaxY, rb.MaxY) - System.MathF.Max(ra.MinY, rb.MinY);
        if (overlapX <= 0 || overlapY <= 0)
        {
            return;
        }

        if (overlapX < overlapY)
        {
            System.Single axc = (ra.MinX + ra.MaxX) * 0.5f;
            System.Single bxc = (rb.MinX + rb.MaxX) * 0.5f;
            System.Single dir = axc < bxc ? -1f : 1f; // a bên trái thì đẩy sang trái
            System.Single half = overlapX * 0.5f * dir;
            a.MoveBy(new Vector2f(half, 0));
            b.MoveBy(new Vector2f(-half, 0));
        }
        else
        {
            System.Single ayc = (ra.MinY + ra.MaxY) * 0.5f;
            System.Single byc = (rb.MinY + rb.MaxY) * 0.5f;
            System.Single dir = ayc < byc ? -1f : 1f; // a ở trên thì đẩy lên trên
            System.Single half = overlapY * 0.5f * dir;
            a.MoveBy(new Vector2f(0, half));
            b.MoveBy(new Vector2f(0, -half));
        }
    }
}
