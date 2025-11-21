using Microsoft.Xna.Framework;
using Nalix.Domain.Inventory;
using Nalix.Domain.Items;

namespace Nalix.Mono.UI.Inventory;

public sealed class InventoryActions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0016:Use 'throw' expression", Justification = "<Pending>")]
    public InventoryActions(InventoryGrid inventory)
    {
        if (inventory == null)
        {
            throw new System.ArgumentNullException(nameof(inventory));
        }
        this._inventory = inventory;
        this._lastClickSlot = new Point(-1, -1);
    }
    public ItemStack CursorStack
    {
        get
        {
            return this._cursorStack;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "<Pending>")]
    public void OnLeftClick(System.Int32 row, System.Int32 column, System.Double nowMs)
    {
        Point currentSlot = new Point(row, column);
        if (this._lastClickSlot == currentSlot && nowMs - this._lastClickTimeMs <= 500.0 && this._cursorStack == null)
        {
            this.HandleDoubleClick(row, column);
        }
        else
        {
            this.HandleLeftClick(row, column);
        }
        this._lastClickSlot = currentSlot;
        this._lastClickTimeMs = nowMs;
    }

    public void OnRightClick(System.Int32 row, System.Int32 column)
    {
        this.HandleRightClick(row, column);
    }

    private void HandleLeftClick(System.Int32 row, System.Int32 column)
    {
        ItemStack slotStack = this._inventory.GetSlot(row, column);
        if (this._cursorStack == null)
        {
            if (slotStack == null)
            {
                return;
            }
            this._cursorStack = slotStack;
            this._inventory.SetSlot(row, column, null);
            return;
        }
        else
        {
            if (slotStack == null)
            {
                this._inventory.SetSlot(row, column, this._cursorStack);
                this._cursorStack = null;
                return;
            }
            if (slotStack.Definition == this._cursorStack.Definition)
            {
                InventoryActions.MergeStacksRespectingMax(slotStack, this._cursorStack);
                if (this._cursorStack.Quantity <= 0)
                {
                    this._cursorStack = null;
                }
                return;
            }
            this._inventory.SetSlot(row, column, this._cursorStack);
            this._cursorStack = slotStack;
            return;
        }
    }

    private void HandleRightClick(System.Int32 row, System.Int32 column)
    {
        ItemStack slotStack = this._inventory.GetSlot(row, column);
        if (this._cursorStack == null)
        {
            if (slotStack == null || slotStack.Quantity <= 1)
            {
                return;
            }
            System.Int32 half = slotStack.Quantity / 2;
            this._cursorStack = new ItemStack(slotStack.Definition, half);
            _ = slotStack.Add(-half);
            return;
        }
        else
        {
            if (slotStack == null)
            {
                this._inventory.SetSlot(row, column, new ItemStack(this._cursorStack.Definition, 1));
                _ = this._cursorStack.Add(-1);
                if (this._cursorStack.Quantity <= 0)
                {
                    this._cursorStack = null;
                }
                return;
            }
            if (slotStack.Definition == this._cursorStack.Definition)
            {
                if (slotStack.IsFull)
                {
                    return;
                }
                if (slotStack.Add(1) == 0)
                {
                    _ = this._cursorStack.Add(-1);
                    if (this._cursorStack.Quantity <= 0)
                    {
                        this._cursorStack = null;
                    }
                }
            }
            return;
        }
    }

    private void HandleDoubleClick(System.Int32 row, System.Int32 column)
    {
        ItemStack target = this._inventory.GetSlot(row, column);
        if (target == null)
        {
            return;
        }
        for (System.Int32 r = 0; r < this._inventory.Rows; r++)
        {
            for (System.Int32 c = 0; c < this._inventory.Columns; c++)
            {
                if (r != row || c != column)
                {
                    ItemStack stack = this._inventory.GetSlot(r, c);
                    if (stack != null && stack.Definition == target.Definition)
                    {
                        InventoryActions.MergeStacksRespectingMax(target, stack);
                        if (stack.Quantity <= 0)
                        {
                            this._inventory.SetSlot(r, c, null);
                        }
                        if (target.IsFull)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }

    private static void MergeStacksRespectingMax(ItemStack target, ItemStack source)
    {
        if (target.Definition != source.Definition)
        {
            return;
        }
        if (target.IsFull || source.Quantity <= 0)
        {
            return;
        }
        System.Int32 overflow = target.Add(source.Quantity);
        System.Int32 moved = source.Quantity - overflow;
        if (moved > 0)
        {
            _ = source.Add(-moved);
        }
    }

    private readonly InventoryGrid _inventory;

    private ItemStack _cursorStack;

    private Point _lastClickSlot;

    private System.Double _lastClickTimeMs;
}
