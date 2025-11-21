using Nalix.Domain.Inventory;
using System;

namespace Nalix.Domain.Items;

public sealed class ItemStack
{
    public ItemDefinition Definition { get; }
    public Int32 Quantity { get; private set; }

    public Boolean IsFull => Quantity >= Definition.MaxStack;

    public Int32 RemainingSpace => Math.Max(Definition.MaxStack - Quantity, 0);

    public ItemStack(ItemDefinition definition, Int32 quantity)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));

        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        Quantity = Math.Min(quantity, Definition.MaxStack);
    }

    /// <summary>
    /// Adds the specified amount to this stack, clamped by MaxStack.
    /// Returns the amount that could not be added due to overflow.
    /// For negative amounts, removes items and returns the negative leftover
    /// that could not be removed (usually 0).
    /// </summary>
    public Int32 Add(Int32 amount)
    {
        if (amount == 0)
        {
            return 0;
        }

        if (amount > 0)
        {
            var space = RemainingSpace;
            var toAdd = Math.Min(space, amount);

            Quantity += toAdd;

            // amount - toAdd là phần thừa không nhét được vào stack này
            return amount - toAdd;
        }
        else
        {
            // Remove
            var remove = Math.Min(Quantity, -amount);
            Quantity -= remove;

            // amount là số âm, amount + remove (âm hoặc 0) là phần không remove được
            return amount + remove;
        }
    }
}

