using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

internal sealed class GameScene : SceneBase
{
    public override void Update(float deltaTime)
    {
        // Update game state (e.g., physics, player, network sync)
    }

    public override void Draw(RenderWindow window)
    {
        // Draw game objects
    }

    public override void HandleInput(KeyEventArgs e)
    {
        // Handle key press events
    }

    public override void HandleMouseInput(MouseButtonEventArgs e)
    {
    }
}