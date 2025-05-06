using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Client.Desktop.Core;

/// <summary>
/// Provides global access and utilities for managing the main game window.
/// </summary>
public static class WindowHost
{
    /// <summary>
    /// Gets the current game window.
    /// </summary>
    public static RenderWindow Window { get; private set; } = null!;

    /// <summary>
    /// Gets the current window width in pixels.
    /// </summary>
    public static uint Width { get; private set; }

    /// <summary>
    /// Gets the current window height in pixels.
    /// </summary>
    public static uint Height { get; private set; }

    /// <summary>
    /// Initializes the main window with the specified size and title.
    /// </summary>
    /// <param name="width">The width of the window in pixels.</param>
    /// <param name="height">The height of the window in pixels.</param>
    /// <param name="title">The window title displayed in the title bar.</param>
    public static void Initialize(uint width, uint height, string title)
    {
        Width = width;
        Height = height;

        VideoMode mode = new(width, height);
        Window = new RenderWindow(mode, title, Styles.Titlebar | Styles.Close);

        Window.SetFramerateLimit(60);
        Window.SetVerticalSyncEnabled(true);
    }

    /// <summary>
    /// Gets the current mouse position relative to the window.
    /// </summary>
    /// <returns>The mouse position as a <see cref="Vector2i"/>.</returns>
    public static Vector2i GetMousePosition() => Mouse.GetPosition(Window);

    /// <summary>
    /// Sets the window size to the specified width and height.
    /// </summary>
    /// <param name="width">The new width of the window.</param>
    /// <param name="height">The new height of the window.</param>
    public static void SetSize(uint width, uint height)
    {
        Width = width;
        Height = height;
        Window.Size = new Vector2u(width, height);
    }

    /// <summary>
    /// Polls and dispatches all pending window events.
    /// </summary>
    public static void PollEvents() => Window.DispatchEvents();

    /// <summary>
    /// Attaches input and window events to the current window.
    /// </summary>
    /// <param name="keyHandler">The handler for key press events.</param>
    /// <param name="mouseHandler">The handler for mouse button press events.</param>
    /// <param name="closedHandler">The optional handler for the window close event.</param>
    public static void AttachEvents(
        EventHandler<KeyEventArgs> keyHandler,
        EventHandler<MouseButtonEventArgs> mouseHandler,
        EventHandler closedHandler = null)
    {
        Window.KeyPressed += keyHandler;
        Window.MouseButtonPressed += mouseHandler;

        if (closedHandler != null)
            Window.Closed += closedHandler;
    }

    /// <summary>
    /// Disposes the window and releases all associated resources.
    /// </summary>
    public static void Dispose() => Window.Dispose();
}