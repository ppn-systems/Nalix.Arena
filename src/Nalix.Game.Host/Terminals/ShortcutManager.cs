using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nalix.Game.Host.Terminals;

// Responsible for managing shortcuts and executing actions
public class ShortcutManager
{
    private readonly ConcurrentDictionary<ConsoleKey, Shortcut> _shortcuts = new();

    public void AddOrUpdateShortcut(ConsoleKey key, Action action, String description)
        => _shortcuts[key] = new Shortcut(action, description);

    public Boolean TryExecuteShortcut(ConsoleModifiers modifiers, ConsoleKey key)
    {
        if (modifiers.HasFlag(ConsoleModifiers.Control) && _shortcuts.TryGetValue(key, out var shortcut))
        {
            shortcut.Action?.Invoke();
            return true;
        }
        return false;
    }

    public IEnumerable<(ConsoleKey Key, String Description)> GetAllShortcuts()
    {
        foreach (var kvp in _shortcuts)
        {
            yield return (kvp.Key, kvp.Value.Description);
        }
    }
}