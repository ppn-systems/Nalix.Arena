using Nalix.Portal;
using Nalix.Portal.Enums;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Portal.Objects.Indicators;

/// <summary>
/// Vòng quay tải (loading spinner) dạng overlay: nền mờ + icon xoay + dao động scale.
/// Có fade in/out, API Show/Hide, và tự re-center khi đổi độ phân giải.
/// </summary>
[IgnoredLoad("RenderObject")]
public sealed class LoadingSpinner : RenderObject
{
    #region Config

    private const System.Single MaxAlpha = 255f;
    private const System.Single Deg2Rad = 0.017453292519943295f;
    private const System.Byte DefaultOverlayAlpha = 160; // nền mờ mặc định

    #endregion

    #region Fields

    // params động
    private System.Single _rotationSpeed = 150f;
    private System.Single _fadeSpeed = 300f;
    private System.Single _baseScale = 0.6f;
    private System.Single _scaleOsc = 0.02f;

    // state
    private System.Single _angle = 0f;
    private System.Single _alpha = 0f;
    private System.Boolean _fadingIn = true;
    private System.Boolean _fadingOut = false;
    private System.Byte _currentAlpha = 0;

    // resize tracking
    private Vector2u _lastScreen = GraphicsEngine.ScreenSize;

    // drawables
    private readonly RectangleShape _bg;
    private readonly Sprite _icon;

    // behavior
    public System.Boolean BlocksInput { get; set; } = true;

    #endregion

    #region Ctor

    public LoadingSpinner()
    {
        SetZIndex(ZIndex.Overlay.ToInt()); // luôn trên cùng
        var screen = GraphicsEngine.ScreenSize;
        Vector2f screenSize = new(screen.X, screen.Y);

        _bg = new RectangleShape(screenSize)
        {
            FillColor = new Color(0, 0, 0, 0), // alpha set bởi fade
            Position = default
        };

        var tex = Assets.UiTextures.Load("icons/3");
        tex.Smooth = true;

        _icon = new Sprite(tex)
        {
            Origin = new Vector2f(tex.Size.X * 0.5f, tex.Size.Y * 0.5f),
            Position = new Vector2f(screenSize.X * 0.5f, screenSize.Y * 0.5f),
            Scale = new Vector2f(_baseScale, _baseScale),
            Color = new Color(255, 255, 255, 0)
        };

        _ = Show(); // mặc định bật spinner với fade-in
    }

    #endregion

    #region Public API (fluent)

    /// <summary>Bật spinner với fade-in.</summary>
    public LoadingSpinner Show()
    {
        Reveal();
        _fadingIn = true;
        _fadingOut = false;
        return this;
    }

    /// <summary>Tắt spinner với fade-out.</summary>
    public LoadingSpinner Hide()
    {
        _fadingOut = true;
        _fadingIn = false;
        return this;
    }

    /// <summary>Đặt icon spinner mới (texture key trong atlas).</summary>
    public LoadingSpinner SetIcon(System.String textureKey)
    {
        var tex = Assets.UiTextures.Load(textureKey);
        tex.Smooth = true;
        _icon.Texture = tex;
        _icon.Origin = new Vector2f(tex.Size.X * 0.5f, tex.Size.Y * 0.5f);
        Recenter();
        return this;
    }

    /// <summary>Đặt màu overlay nền (alpha sẽ bị điều khiển bởi fade).</summary>
    public LoadingSpinner SetOverlayColor(Color baseColor) { _bg.FillColor = new Color(baseColor.R, baseColor.G, baseColor.B, _currentAlpha); return this; }

    /// <summary>Đặt tốc độ: xoay (deg/s) và fade (alpha/s).</summary>
    public LoadingSpinner SetSpeeds(System.Single rotationDegPerSec, System.Single fadeAlphaPerSec)
    {
        _rotationSpeed = rotationDegPerSec;
        _fadeSpeed = fadeAlphaPerSec;
        return this;
    }

    /// <summary>Đặt scale cơ sở và biên độ dao động scale.</summary>
    public LoadingSpinner SetBaseScale(System.Single baseScale, System.Single osc = 0.02f)
    {
        _baseScale = baseScale;
        _scaleOsc = osc;
        return this;
    }

    #endregion

    #region Loop

    public override void Update(System.Single dt)
    {
        if (!Visible && !_fadingOut)
        {
            return;
        }

        // phát hiện đổi độ phân giải -> recenter
        if (_lastScreen != GraphicsEngine.ScreenSize)
        {
            _lastScreen = GraphicsEngine.ScreenSize;
            ResizeOverlay();
            Recenter();
        }

        UpdateAlpha(dt);

        // cập nhật góc xoay
        _angle += dt * _rotationSpeed;
        if (_angle >= 360f)
        {
            _angle -= 360f;
        }

        _icon.Rotation = _angle;

        // dao động scale theo sin
        System.Single scale = _baseScale + System.MathF.Sin(_angle * Deg2Rad) * _scaleOsc;
        _icon.Scale = new Vector2f(scale, scale);
    }

    public override void Render(RenderTarget target)
    {
        if (!Visible && !_fadingOut)
        {
            return;
        }

        target.Draw(_bg);
        target.Draw(_icon);
    }

    protected override Drawable GetDrawable()
        => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

    #endregion

    #region Internals

    private void UpdateAlpha(System.Single dt)
    {
        // nếu đang ẩn hoàn toàn và không fade-out thì khỏi cập nhật
        if (!_fadingIn && !_fadingOut && !Visible)
        {
            return;
        }

        if (_fadingIn)
        {
            _alpha += dt * _fadeSpeed;
            if (_alpha >= MaxAlpha)
            {
                _alpha = MaxAlpha;
                _fadingIn = false;
                Reveal();
            }
        }
        else if (_fadingOut)
        {
            _alpha -= dt * _fadeSpeed;
            if (_alpha <= 0f)
            {
                _alpha = 0f;
                _fadingOut = false;
                Conceal();
            }
        }

        System.Byte newA = (System.Byte)System.Math.Clamp(_alpha, 0f, MaxAlpha);
        if (newA == _currentAlpha)
        {
            return;
        }

        _currentAlpha = newA;

        // overlay: trộn màu nền hiện tại với alpha mới
        var bc = _bg.FillColor;
        _bg.FillColor = new Color(bc.R, bc.G, bc.B, _currentAlpha);

        // icon: alpha theo fade
        var ic = _icon.Color;
        _icon.Color = new Color(ic.R, ic.G, ic.B, _currentAlpha);
    }

    private void ResizeOverlay()
    {
        var sz = GraphicsEngine.ScreenSize;
        _bg.Size = new Vector2f(sz.X, sz.Y);
    }

    private void Recenter()
    {
        var sz = GraphicsEngine.ScreenSize;
        _icon.Position = new Vector2f(sz.X * 0.5f, sz.Y * 0.5f);

        // nếu overlay chưa có màu base, set alpha nền mặc định để dễ nhìn khi fade-in
        if (_bg.FillColor.A == 0)
        {
            _bg.FillColor = new Color(0, 0, 0, System.Math.Min(DefaultOverlayAlpha, _currentAlpha));
        }
    }

    #endregion
}
