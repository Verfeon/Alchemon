using Godot;

using ItemRarity = Game.Items.Domain.ItemRarity;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class MergeableItemData : Resource
{
	[Export] public string Id;
	[Export] public string Name;
	[Export] public ItemRarity Rarity;
	[Export] public Texture2D Texture2d;
}
