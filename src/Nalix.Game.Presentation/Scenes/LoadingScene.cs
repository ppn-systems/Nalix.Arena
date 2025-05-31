using Nalix.Game.Presentation.Utils;
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
        private readonly ArcShape _arc;
        private float _angle;

        public LoadingMenu()
        {
            base.SetZIndex(1);

            // vẽ cung tròn từ 20 đến 340 độ
            _arc = new ArcShape(30, 20, 340, 30, new Color(100, 220, 220))
            {
                Position = new Vector2f(GameEngine.ScreenSize.X / 2, GameEngine.ScreenSize.Y / 2)
            };
        }

        public override void Update(float deltaTime)
        {
            _angle += deltaTime * 200;
        }

        public override void Render(RenderTarget target)
        {
            if (!Visible) return;
            var states = new RenderStates { Transform = Transform.Identity };
            states.Transform.Rotate(_angle, _arc.Position);
            _arc.Draw(target, states);
        }

        protected override Drawable GetDrawable()
            => throw new NotSupportedException("Use Render() instead of GetDrawable().");
    }
}