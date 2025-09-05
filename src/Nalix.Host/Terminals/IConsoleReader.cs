namespace Nalix.Host.Terminals;

internal interface IConsoleReader
{
    System.Boolean KeyAvailable { get; }

    System.ConsoleKeyInfo ReadKey(System.Boolean intercept);
}