using Nalix.Game.Client.Desktop.Scene;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Client.Desktop.Core;

internal class MainWindow : IDisposable
{
    #region Constants

    public const uint FontSize = 14;
    public const uint WindowWidth = 1280;
    public const uint WindowHeight = 720;
    public const string WindowTitle = "Nalix";

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Clock _clock;
    private readonly Text _fpsText;
    private readonly Text _debugText;

    private int _frameCount = 0;
    private float _currentFps = 0f;
    private float _fpsUpdateTimer = 0f;
    private bool _showDebugInfo = true;

    #endregion Fields

    public MainWindow()
    {
        _clock = new Clock();
        _font = new Font("assets/fonts/JetBrainsMono.ttf");

        WindowHost.Initialize(WindowWidth, WindowHeight, WindowTitle);
        WindowHost.AttachEvents(OnKeyPressed, OnMouseButtonPressed, (_, _) => WindowHost.Window.Close());

        _fpsText = CreateText(new Vector2f(10, 10), Color.Yellow);
        _debugText = CreateText(new Vector2f(10, 30), Color.Green);

        SceneHost.SwitchTo(new MainMenuScene());
    }

    public void GameLoop()
    {
        const float frameTime = 1f / 60f;
        float accumulator = 0f;

        while (WindowHost.Window.IsOpen)
        {
            float deltaTime = _clock.Restart().AsSeconds();
            accumulator += deltaTime;

            WindowHost.PollEvents();

            while (accumulator >= frameTime)
            {
                SceneHost.Current.Update(frameTime);
                accumulator -= frameTime;
            }

            if (_showDebugInfo)
                this.UpdateDebugMetrics(deltaTime);

            this.Render();
        }
    }

    private void Render()
    {
        var window = WindowHost.Window;
        window.Clear(Color.Black);
        SceneHost.Current.Draw(window);

        if (_showDebugInfo)
        {
            window.Draw(_fpsText);
            window.Draw(_debugText);
        }

        window.Display();
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
                WindowHost.Window.Close();
                break;

            case Keyboard.Key.F3:
                _showDebugInfo = !_showDebugInfo;
                break;

            default:
                SceneHost.Current.HandleInput(e);
                break;
        }
    }

    private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
    {
        SceneHost.Current.HandleMouseInput(e);
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

    public void Dispose()
    {
        WindowHost.Dispose();
        _font.Dispose();
    }
}