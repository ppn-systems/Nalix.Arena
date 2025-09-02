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
            _backBtn.RegisterClickHandler(() => SceneManager.ChangeScene(SceneNames.Main));
        }

        public override void Update(System.Single deltaTime) => _backBtn.Update(deltaTime);

        public override void Render(RenderTarget target)
        {
            _bg.Render(target);
            target.Draw(_title);
            target.Draw(_teamInfo);
            _backBtn.Render(target);
        }

        protected override Drawable GetDrawable() => null;
    }
}
