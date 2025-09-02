using Nalix.Client.Objects.Controls;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Effects.Visual;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Client.Scenes.Menu;

/// <summary>
/// Màn hình hiển thị thông tin về nhóm phát triển trò chơi.
/// </summary>
internal sealed class CreditsScene : Scene
{
    public CreditsScene() : base(SceneNames.Credits) { }

    protected override void LoadObjects() => AddObject(new CreditsUi());

    [IgnoredLoad("RenderObject")]
    private sealed class CreditsUi : RenderObject
    {
        private readonly NineSlicePanel _bg;
        private readonly Text _title;
        private readonly Text _teamInfo;
        private readonly StretchableButton _backBtn;

        private readonly Sprite _divLeft, _divRight;
        private readonly Texture _divTex;

        private FloatRect _bounds;

        public CreditsUi()
        {
            var tex = Assets.UiTextures.Load("panels/031");
            _bg = new NineSlicePanel(tex, new Thickness(32));

            // Đặt kích thước khung ~ 70% màn hình
            Vector2u screen = GameEngine.ScreenSize;
            Vector2f size = new(screen.X * 0.7f, screen.Y * 0.6f);
            Vector2f pos = new((screen.X - size.X) / 2f, (screen.Y - size.Y) / 2f);
            _bg.SetPosition(pos).SetSize(size).SetColor(new Color(40, 40, 40)).Layout();

            _bounds = new FloatRect(pos.X, pos.Y, size.X, size.Y);

            var font = Assets.Font.Load("1");

            _title = new Text("Credits", font, 32)
            {
                FillColor = Color.White,
                OutlineColor = new Color(0, 0, 0, 200),
                OutlineThickness = 2f
            };
            // căn giữa tiêu đề theo khung
            FloatRect tb = _title.GetLocalBounds();
            _title.Position = new Vector2f(
                pos.X + ((size.X - tb.Width) / 2f) - tb.Left,
                pos.Y + 20f
            );

            _divTex = Assets.UiTextures.Load("divider/002");
            _divLeft = new Sprite(_divTex) { Scale = new Vector2f(0.5f, 0.5f) };
            _divRight = new Sprite(_divTex) { Scale = new Vector2f(-0.5f, 0.5f) }; // mirror X


            _teamInfo = new Text(
                "Game developed by:\n" +
                "- CHAT GPT - Github Copilot - PhcNguyen: Programming\n" +
                "- PhcNguyen: Art & Design\n" +
                "- PhcNguyen: Sound & Music\n" +
                "- PhcNguyen: Project Lead",
                font, 20)
            {
                FillColor = new Color(220, 220, 220),
                OutlineColor = new Color(0, 0, 0, 160),
                OutlineThickness = 1.5f,
                Position = new Vector2f(pos.X + 40f, pos.Y + 80f)
            };

            _backBtn = new StretchableButton("Back", 200f);
            FloatRect bb = _backBtn.GetGlobalBounds();
            _backBtn.SetPosition(new Vector2f(
                pos.X + ((size.X - bb.Width) / 2f),
                pos.Y + size.Y - bb.Height - 30f
            ));

            _backBtn.RegisterClickHandler(static () => Assets.Sfx.Play("1"));
            _backBtn.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Main));

            DoLayout();
        }

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

        private void DoLayout()
        {
            // Căn title vào trong khung
            var p = _bg.Position;
            var s = _bg.Size;
            var tb = _title.GetLocalBounds();

            System.Single titleX = p.X + ((s.X - tb.Width) / 2f) - tb.Left;
            System.Single titleY = p.Y + 24f;
            _title.Position = new Vector2f(titleX, titleY);

            // === Divider 2 bên ===
            const System.Single sidePadding = 24f;   // lề trong panel
            const System.Single gapToText = 12f;   // khoảng cách chữ <-> divider (giữ nguyên)
            const System.Single panelSideTrim = 32f;  // bớt về phía panel (chỉnh số này để ngắn/dài)

            // Mép trái & phải (bên trong panel)
            System.Single innerLeft = p.X + sidePadding;
            System.Single innerRight = p.X + s.X - sidePadding;

            // Khoảng trống ban đầu tới chữ (không đụng)
            System.Single leftAvail = titleX - gapToText - innerLeft;
            System.Single rightAvail = innerRight - (titleX + tb.Width + gapToText);

            // Chiều rộng mỗi divider nếu chạm panel
            System.Single baseW = System.MathF.Max(0f, System.MathF.Min(leftAvail, rightAvail));

            // NGẮN LẠI VỀ PHÍA PANEL: bớt width và DỊCH neo sát panel vào trong đúng trim
            System.Single divTargetW = System.MathF.Max(0f, baseW - panelSideTrim);

            // Scale ngang theo texture; scale dọc nếu muốn mảnh hơn
            System.Single sx = (_divTex.Size.X > 0) ? (divTargetW / _divTex.Size.X) : 0f;
            System.Single sy = 1f; // ví dụ muốn mỏng hơn: 0.7f

            _divLeft.Scale = new Vector2f(sx, sy);
            _divRight.Scale = new Vector2f(-sx, sy); // mirror X

            // Căn theo trục Y: lấy giữa chữ, dùng chiều cao sau scale
            System.Single divHeight = _divTex.Size.Y * sy;
            System.Single midY = titleY + ((tb.Top + tb.Height) * 0.5f);
            System.Single divY = midY - (divHeight * 0.5f);

            // Vị trí:
            // Left: neo phía panel -> dời vào trong 'panelSideTrim'
            _divLeft.Position = new Vector2f(innerLeft + panelSideTrim, divY);

            // Right: neo phía panel -> dời vào trong 'panelSideTrim'
            // Lưu ý: Scale.X âm => Position là mép phải
            _divRight.Position = new Vector2f(innerRight - panelSideTrim, divY);

            // Text nội dung bên trong khung
            _teamInfo.Position = new Vector2f(p.X + 40f, titleY + tb.Height + 28f);

            // Nút Back
            var bb = _backBtn.GetGlobalBounds();
            _backBtn.SetPosition(new Vector2f(
                p.X + ((s.X - bb.Width) / 2f),
                p.Y + s.Y - bb.Height - 28f
            ));
        }
    }
}
