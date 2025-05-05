using Nalix.Game.Client.Desktop.Content;
using Nalix.Game.Client.Desktop.Graphics.Parallax;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace Nalix.Game.Client.Desktop.Scene;

internal class MainMenuScene : IScene
{
    private readonly List<ParallaxLayer> _layers;

    public MainMenuScene()
    {
        _layers =
        [
            new ParallaxLayer(string.Format(TexturePath.Background, 7), 0f, true),    // 7.png, background đứng yên
            new ParallaxLayer(string.Format(TexturePath.Background, 6), 30f, true),   // 6.png, tốc độ di chuyển 10
            new ParallaxLayer(string.Format(TexturePath.Background, 5), 35f, true),   // 5.png, tốc độ di chuyển 15
            new ParallaxLayer(string.Format(TexturePath.Background, 4), 40f, true),   // 4.png, tốc độ di chuyển 20
            new ParallaxLayer(string.Format(TexturePath.Background, 3), 45f, true),   // 3.png, tốc độ di chuyển 30
            new ParallaxLayer(string.Format(TexturePath.Background, 2), 50f, true),   // 2.png, tốc độ di chuyển 40
            new ParallaxLayer(string.Format(TexturePath.Background, 1), 50f, true)    // 1.png, tốc độ di chuyển 50
        ];
    }

    public void Update(float deltaTime)
    {
        foreach (ParallaxLayer layer in _layers)
            layer.Update(deltaTime);
    }

    public void Draw(RenderWindow window)
    {
        foreach (ParallaxLayer layer in _layers) layer.Draw(window);
    }

    public void HandleInput(KeyEventArgs e)
    {
        if (e.Code == Keyboard.Key.Enter)
        {
            SceneManager.SwitchTo(new GameScene());
        }
    }

    public void HandleMouseInput(MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left)
        {
            SceneManager.SwitchTo(new GameScene());
        }
    }
}