using Nalix.Game.Client.Desktop.Scene;
using Nalix.Game.Client.Desktop.Utils;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Client.Desktop.Core;

internal class MainWindow : IDisposable
{
    #region Constants

    public const string WindowTitle = "Nalix";

    public const uint FontSize = 14;
    public const uint WindowWidth = 1280;
    public const uint WindowHeight = 720;
    private const float FrameTime = 1f / 60f;

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Clock _clock;
    private readonly Text _fpsText;
    private readonly Text _debugText;
    private readonly DebugContext _fpsCounter;

    private bool _showDebugInfo = true;

    #endregion Fields

    public MainWindow()
    {
        _clock = new Clock();
        _fpsCounter = new DebugContext();
        _font = new Font("assets/fonts/JetBrainsMono.ttf");

        GameWindow.Initialize(WindowWidth, WindowHeight, WindowTitle);
        GameWindow.AttachEvents(OnKeyPressed, OnMouseButtonPressed, (_, _) => GameWindow.Window.Close());

        _fpsText = CreateText(new Vector2f(10, 10), Color.Yellow);
        _debugText = CreateText(new Vector2f(10, 30), Color.Green);

        SceneManager.Instance.SwitchTo(new MainMenuScene());
    }

    public void GameLoop()
    {
        float accumulator = 0f;

        while (GameWindow.Window.IsOpen)
        {
            float deltaTime = _clock.Restart().AsSeconds();
            accumulator += deltaTime;

            GameWindow.PollEvents();

            while (accumulator >= FrameTime)
            {
                SceneManager.Instance.Current.Update(FrameTime);
                accumulator -= FrameTime;
            }

            if (_showDebugInfo)
            {
                _fpsCounter.Update(deltaTime);
                _fpsText.DisplayedString = _fpsCounter.GetFpsText();
                _debugText.DisplayedString =
                    $"{_fpsCounter.GetElapsedTimeText()}\n{_fpsCounter.GetMemoryUsageText()}";
            }

            this.Render();
        }
    }

    private void Render()
    {
        var window = GameWindow.Window;
        window.Clear(Color.Black);
        SceneManager.Instance.Current.Draw(window);

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
                GameWindow.Window.Close();
                break;

            case Keyboard.Key.F3:
                _showDebugInfo = !_showDebugInfo;
                break;

            default:
                SceneManager.Instance.Current.HandleInput(e);
                break;
        }
    }

    private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        => SceneManager.Instance.Current.HandleMouseInput(e);

    public void Dispose()
    {
        GameWindow.Dispose();
        _font.Dispose();
    }
}