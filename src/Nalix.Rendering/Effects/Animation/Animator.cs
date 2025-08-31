using SFML.Graphics;

namespace Nalix.Rendering.Effects.Animation;

/// <summary>
/// Quản lý khung hình từ spritesheet (IntRect frames).
/// </summary>
public sealed class Animator(Sprite sprite, System.Single frameTime = 0.1f)
{
    private readonly System.Collections.Generic.List<IntRect> _frames = [];
    private readonly Sprite _sprite = sprite;
    private System.Single _frameTime = frameTime <= 0f ? 0.1f : frameTime;
    private System.Single _acc;
    private System.Int32 _index;
    public System.Boolean Loop { get; set; } = true;
    public System.Boolean Playing { get; private set; }

    public void SetFrames(System.Collections.Generic.IEnumerable<IntRect> frames)
    {
        _frames.Clear();
        _frames.AddRange(frames);
        _index = 0;
        _acc = 0f;
        ApplyFrame();
    }

    public void Play() => Playing = true;
    public void Pause() => Playing = false;
    public void Stop() { Playing = false; _index = 0; _acc = 0f; ApplyFrame(); }

    public void SetFrameTime(System.Single seconds) => _frameTime = System.Math.Max(0.001f, seconds);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Update(System.Single dt)
    {
        if (!Playing || _frames.Count == 0)
        {
            return;
        }

        _acc += dt;
        while (_acc >= _frameTime)
        {
            _acc -= _frameTime;
            _index++;
            if (_index >= _frames.Count)
            {
                if (Loop)
                {
                    _index = 0;
                }
                else { _index = _frames.Count - 1; Playing = false; break; }
            }
            ApplyFrame();
        }
    }

    private void ApplyFrame()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        _sprite.TextureRect = _frames[_index];
    }
}
