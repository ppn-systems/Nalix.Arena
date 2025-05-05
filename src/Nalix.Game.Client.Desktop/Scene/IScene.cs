using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

internal interface IScene
{
    void Update(float deltaTime);

    void Draw(RenderWindow window);

    void HandleInput(KeyEventArgs e);

    void HandleMouseInput(MouseButtonEventArgs e);
}