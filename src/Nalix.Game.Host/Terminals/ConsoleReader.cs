namespace Nalix.Game.Host.Terminals;

// Concrete implementation using System.Console
public class ConsoleReader : IConsoleReader
{
    public bool KeyAvailable => System.Console.KeyAvailable;

    public System.ConsoleKeyInfo ReadKey(bool intercept) => System.Console.ReadKey(intercept);
}