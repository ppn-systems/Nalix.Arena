using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

internal class GameScene : IScene
{
    public void Update(float deltaTime)
    {
        // Update game state (e.g., physics, player, network sync)
    }

    public void Draw(RenderWindow window)
    {
        // Draw game objects
    }

    public void HandleInput(KeyEventArgs e)
    {
        // Handle key press events
    }

    public void HandleMouseInput(MouseButtonEventArgs e)
    {
    }
}