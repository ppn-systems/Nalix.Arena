namespace Nalix.Host.Terminals;

// Interface to abstract reading console keys (for testability)
public interface IConsoleReader
{
    System.Boolean KeyAvailable { get; }

    System.ConsoleKeyInfo ReadKey(System.Boolean intercept);
}