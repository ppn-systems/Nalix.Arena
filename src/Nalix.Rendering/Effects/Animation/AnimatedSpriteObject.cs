using Nalix.Rendering.Objects;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Rendering.Effects.Animation;

/// <summary>
/// SpriteObject có Animator tích hợp.
/// </summary>
public abstract class AnimatedSpriteObject : SpriteObject
{
    protected readonly Animator Animator;

    protected AnimatedSpriteObject(Texture texture)
        : base(texture)
        => Animator = new Animator(Sprite);

    protected AnimatedSpriteObject(Texture texture, IntRect rect)
        : base(texture, rect)
        => Animator = new Animator(Sprite);

    protected AnimatedSpriteObject(Texture texture, Vector2f pos, Vector2f scale, System.Single rot)
        : base(texture, pos, scale, rot)
        => Animator = new Animator(Sprite);

    protected AnimatedSpriteObject(Texture texture, IntRect rect, Vector2f pos, Vector2f scale, System.Single rot)
        : base(texture, rect, pos, scale, rot) => Animator = new Animator(Sprite);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetAnimationFrames(
        System.Collections.Generic.IEnumerable<IntRect> frames, System.Single frameTime, System.Boolean loop = true)
    {
        Animator.SetFrames(frames);
        Animator.SetFrameTime(frameTime);
        Animator.Loop = loop;
        Animator.Play();
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Update(System.Single deltaTime) => Animator.Update(deltaTime);
}
