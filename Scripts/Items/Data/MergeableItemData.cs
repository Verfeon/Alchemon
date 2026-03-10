using Godot;

using ItemRarity = Game.Items.Domain.ItemRarity;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class MergeableItemData : BaseItemData
{
	[Export] public ItemRarity Rarity;
}
