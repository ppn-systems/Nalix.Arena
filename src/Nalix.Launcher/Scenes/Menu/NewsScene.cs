// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Launcher.Objects.Controls;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Launcher.Scenes.Menu;

/// <summary>
/// Màn hình hiển thị thông tin về nhóm phát triển trò chơi.
/// </summary>
internal sealed class NewsScene : Scene
{
    public NewsScene() : base(SceneNames.News) { }

    protected override void LoadObjects()
    {
        AddObject(new ShutterBladesRevealEffect());
        AddObject(new NewsUi("divider/002"));
    }

    /// <summary>
    /// Vertical "shutter blades" reveal transition.
    /// A set of vertical panels covers the screen and then slides out
    /// alternately upward/downward with a small per-blade stagger (delay),
    /// gradually revealing the scene underneath.
    /// </summary>
    [IgnoredLoad("RenderObject")]
    public sealed class ShutterBladesRevealEffect : RenderObject
    {
        private readonly System.Collections.Generic.List<RectangleShape> _blades = new();

        // Timing
        private System.Single _t;                       // Global elapsed time
        private readonly System.Single _duration;       // Duration of the slide for each blade (seconds)
        private readonly System.Single _stagger;        // Delay between blades (seconds)

        // Visual config
        private readonly System.Int32 _bladeCount;
        private readonly Color _bladeColor;

        // Easing selector (can be swapped if you want a different feel)
        private readonly System.Func<System.Single, System.Single> _ease;

        /// <summary>
        /// Initializes the shutter blades reveal effect.
        /// </summary>
        /// <param name="bladeCount">Number of vertical blades (recommended: 6–16).</param>
        /// <param name="duration">Time for each blade to slide off (seconds).</param>
        /// <param name="stagger">Delay between each blade's start (seconds).</param>
        /// <param name="color">Color of the blades (mask).</param>
        public ShutterBladesRevealEffect(
            System.Int32 bladeCount = 10,
            System.Single duration = 0.9f,
            System.Single stagger = 0.06f,
            Color? color = null)
        {
            _bladeCount = System.Math.Max(2, bladeCount);
            _duration = System.Math.Max(0.05f, duration);
            _stagger = System.Math.Max(0f, stagger);
            _bladeColor = color ?? new Color(0, 0, 0, 255);

            // Cubic ease-out
            _ease = t =>
            {
                t = System.Math.Clamp(t, 0f, 1f);
                return 1f - System.MathF.Pow(1f - t, 3f);
            };

            // Create blade shapes (position/size is finalized during Update)
            for (System.Int32 i = 0; i < _bladeCount; i++)
            {
                var blade = new RectangleShape
                {
                    FillColor = _bladeColor
                };
                _blades.Add(blade);
            }

            // Ensure this effect renders on top
            SetZIndex(9999);
        }

        /// <summary>
        /// Not used because we override <see cref="Render(RenderTarget)"/>.
        /// </summary>
        protected override Drawable GetDrawable() => _blades[0];

        public override void Update(System.Single deltaTime)
        {
            _t += deltaTime;

            // Read current screen size (assumes GraphicsEngine.ScreenSize is valid)
            var screen = GraphicsEngine.ScreenSize; // Vector2u
            System.Single sw = screen.X;
            System.Single sh = screen.Y;

            if (sw <= 0f || sh <= 0f)
            {
                // Edge case: no size, keep covered
                return;
            }

            System.Single bladeWidth = sw / _bladeCount;

            System.Boolean allFinished = true;

            for (System.Int32 i = 0; i < _bladeCount; i++)
            {
                var blade = _blades[i];

                // Initial coverage: each blade covers full height, sits at y = 0
                blade.Size = new Vector2f(bladeWidth + 1f, sh + 1f); // +1 to avoid hairline gaps
                blade.Position = new Vector2f(i * bladeWidth, 0f);

                // Per-blade local time with stagger
                System.Single localStart = i * _stagger;
                System.Single localT = (_t - localStart) / _duration;

                // Progress (0..1) with easing; negative means not started yet
                System.Single eased = localT <= 0f ? 0f : _ease(localT);

                // Direction: even index moves down, odd moves up
                System.Boolean moveDown = (i % 2) == 0;

                // Compute Y offset based on eased progress
                System.Single targetY = moveDown ? sh : -sh; // slide completely out of view
                System.Single yOffset = targetY * eased;

                // Apply offset
                blade.Position = new Vector2f(i * bladeWidth, yOffset);

                // If any blade hasn't finished (localT < 1), keep the effect alive
                if (localT < 1f)
                {
                    allFinished = false;
                }
            }

            if (allFinished)
            {
                Destroy();
            }
        }

