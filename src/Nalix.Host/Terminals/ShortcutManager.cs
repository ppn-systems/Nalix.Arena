using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Host.Terminals;

/// <summary>
/// Responsible for managing keyboard shortcuts (with modifiers) and executing actions.
/// </summary>
internal sealed class ShortcutManager
{
    private readonly ConcurrentDictionary<(ConsoleModifiers Modifiers, ConsoleKey Key), Shortcut> _shortcuts = new();

    /// <summary>
    /// Add or update a shortcut with specific modifiers and key.
    /// </summary>
    public void AddOrUpdateShortcut(ConsoleModifiers modifiers, ConsoleKey key, Action action, String description)
        => _shortcuts[(modifiers, key)] = new Shortcut(action, description);

    /// <summary>
    /// Add or update a shortcut with only a key (defaults to Ctrl+Key).
    /// </summary>
    public void AddOrUpdateShortcut(ConsoleKey key, Action action, String description)
        => AddOrUpdateShortcut(ConsoleModifiers.Control, key, action, description);

    /// <summary>
    /// Try execute a shortcut given pressed modifiers and key.
    /// </summary>
    public Boolean TryExecuteShortcut(ConsoleModifiers modifiers, ConsoleKey key)
    {
        // Normalize: only consider Ctrl/Shift/Alt flags
        var lookup = (modifiers & (ConsoleModifiers.Control | ConsoleModifiers.Shift | ConsoleModifiers.Alt), key);

        if (_shortcuts.TryGetValue(lookup, out var shortcut))
        {
            shortcut.Action?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get all registered shortcuts for help display.
    /// </summary>
    public IEnumerable<(ConsoleModifiers Modifiers, ConsoleKey Key, String Description)> GetAllShortcuts()
    {
        foreach (var kvp in _shortcuts)
        {
            yield return (kvp.Key.Modifiers, kvp.Key.Key, kvp.Value.Description);
        }
    }

    private sealed class Shortcut(Action action, String description)
    {
        public Action Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
        public String Description { get; } = description ?? String.Empty;
    }
}
