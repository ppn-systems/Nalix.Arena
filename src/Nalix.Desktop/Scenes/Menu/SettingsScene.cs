// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Desktop.Objects.Controls;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Desktop.Scenes.Menu;

/// <summary>
/// Cảnh thiết lập trò chơi, nơi người dùng có thể thực hiện các tùy chỉnh.
/// </summary>
public class SettingsScene : Scene
{
    #region Ctor

    /// <summary>Khởi tạo cảnh thiết lập với tên từ <see cref="SceneNames.Settings"/>.</summary>
    public SettingsScene() : base(SceneNames.Settings) { }

    #endregion

    #region Scene lifecycle

    /// <summary>Tải các đối tượng của cảnh thiết lập như nền, banner và biểu tượng đóng.</summary>
    protected override void LoadObjects() => AddObject(new SettingsUi());

    #endregion

    #region Private types

    [IgnoredLoad("RenderObject")]
    private sealed class SettingsUi : RenderObject
    {
        #region Constants

        private const System.Single PanelWidthRatio = 0.7f;
        private const System.Single PanelHeightRatio = 0.6f;
        private const System.Single PanelColorGray = 40f;
        private const System.Single TitleFontSize = 32f;
        private const System.Single TitleTopPadding = 24f;
        private const System.Single SidePadding = 24f;        // lề trong panel cho divider
        private const System.Single GapTitleToDivider = 12f;  // khoảng cách chữ <-> divider
        private const System.Single PanelSideTrim = 32f;      // rút ngắn divider về phía panel
        private const System.Single BackBottomPadding = 28f;
        private const System.Single TitleOutlineThickness = 2f;
        private const System.Single BackButtonWidth = 200f;

        #endregion Constants

        #region Fields

        private readonly NineSlicePanel _bg;
        private readonly Text _title;
        private readonly StretchableButton _backBtn;
        private readonly Sprite _divLeft, _divRight;
        private readonly Texture _divTex;
        private readonly Font _font;

        #endregion Fields

        public SettingsUi(System.String texture = "divider/002")
        {
            // Build phase
            _bg = BuildBackground();
            _font = Assets.Font.Load("1");
            _title = BuildTitle(_font);
            _divTex = Assets.UiTextures.Load(texture);
            (_divLeft, _divRight) = BuildDividers(_divTex);
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
            _backBtn.Render(target);
        }

        protected override Drawable GetDrawable() => null;

        #endregion Render

        #region Build helpers

        private static NineSlicePanel BuildBackground()
        {
            var tex = Assets.UiTextures.Load("panels/031");
            var panel = new NineSlicePanel(tex, new Thickness(32));

            Vector2u screen = GameEngine.ScreenSize;
            Vector2f size = new(screen.X * PanelWidthRatio, screen.Y * PanelHeightRatio);
            Vector2f pos = new((screen.X - size.X) / 2f, (screen.Y - size.Y) / 2f);

            panel.SetPosition(pos)
                 .SetSize(size)
                 .SetColor(new Color((System.Byte)PanelColorGray, (System.Byte)PanelColorGray, (System.Byte)PanelColorGray))
                 .Layout();

            return panel;
        }

        private static Text BuildTitle(Font font)
            => new("Settings", font, (System.UInt32)TitleFontSize)
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

            System.Single titleX = p.X + ((s.X - tb.Width) / 2f) - tb.Left;
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
            System.Single midY = titleY + ((tb.Top + tb.Height) * 0.5f);
            System.Single divY = midY - (divHeight * 0.5f);

            // Đặt vị trí: neo về phía panel và trim vào trong
            _divLeft.Position = new Vector2f(innerLeft + PanelSideTrim, divY);
            _divRight.Position = new Vector2f(innerRight - PanelSideTrim, divY);
        }

        private void LayoutBackButton()
        {
            var p = _bg.Position;
            var s = _bg.Size;

            var bb = _backBtn.GetGlobalBounds(); // dùng để căn giữa ngang & cách đáy
            System.Single x = p.X + ((s.X - bb.Width) / 2f);
            System.Single y = p.Y + s.Y - bb.Height - BackBottomPadding;

            _backBtn.SetPosition(new Vector2f(x, y));
        }

        #endregion Build helpers
    }

    #endregion Private types
}
