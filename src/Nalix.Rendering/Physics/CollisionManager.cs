using SFML.System;

namespace Nalix.Rendering.Physics;

/// <summary>
/// 2D collision system:
/// <b>Broad-phase</b> via spatial hash grid → <b>Narrow-phase</b> via <see cref="AABB"/>.
/// Supports layer/mask filtering, triggers, and 50/50 push-out resolution.
/// </summary>
/// <remarks>
/// <para>
/// (VN) Va chạm 2D: quét nhanh bằng lưới (spatial hash), kiểm tra chồng lấn bằng AABB.
/// Hỗ trợ lọc <b>Layer/Mask</b>, <b>Trigger</b>, và <b>đẩy tách</b> theo MTV (chia đều mỗi bên).
/// </para>
/// <para>
/// Quy trình mỗi frame: <br/>
/// 1) Xây lưới từ tất cả collider hợp lệ → nhét vào các cell giao nhau. <br/>
/// 2) Với từng bucket (cell), kiểm tra cặp (i,j): lọc mask, test AABB, phát <c>Enter/Stay</c>, và (nếu bật) resolve MTV. <br/>
/// 3) Cặp nào frame trước có, frame này không có → phát <c>Exit</c>.
/// </para>
/// </remarks>
public static class CollisionManager
{
    #region ===== Tuning (public knobs) =====

    /// <summary>
    /// Spatial grid cell size (world units). Larger cell → ít bucket hơn, nhưng nhiều cặp trong cùng bucket.
    /// </summary>
    public static System.Single CellSize = 256f;

    /// <summary>
    /// Whether to resolve penetrations by pushing objects apart using MTV.
    /// </summary>
    public static System.Boolean DoResolve = true;

    /// <summary>
    /// If true, both (A.Mask &amp; B.Layer) and (B.Mask &amp; A.Layer) must pass; if false, only A→B is checked.
    /// </summary>
    public static System.Boolean SymmetricMask = true;

    #endregion

    #region ===== Internal state =====

    private static readonly System.Collections.Generic.HashSet<ColliderObject> _colliders = [];
    // Active (=overlapping) pairs persisted from previous frame
    private static readonly System.Collections.Generic.HashSet<(System.Int32 A, System.Int32 B)> _activePairs = [];
    // Pairs discovered this frame
    private static readonly System.Collections.Generic.HashSet<(System.Int32 A, System.Int32 B)> _thisFramePairs = [];

    // Spatial grid: (cellX, cellY) → colliders in this bucket
    private static readonly System.Collections.Generic.Dictionary<(System.Int32 cx, System.Int32 cy), System.Collections.Generic.List<ColliderObject>> _grid = [];

    #endregion

    #region ===== Public API =====

    /// <summary>Registers a collider to be considered in collision queries.</summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Register(ColliderObject c) => _colliders.Add(c);

    /// <summary>Unregisters a collider and clears any active pairs involving it.</summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Unregister(ColliderObject c)
    {
        _ = _colliders.Remove(c);
        System.Int32 id = Id(c);
        _ = _activePairs.RemoveWhere(p => p.A == id || p.B == id);
    }

