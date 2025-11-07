// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Portal;
using Nalix.Portal.Objects.Controls;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Portal.Scenes.Menu.News.View;

/// <summary>
/// View-only: draw/layout News UI; raise events for controller.
/// No navigation or network calls here.
/// </summary>
[IgnoredLoad("RenderObject")]
internal sealed class NewsView : RenderObject
{
    // ===== Events to be handled by Controller =====
    public event System.Action BackRequested;

    // ===== Visual constants =====
    private const System.Single PanelWidthRatio = 0.95f;
    private const System.Single PanelHeightRatio = 0.9f;
    private const System.Single PanelColorGray = 40f;
    private const System.Single TitleFontSize = 32f;
    private const System.Single BodyFontSize = 20f;
    private const System.Single TitleTopPadding = 24f;
    private const System.Single SidePadding = 24f;
    private const System.Single GapTitleToDivider = 12f;
    private const System.Single PanelSideTrim = 32f;
    private const System.Single BodyLeftPadding = 40f;
    private const System.Single BodyTopGapFromTitle = 28f;
    private const System.Single BackBottomPadding = 28f;
    private const System.Single TitleOutlineThickness = 2f;
    private const System.Single BodyOutlineThickness = 1.5f;
    private const System.Single BackButtonWidth = 200f;

    // ===== Fields & assets =====
    private readonly NineSlicePanel _bg;
    private readonly Text _title;
    private readonly Text _content;
    private readonly StretchableButton _backBtn;
    private readonly Sprite _divLeft, _divRight;
    private readonly Texture _divTex;
    private readonly Font _font;

    public NewsView(System.String dividerTexture = "divider/002")
    {
        // Background panel
        var texPanel = Assets.UiTextures.Load("panels/031");
        _bg = new NineSlicePanel(texPanel, new Thickness(32));
        var screen = GraphicsEngine.ScreenSize;
        var size = new Vector2f(screen.X * PanelWidthRatio, screen.Y * PanelHeightRatio);
        var pos = new Vector2f((screen.X - size.X) / 2f, (screen.Y - size.Y) / 2f);
        _bg.SetPosition(pos)
           .SetSize(size)
           .SetColor(new Color((System.Byte)PanelColorGray, (System.Byte)PanelColorGray, (System.Byte)PanelColorGray))
           .Layout();

        _font = Assets.Font.Load("1");

        _title = new Text("News", _font, (System.UInt32)TitleFontSize)
        {
            FillColor = Color.White,
            OutlineColor = new Color(0, 0, 0, 200),
            OutlineThickness = TitleOutlineThickness
        };

        _divTex = Assets.UiTextures.Load(dividerTexture);
        _divLeft = new Sprite(_divTex) { Scale = new Vector2f(0.5f, 0.5f) };
        _divRight = new Sprite(_divTex) { Scale = new Vector2f(-0.5f, 0.5f) };

        _content = new Text(
            "Game developed by:\n" +
            "- CHAT GPT - Github Copilot - PhcNguyen: Programming\n" +
            "- PhcNguyen: Art & Design\n" +
            "- PhcNguyen: Sound & Music\n" +
            "- PhcNguyen: Project Lead",
            _font, (System.UInt32)BodyFontSize)
        {
            FillColor = new Color(220, 220, 220),
            OutlineColor = new Color(0, 0, 0, 160),
            OutlineThickness = BodyOutlineThickness
        };

        _backBtn = new StretchableButton("Back", BackButtonWidth);
        _backBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
        _backBtn.RegisterClickHandler(() => BackRequested?.Invoke());

        SetZIndex(2);
        DoLayout();
    }

    public override void Update(System.Single dt)
    {
        _backBtn.Update(dt);
    }

    public override void Render(RenderTarget target)
    {
        _bg.Render(target);
        target.Draw(_title);
        target.Draw(_divLeft);
        target.Draw(_divRight);
        target.Draw(_content);
        _backBtn.Render(target);
    }

    protected override Drawable GetDrawable() => _title;

    // ===== Layout =====

    private void DoLayout()
    {
        LayoutTitle();
        LayoutDividers();
        LayoutBody();
        LayoutBackButton();
    }

    private void LayoutTitle()
    {
        var p = _bg.Position;
        var s = _bg.Size;
        var tb = _title.GetLocalBounds();

        System.Single titleX = p.X + (s.X - tb.Width) / 2f - tb.Left;
        System.Single titleY = p.Y + TitleTopPadding;
        _title.Position = new Vector2f(titleX, titleY);
    }

    private void LayoutDividers()
    {
        var p = _bg.Position;
        var s = _bg.Size;
        var tb = _title.GetLocalBounds();

        System.Single innerLeft = p.X + SidePadding;
        System.Single innerRight = p.X + s.X - SidePadding;
        System.Single leftAvail = _title.Position.X - GapTitleToDivider - innerLeft;
        System.Single rightAvail = innerRight - (_title.Position.X + tb.Width + GapTitleToDivider);

        System.Single baseW = System.MathF.Max(0f, System.MathF.Min(leftAvail, rightAvail));
        System.Single divTargetW = System.MathF.Max(0f, baseW - PanelSideTrim);
        System.Single sx = _divTex.Size.X > 0 ? divTargetW / _divTex.Size.X : 0f;

        _divLeft.Scale = new Vector2f(sx, 1f);
        _divRight.Scale = new Vector2f(-sx, 1f);

        System.Single divHeight = _divTex.Size.Y * 1f;
        System.Single midY = _title.Position.Y + (tb.Top + tb.Height) * 0.5f;
        System.Single divY = midY - divHeight * 0.5f;

        _divLeft.Position = new Vector2f(innerLeft + PanelSideTrim, divY);
        _divRight.Position = new Vector2f(innerRight - PanelSideTrim, divY);
    }

    private void LayoutBody()
    {
        var p = _bg.Position;
        var tb = _title.GetLocalBounds();

        System.Single bodyX = p.X + BodyLeftPadding;
        System.Single bodyY = _title.Position.Y + tb.Height + BodyTopGapFromTitle;
        _content.Position = new Vector2f(bodyX, bodyY);
    }

    private void LayoutBackButton()
    {
        var p = _bg.Position;
        var s = _bg.Size;
        var bb = _backBtn.GetGlobalBounds();
        System.Single x = p.X + (s.X - bb.Width) / 2f;
        System.Single y = p.Y + s.Y - bb.Height - BackBottomPadding;
        _backBtn.SetPosition(new Vector2f(x, y));
    }
}
