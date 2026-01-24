using Godot;
using System;
using System.Collections.Generic;

using MergeableItemData = Game.Resources.MergeableItemData;
using ItemQuality = Game.Enums.ItemQuality;

namespace Game;

public class MergeableItem
{
	public MergeableItemData Data { get;}
	public ItemQuality Quality { get; }

	public MergeableItem(MergeableItemData data, ItemQuality quality)
	{
		Data = data;
		Quality = quality;
	}
}
