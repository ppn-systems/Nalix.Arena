// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Objects.Controls;
using Nalix.Launcher.Services.Dtos;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu.Main.View;

[IgnoredLoad("RenderObject")]
internal sealed class MainMenuView : RenderObject
{
    // Sự kiện bắn ra cho Controller
    public event System.Action LoginRequested;
    public event System.Action RegisterRequested;
    public event System.Action NewsRequested;
    public event System.Action ExitRequested;

    private readonly StretchableButton _login;
    private readonly StretchableButton _register;
    private readonly StretchableButton _news;
    private readonly StretchableButton _exit;
    private readonly StretchableButton[] _buttons;

    private readonly ThemeDto _theme;
    private const System.Single ButtonWidth = 380f;
    private const System.Single VerticalSpacing = 25f;
    private Vector2u _lastSize;

    public MainMenuView(ThemeDto theme)
    {
        _theme = theme ?? throw new System.ArgumentNullException(nameof(theme));
        SetZIndex(_theme.OverlayZ);

        _login = NewButton("LOGIN");
        _register = NewButton("REGISTER");
        _news = NewButton("NEWS");
        _exit = NewButton("EXIT");
        _buttons = [_login, _register, _news, _exit];

        ApplyStyles();
        WireHandlers();

        _lastSize = GraphicsEngine.ScreenSize;
        LayoutButtons(_lastSize);
    }

    private static StretchableButton NewButton(System.String text)
        => new(text, ButtonWidth, "panels/005");

    private void ApplyStyles()
    {
        _ = _login.SetColors(C(_theme.PanelDark), C(_theme.PanelHover))
                  .SetTextColors(C(_theme.TextWhite), C(_theme.TextNeon));
        _login.SetTextOutline(new Color(0, 0, 0, 160), 2f);

        _ = _register.SetColors(C(_theme.PanelAlt), C(_theme.PanelAltHover))
                     .SetTextColors(C(_theme.TextSoft), C(_theme.TextNeon));
        _register.SetTextOutline(new Color(0, 0, 0, 160), 2f);

        _ = _news.SetColors(C(_theme.PanelDark), C(_theme.PanelHover))
                 .SetTextColors(C(_theme.TextSoft), C(_theme.TextNeon));
        _news.SetTextOutline(new Color(0, 0, 0, 160), 2f);

        _ = _exit.SetColors(C(_theme.PanelAlt), C(_theme.PanelAltHover))
                 .SetTextColors(C(_theme.ExitNormal), C(_theme.ExitHover));
        _exit.SetTextOutline(new Color(0, 0, 0, 180), 2f);
    }

    private void WireHandlers()
    {
        // Chỉ raise event, Controller sẽ xử lý SFX + điều hướng
        _login.RegisterClickHandler(() => LoginRequested?.Invoke());
        _register.RegisterClickHandler(() => RegisterRequested?.Invoke());
        _news.RegisterClickHandler(() => NewsRequested?.Invoke());
        _exit.RegisterClickHandler(() => ExitRequested?.Invoke());
    }

    private void LayoutButtons(Vector2u screen)
    {
        System.Single total = 0f;
        foreach (var b in _buttons)
        {
            total += b.GetGlobalBounds().Height + VerticalSpacing;
        }

        total -= VerticalSpacing;

        System.Single y = (screen.Y - total) / 1.65f;

        foreach (var b in _buttons)
        {
            var r = b.GetGlobalBounds();
            System.Single x = (screen.X - r.Width) / 2f;
            b.SetPosition(new Vector2f(x, y));
            y += r.Height + VerticalSpacing;
        }
    }

    public override void Update(System.Single dt)
    {
        if (!Visible)
        {
            return;
        }

        foreach (var b in _buttons)
        {
            b.Update(dt);
        }

        // Re-layout khi thay đổi kích thước
        var size = GraphicsEngine.ScreenSize;
        if (size != _lastSize)
        {
            _lastSize = size;
            LayoutButtons(size);
        }
    }

    public override void Render(RenderTarget target)
    {
        foreach (var b in _buttons)
        {
            b.Render(target);
        }
    }

    protected override Drawable GetDrawable() => null;

    private static Color C(System.Drawing.Color c) => new(c.R, c.G, c.B, c.A);
}
