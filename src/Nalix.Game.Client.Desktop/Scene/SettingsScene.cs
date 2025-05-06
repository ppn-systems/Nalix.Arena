using Nalix.Game.Client.Desktop.Content;
using Nalix.Game.Client.Desktop.Core;
using Nalix.Game.Client.Desktop.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Nalix.Game.Client.Desktop.Scene;

internal class SettingsScene : IScene
{
    private readonly Sprite _background;
    private readonly Button _backButton;

    public SettingsScene()
    {
        // Set the background
        Texture texture = new(string.Format(TexturePath.Background, 0));
        _background = new Sprite(texture)
        {
            Position = new Vector2f(0, 0)
        };

        float scaleX = (float)WindowHost.Width / texture.Size.X;
        float scaleY = (float)WindowHost.Height / texture.Size.Y;
        _background.Scale = new Vector2f(scaleX, scaleY);

        // Create the back button (position, size, etc.)
        _backButton = new Button("Back", FontAssets.Pixel, new Vector2f(500, 500), new Vector2f(300, 150))
        {
            OnClick = OnBackButtonClick  // Set the action when clicked
        };
    }

    public void Draw(RenderWindow window)
    {
        // Draw the background and button
        window.Draw(_background);
        _backButton.Draw(window);
    }

    public void HandleInput(KeyEventArgs e)
    {
        // You can handle keyboard input here if needed (e.g., escape key for going back)
        if (e.Code == Keyboard.Key.Escape)
        {
            OnBackButtonClick();  // Trigger the back action
        }
    }

    public void HandleMouseInput(MouseButtonEventArgs e)
    {
        // Pass mouse input to the button
        _backButton.HandleMouseClick(e);
    }

    public void Update(float deltaTime)
    {
        // You can update your scene elements here if needed (animations, etc.)
    }

    // Action when the back button is clicked
    private void OnBackButtonClick()
    {
        // Logic for going back (for example, switching to the main menu scene)
        Console.WriteLine("Back button clicked!");
        // You can add your logic here to change the scene, e.g., to the main menu scene
    }
}