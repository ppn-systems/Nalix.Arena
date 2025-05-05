using SFML.Graphics;
using SFML.Window;

namespace Nalix.Game.Client.Desktop.Scene
{
    internal class MainMenuScene : IScene
    {
        public void Update(float deltaTime)
        { /* animate title, buttons, etc */ }

        public void Draw(RenderWindow window)
        {
            // window.Draw(titleText);
            // window.Draw(startButton);
        }

        public void HandleInput(KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Enter)
            {
                SceneManager.SwitchTo(new GameScene());
            }
        }
    }
}