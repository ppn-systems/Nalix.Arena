using Nalix.Game.Client.Desktop.Scene;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

internal class MainWindow : IDisposable
{
    #region Constants

    public const uint FontSize = 14;
    public const uint FpsLimit = 60;
    public const int WindowWidth = 1280;
    public const int WindowHeight = 720;
    public const string WindowTitle = "Nalix";

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Clock _clock;
    private readonly Text _fpsText;
    private readonly Text _debugText;
    private readonly RenderWindow _window;

    private int _frameCount = 0;
    private float _currentFps = 0f;
    private float _fpsUpdateTimer = 0f;
    private bool _showDebugInfo = true;

    #endregion Fields

    #region Constructor

    public MainWindow()
    {
        _clock = new Clock();
        _font = new Font("assets/fonts/JetBrainsMono.ttf");

        // Initialize window
        _window = Initialize();
        this.AttachWindowEvents();

        // Initialize debug text elements
        _fpsText = CreateText(new Vector2f(10, 10), Color.Yellow);
        _debugText = CreateText(new Vector2f(10, 30), Color.Green);

        SceneManager.SwitchTo(new MainMenuScene());
    }

    #endregion Constructor

    public void GameLoop()
    {
        const float frameTime = 1f / 60f; // Fixed time step for update logic
        float accumulator = 0f;

        while (_window.IsOpen)
        {
            float deltaTime = _clock.Restart().AsSeconds();
            accumulator += deltaTime;

            _window.DispatchEvents();

            // Update game logic with fixed time step
            while (accumulator >= frameTime)
            {
                SceneManager.Current.Update(frameTime);
                accumulator -= frameTime;
            }

            // Only update debug information when debug info is enabled
            if (_showDebugInfo) this.UpdateDebugMetrics(deltaTime);

            this.Render();
        }
    }

    #region Private Methods

    private static RenderWindow Initialize()
    {
        VideoMode mode = new(WindowWidth, WindowHeight); // HD resolution
        RenderWindow window = new(mode, WindowTitle, Styles.Titlebar | Styles.Close);

        window.SetFramerateLimit(FpsLimit);
        window.SetVerticalSyncEnabled(true);
        return window;
    }

    private void Render()
    {
        _window.Clear(Color.Black);
        SceneManager.Current.Draw(_window);

        if (_showDebugInfo)
        {
            _window.Draw(_fpsText);
            _window.Draw(_debugText);
        }

        _window.Display();
    }

    private void AttachWindowEvents()
    {
        _window.Closed += (_, _) => _window.Close();
        _window.KeyPressed += OnKeyPressed;
        _window.MouseButtonPressed += OnMouseButtonPressed;
    }

    private Text CreateText(Vector2f position, Color color)
    {
        return new Text("", _font, FontSize)
        {
            Position = position,
            FillColor = color
        };
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Code)
        {
            case Keyboard.Key.Escape:
                _window.Close();
                break;

            case Keyboard.Key.F3:
                _showDebugInfo = !_showDebugInfo;
                break;

            default:
                SceneManager.Current.HandleInput(e);
                break;
        }
    }

    // Xử lý sự kiện nhấn chuột
    private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
    {
        switch (e.Button)
        {
            default:
                SceneManager.Current.HandleMouseInput(e);
                break;
        }
    }

    private void UpdateDebugMetrics(float deltaTime)
    {
        _frameCount++;
        _fpsUpdateTimer += deltaTime;

        if (_fpsUpdateTimer >= 1.0f)
        {
            _currentFps = _frameCount / _fpsUpdateTimer;
            _frameCount = 0;
            _fpsUpdateTimer = 0f;

            _fpsText.DisplayedString = $"FPS: {_currentFps:0}";
            _debugText.DisplayedString = $"F3: Debug | Memory: {GC.GetTotalMemory(false) / 1024} KB";
        }
    }

    #endregion Private Methods

    #region Disposable

    public void Dispose()
    {
        // Properly dispose of resources and detach events
        _window.Closed -= (_, _) => _window.Close();
        _window.KeyPressed -= OnKeyPressed;
        _window.MouseButtonPressed -= OnMouseButtonPressed;

        _window.Dispose();
        _font.Dispose();
    }

    #endregion Disposable
}