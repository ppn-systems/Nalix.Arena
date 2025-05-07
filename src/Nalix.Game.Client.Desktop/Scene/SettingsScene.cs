using Nalix.Game.Client.Desktop.Content;
using Nalix.Game.Client.Desktop.Core;
using Nalix.Game.Client.Desktop.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene;

internal sealed class SettingsScene : Scene
{
    private readonly Sprite _background;
    private readonly Sprite _settingsTable;
    private readonly Button _backButton;

    private float _backDelayTimer = 0f;
    private bool _isBackPressed = false;

    public SettingsScene()
    {
        // Set the background
        Texture texture = new(string.Format(TexturePath.Background, 0));
        _background = new Sprite(texture)
        {
            Position = new Vector2f(0, 0)
        };

        float scaleX = (float)GameWindow.Width / texture.Size.X;
        float scaleY = (float)GameWindow.Height / texture.Size.Y;
        _background.Scale = new Vector2f(scaleX, scaleY);

        // Create the settings image (position, size, etc.)
        Texture sTexture = new(string.Format(TexturePath.UI, 3)); // Adjust texture path for settings image
        Vector2f scale = new(5f, 5f);

        _settingsTable = new Sprite(sTexture)
        {
            Scale = scale // Adjust the scale of the settings image
        };

        float posX = (GameWindow.Width - sTexture.Size.X * scale.X) / 2f;
        float posY = (GameWindow.Height - sTexture.Size.Y * scale.Y) / 2f;

        _settingsTable.Position = new Vector2f(posX, posY);

        Vector2f tableSize = new(
            _settingsTable.Texture.Size.X * _settingsTable.Scale.X,
            _settingsTable.Texture.Size.Y * _settingsTable.Scale.Y
        );

        Vector2f buttonSize = new(
            tableSize.X * 0.18f, // ví dụ: 18% chiều rộng bảng
            tableSize.Y * 0.11f  // ví dụ: 11% chiều cao bảng
        );

        // Vị trí nút: góc dưới trái
        Vector2f buttonPos = new(
            _settingsTable.Position.X + tableSize.X * 0.15f,
            _settingsTable.Position.Y + tableSize.Y - tableSize.Y * 0.2f
        );

        // Create the back button (position, size, etc.)
        _backButton = new Button("Back", FontAssets.Pixel, buttonPos, buttonSize)
        {
            OnClick = OnBackButtonClick  // Set the action when clicked
        };
    }

    public override void Draw(RenderWindow window)
    {
        // Draw the background and button
        window.Draw(_background);
        window.Draw(_settingsTable);
        _backButton.Draw(window);
    }

    public override void HandleInput(KeyEventArgs e)
    {
        // You can handle keyboard input here if needed (e.g., escape key for going back)
        if (e.Code == Keyboard.Key.B) OnBackButtonClick();
    }

    // Handle mouse input for the back button
    public override void Update(float deltaTime)
    {
        // You can update your scene elements here if needed (animations, etc.)
        if (_isBackPressed)
        {
            _backDelayTimer += deltaTime;
            if (_backDelayTimer >= 0.1f)
            {
                SceneManager.Instance.SwitchTo(new MainMenuScene());
                _isBackPressed = false;
            }
        }
    }

    // Handle mouse input for the back button
    public override void HandleMouseInput(MouseButtonEventArgs e) => _backButton.HandleMouseClick(e);

    // Action when the back button is clicked
    private void OnBackButtonClick() => _isBackPressed = true;
}