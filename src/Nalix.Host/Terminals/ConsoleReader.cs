namespace Nalix.Host.Terminals;

internal interface IConsoleReader
{
    System.Boolean KeyAvailable { get; }

    System.ConsoleKeyInfo ReadKey(System.Boolean intercept);
}

internal class ConsoleReader : IConsoleReader
{
    public System.Boolean KeyAvailable => System.Console.KeyAvailable;

    public System.ConsoleKeyInfo ReadKey(System.Boolean intercept) => System.Console.ReadKey(intercept);
}