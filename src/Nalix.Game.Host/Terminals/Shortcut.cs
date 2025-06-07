namespace Nalix.Game.Host.Terminals;

// Represents one shortcut's action and description
public class Shortcut(System.Action action, string description)
{
    public System.Action Action { get; } = action;
    public string Description { get; } = description;
}