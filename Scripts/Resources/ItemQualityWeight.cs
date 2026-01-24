using Godot;
using System;

using ItemQuality = Game.Enums.ItemQuality;

namespace Game.Resources;

[GlobalClass]
public partial class ItemQualityWeight : Resource
{
	[Export]
	public ItemQuality Quality;

	[Export]
	public float Weight = 1f;
	
	public ItemQualityWeight()
	{
		Quality = ((ItemQuality[])Enum.GetValues(typeof(ItemQuality)))[0];
		Weight = 1f;
	}
	
	public ItemQualityWeight(ItemQuality quality)
	{
		Quality = quality;
		Weight = 1f;
	}
}