    /// <summary>
    /// Steps the collision world by one frame: rebuild spatial grid, detect overlaps, send events, resolve penetration.
    /// </summary>
    /// <param name="_deltaTime">Delta time (not used here; kept for symmetry with engine loop).</param>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Update(System.Single _deltaTime)
    {
        // === 1) Build spatial grid ===
        _grid.Clear();

        foreach (var c in _colliders)
        {
            // (VN) Bỏ qua collider không “hoạt động” trong frame
            if (!c.Enabled || c.Paused || !c.Visible)
            {
                continue;
            }

            var r = c.ColliderAABB;
            if (r.Width <= 0 || r.Height <= 0)
            {
                continue;
            }

            // (VN) Colliders có thể phủ nhiều cell → chèn vào tất cả cell giao
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

        // === 2) Narrow-phase per bucket ===
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

                    // Chuẩn hoá cặp (A < B) để set logic/event ổn định
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

                    // (VN) Đẩy tách nếu được bật và cả hai KHÔNG phải trigger
                    if (DoResolve && !A.IsTrigger && !B.IsTrigger)
                    {
                        ResolveMTV(A, B); // push-out 50/50 theo trục chèn ít nhất
                    }
                }
            }
        }

        // === 3) Exit events (cặp cũ không còn trong frame này) ===
        foreach (var pair in _activePairs)
        {
            if (_thisFramePairs.Contains(pair))
            {
                continue;
            }

            var a = FindById(pair.A);
            var b = FindById(pair.B);
            if (a != null && b != null)
            {
                a.OnCollisionExit(b);
                b.OnCollisionExit(a);
            }
        }

        // Ghi đè activePairs = thisFramePairs cho frame kế
        _activePairs.Clear();
        foreach (var p in _thisFramePairs)
        {
            _ = _activePairs.Add(p);
        }
    }

    #endregion

    #region ===== Helpers =====

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Int32 Id(ColliderObject c)
        => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(c);

    /// <summary>
    /// Mask filtering: checks (A.Mask &amp; B.Layer) and, if <see cref="SymmetricMask"/> is true, also (B.Mask &amp; A.Layer).
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Computes which grid cells an AABB spans.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void CellBounds(in AABB r, out System.Int32 minX, out System.Int32 minY, out System.Int32 maxX, out System.Int32 maxY)
    {
        System.Single inv = 1f / CellSize;
        minX = (System.Int32)System.MathF.Floor(r.MinX * inv);
        minY = (System.Int32)System.MathF.Floor(r.MinY * inv);
        maxX = (System.Int32)System.MathF.Floor(r.MaxX * inv);
        maxY = (System.Int32)System.MathF.Floor(r.MaxY * inv);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Pushes two overlapping AABBs apart along the minimum-overlap axis (MTV), splitting displacement 50/50.
    /// </summary>
    /// <remarks>
    /// (VN) Lấy AABB mới nhất (vì callback <c>Enter/Stay</c> có thể đã đổi transform), tính chèn X/Y,
    /// rồi đẩy mỗi bên một nửa theo trục chèn ít hơn.
    /// </remarks>
    private static void ResolveMTV(ColliderObject a, ColliderObject b)
    {
        var ra = a.ColliderAABB;
        var rb = b.ColliderAABB;

        System.Single overlapX = System.MathF.Min(ra.MaxX, rb.MaxX) - System.MathF.Max(ra.MinX, rb.MinX);
        System.Single overlapY = System.MathF.Min(ra.MaxY, rb.MaxY) - System.MathF.Max(ra.MinY, rb.MinY);
        if (overlapX <= 0f || overlapY <= 0f)
        {
            return;
        }

        if (overlapX < overlapY)
        {
            // (VN) Đẩy theo trục X – xác định chiều dựa tâm
            System.Single axc = (ra.MinX + ra.MaxX) * 0.5f;
            System.Single bxc = (rb.MinX + rb.MaxX) * 0.5f;
            System.Single dir = axc < bxc ? -1f : 1f;     // A bên trái → đẩy A sang trái
            System.Single half = overlapX * 0.5f * dir;

            a.MoveBy(new Vector2f(half, 0f));
            b.MoveBy(new Vector2f(-half, 0f));
        }
        else
        {
            // (VN) Đẩy theo trục Y
            System.Single ayc = (ra.MinY + ra.MaxY) * 0.5f;
            System.Single byc = (rb.MinY + rb.MaxY) * 0.5f;
            System.Single dir = ayc < byc ? -1f : 1f;     // A ở trên → đẩy A lên trên
            System.Single half = overlapY * 0.5f * dir;

            a.MoveBy(new Vector2f(0f, half));
            b.MoveBy(new Vector2f(0f, -half));
        }
    }

    #endregion
}