        public override void Render(RenderTarget target)
        {
            // Draw all blades on top of the scene
            for (System.Int32 i = 0; i < _blades.Count; i++)
            {
                target.Draw(_blades[i]);
            }
        }
    }


    [IgnoredLoad("RenderObject")]
    private sealed class NewsUi : RenderObject
    {
        #region Constants

        private const System.Single PanelWidthRatio = 0.95f;
        private const System.Single PanelHeightRatio = 0.9f;
        private const System.Single PanelColorGray = 40f;
        private const System.Single TitleFontSize = 32f;
        private const System.Single BodyFontSize = 20f;
        private const System.Single TitleTopPadding = 24f;
        private const System.Single SidePadding = 24f;        // lề trong panel cho divider
        private const System.Single GapTitleToDivider = 12f;  // khoảng cách chữ <-> divider
        private const System.Single PanelSideTrim = 32f;      // rút ngắn divider về phía panel
        private const System.Single BodyLeftPadding = 40f;
        private const System.Single BodyTopGapFromTitle = 28f;
        private const System.Single BackBottomPadding = 28f;
        private const System.Single TitleOutlineThickness = 2f;
        private const System.Single BodyOutlineThickness = 1.5f;
        private const System.Single BackButtonWidth = 200f;

        #endregion Constants

        #region Fields

        private readonly NineSlicePanel _bg;
        private readonly Text _title;
        private readonly Text _teamInfo;
        private readonly StretchableButton _backBtn;
        private readonly Sprite _divLeft, _divRight;
        private readonly Texture _divTex;
        private readonly Font _font;

        #endregion Fields

        public NewsUi(System.String texture = "divider/002")
        {
            // Build phase
            _bg = BuildBackground();
            _font = Assets.Font.Load("1");
            _title = BuildTitle(_font);
            _divTex = Assets.UiTextures.Load(texture);
            (_divLeft, _divRight) = BuildDividers(_divTex);
            _teamInfo = BuildTeamInfo(_font);
            _backBtn = BuildBackButton();

            // Wire events
            WireButtonHandlers(_backBtn);

            // Initial layout
            DoLayout();
        }


        #region Render

        public override void Update(System.Single deltaTime) => _backBtn.Update(deltaTime);

        public override void Render(RenderTarget target)
        {
            _bg.Render(target);
            target.Draw(_title);
            target.Draw(_divLeft);
            target.Draw(_divRight);
            target.Draw(_teamInfo);
            _backBtn.Render(target);
        }

        protected override Drawable GetDrawable() => null;

        #endregion Render

        #region Build helpers

        private static NineSlicePanel BuildBackground()
        {
            var tex = Assets.UiTextures.Load("panels/031");
            var panel = new NineSlicePanel(tex, new Thickness(32));

            Vector2u screen = GraphicsEngine.ScreenSize;
            Vector2f size = new(screen.X * PanelWidthRatio, screen.Y * PanelHeightRatio);
            Vector2f pos = new((screen.X - size.X) / 2f, (screen.Y - size.Y) / 2f);

            panel.SetPosition(pos)
                 .SetSize(size)
                 .SetColor(new Color((System.Byte)PanelColorGray, (System.Byte)PanelColorGray, (System.Byte)PanelColorGray))
                 .Layout();

            return panel;
        }

