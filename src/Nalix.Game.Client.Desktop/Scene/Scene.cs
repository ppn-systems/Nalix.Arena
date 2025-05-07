using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

/// <summary>
/// Base class that implements IScene with default logic.
/// Override what you need in your derived scenes.
/// </summary>
internal abstract class Scene : IScene
{
    public virtual void OnEnter()
    {
        // Called when the scene becomes active.
        // You can override this to load resources or initialize logic.
    }

    public virtual void OnExit()
    {
        // Called when the scene is no longer active.
        // You can override this to dispose assets or reset state.
    }

    public virtual void Update(float deltaTime)
    {
        // Called every frame to update game logic.
    }

    public virtual void Draw(RenderWindow window)
    {
        // Called every frame to render.
    }

    public virtual void HandleInput(KeyEventArgs e)
    {
        // Called when a key is pressed.
    }

    public virtual void HandleMouseInput(MouseButtonEventArgs e)
    {
        // Called when a mouse button is clicked.
    }
}