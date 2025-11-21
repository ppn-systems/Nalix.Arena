using Nalix.Domain.Items;

namespace Nalix.Domain.Inventory;

public sealed class ItemDefinition(
    System.String id, System.String displayName,
    System.String iconKey,
    System.Int32 maxStack = 64,
    ItemRarity rarity = ItemRarity.Common)
{
    public ItemRarity Rarity { get; } = rarity;
    public System.Int32 MaxStack { get; } = maxStack;
    public System.String Id { get; } = id ?? throw new System.ArgumentNullException(nameof(id));
    public System.String IconKey { get; } = iconKey ?? throw new System.ArgumentNullException(nameof(iconKey));
    public System.String DisplayName { get; } = displayName ?? throw new System.ArgumentNullException(nameof(displayName));
}
