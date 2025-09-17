// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Host.Terminals;

/// <summary>
/// Responsible for managing keyboard shortcuts (with modifiers) and executing actions.
/// </summary>
internal sealed class ShortcutManager
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<
        (System.ConsoleModifiers Modifiers, System.ConsoleKey Key), Shortcut> _shortcuts = new();

    /// <summary>
    /// Add or update a shortcut with specific modifiers and key.
    /// </summary>
    public void AddOrUpdateShortcut(
        System.ConsoleModifiers modifiers,
        System.ConsoleKey key, System.Action action, System.String description)
        => _shortcuts[(modifiers, key)] = new Shortcut(action, description);

    /// <summary>
    /// Add or update a shortcut with only a key (defaults to Ctrl+Key).
    /// </summary>
    public void AddOrUpdateShortcut(System.ConsoleKey key, System.Action action, System.String description)
        => AddOrUpdateShortcut(System.ConsoleModifiers.Control, key, action, description);

    /// <summary>
    /// Try execute a shortcut given pressed modifiers and key.
    /// </summary>
    public System.Boolean TryExecuteShortcut(System.ConsoleModifiers modifiers, System.ConsoleKey key)
    {
        // Normalize: only consider Ctrl/Shift/Alt flags
        var lookup = (modifiers & (System.ConsoleModifiers.Control | System.ConsoleModifiers.Shift | System.ConsoleModifiers.Alt), key);

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
    public System.Collections.Generic.IEnumerable<
        (System.ConsoleModifiers Modifiers, System.ConsoleKey Key, System.String Description)> GetAllShortcuts()
    {
        foreach (var kvp in _shortcuts)
        {
            yield return (kvp.Key.Modifiers, kvp.Key.Key, kvp.Value.Description);
        }
    }

    private sealed class Shortcut(System.Action action, System.String description)
    {
        public System.Action Action { get; } = action ?? throw new System.ArgumentNullException(nameof(action));
        public System.String Description { get; } = description ?? System.String.Empty;
    }
}
