namespace Nalix.Host.Terminals;

// Concrete implementation using System.Console
internal class ConsoleReader : IConsoleReader
{
    public System.Boolean KeyAvailable => System.Console.KeyAvailable;

    public System.ConsoleKeyInfo ReadKey(System.Boolean intercept) => System.Console.ReadKey(intercept);
}