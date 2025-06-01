using Nalix.Graphics;
using Nalix.Graphics.Rendering.Object;
using Nalix.Graphics.Scenes;
using SFML.Graphics;
using SFML.System;
using System;

namespace Nalix.Game.Presentation.Scenes;

internal class LoadingScene : Scene
{
    public LoadingScene() : base(SceneNames.Loading)
    {
    }

    protected override void LoadObjects()
    {
        base.AddObject(new LoadingMenu());
    }

    [IgnoredLoad("RenderObject")]
    public class LoadingMenu : RenderObject
    {
        private float _angle;
        private readonly Sprite _iconSprite;
        private readonly RectangleShape _background;

        public LoadingMenu()
        {
            base.SetZIndex(1);

            _background = new RectangleShape((Vector2f)GameEngine.ScreenSize)
            {
                FillColor = Color.Black,
                Position = new Vector2f(0, 0)
            };

            // Tải icon
            Texture iconTexture = Assets.UI.Load("icons/15.png");
            _iconSprite = new Sprite(iconTexture)
            {
                Origin = new Vector2f(iconTexture.Size.X / 2f, iconTexture.Size.Y / 2f),
                Position = new Vector2f(GameEngine.ScreenSize.X / 2f, GameEngine.ScreenSize.Y / 2f),
                Scale = new Vector2f(0.5f, 0.5f) // scale nếu cần
            };
        }

        public override void Update(float deltaTime)
        {
            _angle += deltaTime * 200f;
            _iconSprite.Rotation = _angle;
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;

            target.Draw(_background);
            target.Draw(_iconSprite);
        }

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }
}