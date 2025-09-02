using Nalix.Rendering.Input;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Resources.Manager;
using Nalix.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Rendering.Runtime;

/// <summary>
/// The Game class serves as the entry point for managing the game window, rendering, and scene updates.
/// </summary>
public static class GameEngine
{
    // Private fields
    private static readonly RenderWindow _window;

    /// <summary>
    /// Indicates whether debugging mode is enabled.
    /// </summary>
    public static System.Boolean Debugging { get; private set; }

    /// <summary>
    /// Gets the dimensions (width and height) of the screen or viewport,
    /// used to set the screen size for rendering purposes.
    /// </summary>
    public static Vector2u ScreenSize { get; private set; }

    /// <summary>
    /// Provides access to the assembly configuration.
    /// </summary>
    public static GraphicsConfig GraphicsConfig { get; private set; }

    /// <summary>
    /// Static constructor to initialize the game configuration and window.
    /// </summary>
    static GameEngine()
    {
        GraphicsConfig = new GraphicsConfig();
        ScreenSize = new Vector2u(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);

        _window = new RenderWindow(
            new VideoMode(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight),
            GraphicsConfig.Title, Styles.Titlebar | Styles.Close
        );
        _window.Closed += (_, _) => _window.Close();
        _window.SetFramerateLimit(GraphicsConfig.FrameLimit);

        Effects.Camera.Camera2D.Initialize(ScreenSize);
    }

    /// <summary>
    /// Enables or disables debug mode.
    /// </summary>
    /// <param name="on">Set to true to enable debug mode, false to disable it.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void SetDebugMode(System.Boolean on) => Debugging = on;

    /// <summary>
    /// Sets the icon for the game window.
    /// </summary>
    /// <param name="image">The image to use as the window icon.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void SetIcon(Image image)
        => _window.SetIcon(image.Size.X, image.Size.Y, image.Pixels);

    /// <summary>
    /// Opens the game window and starts the main game loop.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void OpenWindow()
    {
        Clock clock = new();
        SceneManager.Instantiate();

        while (_window.IsOpen)
        {
            _window.DispatchEvents();
            System.Single deltaTime = clock.Restart().AsSeconds();
            Update(deltaTime);

            _window.Clear();
            Render(_window);

            _window.Display();
        }

        _window.Dispose();
    }

    public static void CloseWindow()
    {
        _window.Close();

        MusicManager.Dispose();
    }

    /// <summary>
    /// Updates all game components, including input, scene management, and scene objects.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update, in seconds.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Update(System.Single deltaTime)
    {
        InputState.Update(_window);
        SceneManager.ProcessLoadScene();
        SceneManager.ProcessDestroyQueue();
        SceneManager.ProcessSpawnQueue();
        SceneManager.UpdateSceneObjects(deltaTime);
        Effects.Camera.Camera2D.Update(deltaTime);
        Physics.CollisionManager.Update(deltaTime);
    }

    /// <summary>
    /// Renders all objects in the current scene, sorted by their Z-index.
    /// </summary>
    /// <param name="target">The render target.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Render(RenderTarget target)
    {
        Effects.Camera.Camera2D.Apply(target);
        System.Collections.Generic.List<RenderObject> renderObjects = [.. SceneManager.AllObjects<RenderObject>()];
        renderObjects.Sort(RenderObject.CompareByZIndex);
        foreach (RenderObject r in renderObjects)
        {
            if (r.Enabled && r.Visible)
            {
                r.Render(target);
            }
        }
    }
}