namespace Nalix.Game.Client.Desktop;

internal class Program
{
    internal static void Main(string[] args)
    {
        MainWindow window = new();
        window.GameLoop();
    }
}