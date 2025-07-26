namespace Nalix.Host.Terminals;

// Represents one shortcut's action and description
public class Shortcut(System.Action action, System.String description)
{
    public System.Action Action { get; } = action;
    public System.String Description { get; } = description;
}