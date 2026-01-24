using Godot;

using ItemRarity = Game.Enums.ItemRarity;

namespace Game.Resources;

[GlobalClass]
public partial class MergeableItemData : Resource
{
	[Export] public string Id;
	[Export] public string Name;
	[Export] public ItemRarity Rarity;
	[Export] public Texture2D Texture2d;
}
