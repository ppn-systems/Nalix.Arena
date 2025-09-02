using Nalix.Rendering.Attributes;
using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Runtime;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
    protected override void LoadObjects()
    {
        AddObject(new Background());
        AddObject(new Banner());      // trước đây chưa Add — giờ add để hiển thị
        AddObject(new CloseIcon());
    }

    #endregion

    #region Private types

    /// <summary>Nền mờ phủ toàn màn hình.</summary>
    [IgnoredLoad("RenderObject")]
    private class Background : RenderObject
    {
        #region Config

        private const System.String BgTextureKey = "bg/0";
        private const System.Byte BackgroundAlpha = 180; // mờ nhẹ

        #endregion

        #region Fields

        private readonly Sprite _background;

        #endregion

        #region Ctor

        public Background()
        {
            SetZIndex(0); // Hiển thị phía sau
            _background = BuildBackgroundSprite();
        }

        #endregion

        #region Build helpers

        private static Sprite BuildBackgroundSprite()
        {
            Texture bg = Assets.UiTextures.Load(BgTextureKey);
            System.Single scaleX = (System.Single)GameEngine.ScreenSize.X / bg.Size.X;
            System.Single scaleY = (System.Single)GameEngine.ScreenSize.Y / bg.Size.Y;

            return new Sprite(bg)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(scaleX, scaleY),
                Color = new Color(255, 255, 255, BackgroundAlpha)
            };
        }

        #endregion

        #region Render loop

        public override void Render(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            target.Draw(_background);
        }

        protected override Drawable GetDrawable()
            => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

        #endregion
    }

    /// <summary>Banner trung tâm hiển thị trên màn hình thiết lập.</summary>
    [IgnoredLoad("RenderObject")]
    private class Banner : RenderObject
    {
        #region Config

        private const System.String PanelTextureKey = "panels/007";
        private const System.Single ScaleXFactor = 2f;
        private const System.Single ScaleYFactor = 1.2f;

        #endregion

        #region Fields

        private readonly Sprite _banner;

        #endregion

        #region Public props

        /// <summary>Vị trí hiển thị của panel.</summary>
        public Vector2f PanelPosition => _banner.Position;

        /// <summary>Kích thước hiển thị sau scale.</summary>
        public Vector2f PanelSize => new(
            _banner.Texture.Size.X * _banner.Scale.X,
            _banner.Texture.Size.Y * _banner.Scale.Y);

        #endregion

        #region Ctor

        public Banner()
        {
            SetZIndex(2);
            _banner = BuildBannerSprite();
        }

        #endregion

        #region Build helpers

        private static Sprite BuildBannerSprite()
        {
            Texture panel = Assets.UiTextures.Load(PanelTextureKey);

            System.Single scaleFit = System.Math.Min(
                (System.Single)GameEngine.ScreenSize.X / panel.Size.X,
                (System.Single)GameEngine.ScreenSize.Y / panel.Size.Y);

            Vector2f scale = new(scaleFit * ScaleXFactor, scaleFit * ScaleYFactor);

            System.Single posX = (GameEngine.ScreenSize.X - (panel.Size.X * scale.X)) / 2f;
            System.Single posY = (GameEngine.ScreenSize.Y - (panel.Size.Y * scale.Y)) / 2f;

            return new Sprite(panel)
            {
                Scale = scale,
                Position = new Vector2f(posX, posY)
            };
        }

        #endregion

        #region Render

        protected override Drawable GetDrawable() => _banner;

        #endregion
    }

    /// <summary>Biểu tượng đóng (góc trên phải) để quay lại màn hình chính.</summary>
    [IgnoredLoad("RenderObject")]
    internal class CloseIcon : RenderObject
    {
        #region Config

        private const System.String IconTextureKey = "icons/2";
        private const System.Single IconScale = 0.6f;
        private const System.Single PaddingTop = 5f;
        private const System.Single PaddingRight = 0f; // căn sát phải, đã trừ theo width

        #endregion

        #region Fields

        private readonly Sprite _icon;

        #endregion

        #region Ctor

        public CloseIcon()
        {
            SetZIndex(2);
            _icon = BuildIconSprite();
            PlaceTopRight();
        }

        #endregion

        #region Build & layout

        private static Sprite BuildIconSprite()
        {
            Texture texture = Assets.UiTextures.Load(IconTextureKey);
            return new Sprite(texture)
            {
                Scale = new Vector2f(IconScale, IconScale),
            };
        }

        private void PlaceTopRight()
        {
            FloatRect bounds = _icon.GetGlobalBounds();
            System.Single x = GameEngine.ScreenSize.X - bounds.Width - PaddingRight;
            _icon.Position = new Vector2f(x, PaddingTop);
        }

        #endregion

        #region Input helpers

        private System.Boolean IsMouseOver()
            => _icon.GetGlobalBounds().Contains(InputState.GetMousePosition());

        #endregion

        #region Render loop

        public override void Update(System.Single deltaTime)
        {
            if (!Visible)
            {
                return;
            }

            // Click chuột
            if (InputState.IsMouseButtonPressed(Mouse.Button.Left) && IsMouseOver())
            {
                Assets.Sfx.Play("1");
                SceneManager.ChangeScene(SceneNames.Main);
            }

            // Thoát bằng phím Esc (tiện UX)
            if (InputState.IsKeyPressed(Keyboard.Key.Escape))
            {
                Assets.Sfx.Play("1");
                SceneManager.ChangeScene(SceneNames.Main);
            }
        }

        protected override Drawable GetDrawable() => _icon;

        #endregion
    }

    #endregion Private types
}
