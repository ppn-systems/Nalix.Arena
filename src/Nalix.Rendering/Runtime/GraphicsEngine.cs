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
public static class GraphicsEngine
{
    // Private fields
    internal static readonly RenderWindow _window;
    internal static readonly System.UInt32 _foregroundFps;
    internal static readonly System.UInt32 _backgroundFps;

    private static System.Boolean _focused = true;

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
    static GraphicsEngine()
    {
        GraphicsConfig = new GraphicsConfig();
        ScreenSize = new Vector2u(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);

        _foregroundFps = (System.UInt32)(GraphicsConfig.FrameLimit > 0 ? GraphicsConfig.FrameLimit : 60);
        _backgroundFps = 15;

        ContextSettings ctx = new()
        {
            AntialiasingLevel = 0,
            DepthBits = 0,
            StencilBits = 0
        };

        _window = new RenderWindow(
                    new VideoMode(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight),
                    GraphicsConfig.Title,
                    Styles.Titlebar | Styles.Close,
                    ctx);

        // Window events
        _window.Closed += (_, _) => _window.Close();
        _window.GainedFocus += (_, _) => SetFocus(true);
        _window.LostFocus += (_, _) => SetFocus(false);
        _window.Resized += (_, e) => ScreenSize = new Vector2u(e.Width, e.Height);

        // Limit mode: prefer VSync for idle UI; don't enable both
        if (GraphicsConfig.VSync)
        {
            _window.SetVerticalSyncEnabled(true);
        }
        else
        {
            _window.SetFramerateLimit(_foregroundFps);
        }
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
        var clock = new Clock();
        SceneManager.Instantiate();

        // Fixed step (ổn định logic), vẫn render mỗi vòng
        const System.Single targetDt = 1f / 60f;
        System.Single accumulator = 0f;

        while (_window.IsOpen)
        {
            // Event pump
            _window.DispatchEvents();

            // Timing
            System.Single frameDt = clock.Restart().AsSeconds();
            // Clamp để tránh spike quá lớn khi alt-tab
            if (frameDt > 0.25f)
            {
                frameDt = 0.25f;
            }

            accumulator += frameDt;
            while (accumulator >= targetDt)
            {
                Update(targetDt);
                accumulator -= targetDt;
            }

            // Render
            _window.Clear();
            Render(_window);
            _window.Display();

            // Throttle nhẹ khi nền (mất focus) để hạ GPU/CPU
            if (!_focused)
            {
                // Nếu VSync OFF: giảm FPS nền.
                if (!GraphicsConfig.VSync)
                {
                    _window.SetFramerateLimit(_backgroundFps);
                }

                // Nhường CPU 1–3ms là đủ
                System.Threading.Thread.Sleep(2);
            }
            else
            {
                // Restore foreground FPS khi có focus (nếu không dùng VSync)
                if (!GraphicsConfig.VSync)
                {
                    _window.SetFramerateLimit(_foregroundFps);
                }
            }
        }

        _window.Dispose();
    }

    public static void CloseWindow()
    {
        _window.Close();

        // Dispose game subsystems first
        try { MusicManager.Dispose(); } catch { /* swallow to guarantee shutdown */ }
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
    }

    /// <summary>
    /// Renders all objects in the current scene, sorted by their Z-index.
    /// </summary>
    /// <param name="target">The render target.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Render(RenderTarget target)
    {
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

    // New: focus toggle helper
    private static void SetFocus(System.Boolean focused)
    {
        _focused = focused;
        // Nếu dùng VSync, không đổi gì; còn nếu limit FPS, chuyển giữa Foreground/Background
        if (!GraphicsConfig.VSync)
        {
            _window.SetFramerateLimit(focused ? _foregroundFps : _backgroundFps);
        }
    }
}