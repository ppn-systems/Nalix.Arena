using Nalix.Domain.Items;

namespace Nalix.Domain.Inventory;

public sealed class InventoryGrid(System.Int32 rows, System.Int32 columns)
{
    private readonly ItemStack[,] _slots = new ItemStack[rows, columns];

    public System.Int32 Rows { get; } = rows;
    public System.Int32 Columns { get; } = columns;

    public ItemStack GetSlot(System.Int32 row, System.Int32 column) => _slots[row, column];

    public void SetSlot(System.Int32 row, System.Int32 column, ItemStack stack) => _slots[row, column] = stack;
}