        private static Text BuildTitle(Font font)
            => new("News", font, (System.UInt32)TitleFontSize)
            {
                FillColor = Color.White,
                OutlineColor = new Color(0, 0, 0, 200),
                OutlineThickness = TitleOutlineThickness
            };

        private static (Sprite left, Sprite right) BuildDividers(Texture tex)
        {
            var left = new Sprite(tex) { Scale = new Vector2f(0.5f, 0.5f) };
            var right = new Sprite(tex) { Scale = new Vector2f(-0.5f, 0.5f) }; // mirror X
            return (left, right);
        }

        private static Text BuildTeamInfo(Font font)
            => new(
                "Game developed by:\n" +
                "- CHAT GPT - Github Copilot - PhcNguyen: Programming\n" +
                "- PhcNguyen: Art & Design\n" +
                "- PhcNguyen: Sound & Music\n" +
                "- PhcNguyen: Project Lead",
                font, (System.UInt32)BodyFontSize)
            {
                FillColor = new Color(220, 220, 220),
                OutlineColor = new Color(0, 0, 0, 160),
                OutlineThickness = BodyOutlineThickness,
            };

        private static StretchableButton BuildBackButton()
            => new("Back", BackButtonWidth);

        private static void WireButtonHandlers(StretchableButton btn)
        {
            btn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            btn.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Main));
        }

        // =======================
        // Layout orchestrator
        // =======================
        private void DoLayout()
        {
            LayoutTitle();
            LayoutDividers();
            LayoutBody();
            LayoutBackButton();
        }

        // =======================
        // Layout helpers
        // =======================
        private void LayoutTitle()
        {
            var p = _bg.Position; // top-left of panel
            var s = _bg.Size;     // size of panel
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

            System.Single titleX = _title.Position.X;
            System.Single titleY = _title.Position.Y;

            // Mép trái/phải trong panel
            System.Single innerLeft = p.X + SidePadding;
            System.Single innerRight = p.X + s.X - SidePadding;

            // Khoảng trống tới chữ (không đụng chữ)
            System.Single leftAvail = titleX - GapTitleToDivider - innerLeft;
            System.Single rightAvail = innerRight - (titleX + tb.Width + GapTitleToDivider);

            // Chiều rộng chia đều (tối đa không vượt chữ)
            System.Single baseW = System.MathF.Max(0f, System.MathF.Min(leftAvail, rightAvail));
            System.Single divTargetW = System.MathF.Max(0f, baseW - PanelSideTrim);

            // Scale theo texture
            System.Single sx = _divTex.Size.X > 0 ? divTargetW / _divTex.Size.X : 0f;
            const System.Single sy = 1f;

            _divLeft.Scale = new Vector2f(sx, sy);
            _divRight.Scale = new Vector2f(-sx, sy); // mirror X

            // Căn Y theo giữa của chữ
            System.Single divHeight = _divTex.Size.Y * sy;
            System.Single midY = titleY + (tb.Top + tb.Height) * 0.5f;
            System.Single divY = midY - divHeight * 0.5f;

            // Đặt vị trí: neo về phía panel và trim vào trong
            _divLeft.Position = new Vector2f(innerLeft + PanelSideTrim, divY);
            _divRight.Position = new Vector2f(innerRight - PanelSideTrim, divY);
        }

        private void LayoutBody()
        {
            var p = _bg.Position;
            var tb = _title.GetLocalBounds();

            // Đặt nội dung ngay dưới title 1 khoảng
            System.Single bodyX = p.X + BodyLeftPadding;
            System.Single bodyY = _title.Position.Y + tb.Height + BodyTopGapFromTitle;
            _teamInfo.Position = new Vector2f(bodyX, bodyY);
        }

        private void LayoutBackButton()
        {
            var p = _bg.Position;
            var s = _bg.Size;

            var bb = _backBtn.GetGlobalBounds(); // dùng để căn giữa ngang & cách đáy
            System.Single x = p.X + (s.X - bb.Width) / 2f;
            System.Single y = p.Y + s.Y - bb.Height - BackBottomPadding;

            _backBtn.SetPosition(new Vector2f(x, y));
        }

        #endregion Build helpers
    }
}
