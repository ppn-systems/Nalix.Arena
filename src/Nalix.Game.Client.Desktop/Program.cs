namespace Nalix.Game.Client.Desktop;

internal class Program
{
    internal static void Main(string[] args)
    {
        Window window = new();
        window.GameLoop();
    }
}