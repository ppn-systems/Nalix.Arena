using Nalix.Game.Client.Desktop.Content;
using Nalix.Game.Client.Desktop.Core;
using Nalix.Game.Client.Desktop.Graphics;
using Nalix.Game.Client.Desktop.Graphics.Parallax;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace Nalix.Game.Client.Desktop.Scene;

internal class MainMenuScene : IScene
{
    private readonly List<ParallaxLayer> _layers;
    private readonly Sprite _settingSprite;
    private readonly Texture _settingTexture;

    public MainMenuScene()
    {
        _layers =
        [
            new ParallaxLayer(string.Format(TexturePath.Background, 7), 0f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 6), 25f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 5), 30f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 4), 35f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 3), 40f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 2), 45f, true),
            new ParallaxLayer(string.Format(TexturePath.Background, 1), 50f, true)
        ];

        Texture texture = new(string.Format(TexturePath.UI, 1));

        ImageCutter image = new(texture, 16, 16);

        // Cắt icon hàng 6, cột 3 (dòng thứ 6, cột thứ 3 - tính từ 0)
        _settingSprite = image.CutIconAt(3, 5); // (column, row) → row = 5 vì bắt đầu từ 0
        _settingSprite.Scale = new SFML.System.Vector2f(3.5f, 3.5f);

        // Tính toán lại kích thước thật sau khi scale
        FloatRect bounds = _settingSprite.GetGlobalBounds();

        // Đặt vị trí ở góc trên bên phải (cách mép phải và mép trên 20px)
        _settingSprite.Position = new SFML.System.Vector2f(
            WindowHost.Width - bounds.Width - 20,
            20
        );
    }

    public void Update(float deltaTime)
    {
        foreach (ParallaxLayer layer in _layers)
            layer.Update(deltaTime);
    }

    public void Draw(RenderWindow window)
    {
        foreach (ParallaxLayer layer in _layers) layer.Draw(window);

        window.Draw(_settingSprite);
    }

    public void HandleInput(KeyEventArgs e)
    {
        if (e.Code == Keyboard.Key.S)
        {
            SceneHost.SwitchTo(new SettingsScene()); // Chuyển sang scene Settings
        }
    }

    public void HandleMouseInput(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left)
        {
            var mousePos = Mouse.GetPosition(WindowHost.Window); // hoặc window
            var bounds = _settingSprite.GetGlobalBounds();

            if (bounds.Contains(mousePos.X, mousePos.Y))
            {
                SceneHost.SwitchTo(new SettingsScene()); // Chuyển sang scene Settings
            }
        }
    }
}