using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Camera;

public static class Camera2D
{
    private static Vector2f _targetPos;
    private static System.Single _zoom = 1f;
    private static System.Single _shakeTime, _shakeStrength;

    public static void Initialize(Vector2u screenSize)
    {
        Current = new View(new FloatRect(0, 0, screenSize.X, screenSize.Y));
        _targetPos = Current.Center;
    }

    public static void Follow(Vector2f worldPos) => _targetPos = worldPos;

    public static void SetZoom(System.Single factor)
    {
        if (factor <= 0f)
        {
            return;
        }

        System.Single scale = factor / _zoom;
        Current.Zoom(scale);
        _zoom = factor;
    }

    public static void Shake(System.Single strength, System.Single time)
    {
        _shakeStrength = strength;
        _shakeTime = time;
    }

    public static void Update(System.Single dt)
    {
        // lerp center cho mượt
        var c = Current.Center;
        c += (_targetPos - c) * System.MathF.Min(1f, dt * 10f);

        // shake
        if (_shakeTime > 0f)
        {
            _shakeTime -= dt;
            var ox = (System.Random.Shared.NextSingle() - 0.5f) * 2f * _shakeStrength;
            var oy = (System.Random.Shared.NextSingle() - 0.5f) * 2f * _shakeStrength;
            Current.Center = new(c.X + ox, c.Y + oy);
        }
        else
        {
            Current.Center = c;
        }
    }

    public static void Apply(RenderTarget target) => target.SetView(Current);

    public static View Current { get; private set; }
}
