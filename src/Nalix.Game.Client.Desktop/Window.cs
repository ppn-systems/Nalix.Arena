using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Game.Client.Desktop;

internal class Window
{
    private readonly Clock _clock;
    private readonly GameScene _scene;
    private readonly RenderWindow _window;

    public Window()
    {
        // Initialize the window here if needed
        VideoMode mode = new(1280, 720); // HD resolution

        _clock = new Clock(); // Initialize the clock for delta time calculation
        _scene = new GameScene(); // Initialize your game scene here
        _window = new RenderWindow(mode, "Nalix");

        _window.SetFramerateLimit(60); // ✅ Limit FPS to 60

        _window.Closed += (_, _) => _window.Close();
        _window.KeyPressed += (_, e) =>
        {
            if (e.Code == Keyboard.Key.Escape)
                _window.Close();
            else
                _scene.HandleInput(e);
        };
    }

    public void GameLoop()
    {
        // Game loop
        while (_window.IsOpen)
        {
            _window.DispatchEvents();

            // precise delta time
            _scene.Update(_clock.Restart().AsSeconds());

            // TODO: draw game here
            _scene.Draw(_window);
            _window.Display();
        }
    }
}