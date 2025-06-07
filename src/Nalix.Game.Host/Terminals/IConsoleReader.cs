namespace Nalix.Game.Host.Terminals;

// Interface to abstract reading console keys (for testability)
public interface IConsoleReader
{
    bool KeyAvailable { get; }

    System.ConsoleKeyInfo ReadKey(bool intercept);
}