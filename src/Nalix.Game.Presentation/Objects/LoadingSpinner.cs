using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Game.Presentation.Objects;

[IgnoredLoad("RenderObject")]
public class LoadingSpinner : RenderObject
{
    private float _angle;
    private float _alpha = 0f;
    private bool _fadingIn = true;
    private Color _backgroundColor = new(0, 0, 0, 0);
    private Color _iconColor = new(255, 255, 255, 255);

    private readonly Sprite _iconSprite;
    private readonly RectangleShape _background;

    public LoadingSpinner()
    {
        base.SetZIndex(System.Int32.MaxValue);

        _background = new RectangleShape((Vector2f)GameEngine.ScreenSize)
        {
            FillColor = _backgroundColor, // bắt đầu từ trong suốt
            Position = new Vector2f(0, 0)
        };

        // Tải icon
        Texture iconTexture = Assets.UI.Load("icons/16");
        iconTexture.Smooth = true;

        _iconSprite = new Sprite(iconTexture)
        {
            Origin = new Vector2f(iconTexture.Size.X / 2f, iconTexture.Size.Y / 2f),
            Position = new Vector2f(GameEngine.ScreenSize.X / 2f, GameEngine.ScreenSize.Y / 2f),
            Scale = new Vector2f(0.5f, 0.5f) // scale nếu cần
        };
    }

    public override void Update(float deltaTime)
    {
        _angle += deltaTime * 150f;
        if (_angle > 360f) _angle -= 360f;
        _iconSprite.Rotation = _angle;

        float scaleValue = 0.5f + (System.MathF.Sin(_angle * 0.01f) * 0.02f);
        _iconSprite.Scale = new Vector2f(scaleValue, scaleValue);

        this.UpdateAlpha(deltaTime);
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible) return;

        target.Draw(_background);
        target.Draw(_iconSprite);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

    private void UpdateAlpha(float deltaTime)
    {
        float fadeSpeed = 300f; // tốc độ fade per second

        if (_fadingIn)
        {
            _alpha += deltaTime * fadeSpeed;
            if (_alpha >= 255f)
            {
                _alpha = 255f;
                _fadingIn = false; // tự dừng fade nếu cần
            }
        }

        byte a = (byte)_alpha;

        if (_backgroundColor.A != a)
        {
            _backgroundColor.A = a;
            _background.FillColor = _backgroundColor;
        }
        if (_iconColor.A != a)
        {
            _iconColor.A = a;
            _iconSprite.Color = _iconColor;
        }
    }
}