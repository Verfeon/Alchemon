using Godot;
using System;
using System.Collections.Generic;

using ItemQuality = Game.Items.Domain.ItemQuality;

namespace Game.Items.Data;

[GlobalClass] [Tool]
public partial class ItemQualityWeights : Resource
{
	[Export]
	public ItemQualityWeight[] Weights;
	
	public ItemQualityWeights()
	{
		ItemQuality[] itemQualities = (ItemQuality[])Enum.GetValues(typeof(ItemQuality));
		int count = itemQualities.Length;
		if (Weights == null || Weights.Length != count)
		{
			Weights = new ItemQualityWeight[count];
			for (int i = 0; i < count; i++)
			{
				Weights[i] = new ItemQualityWeight(itemQualities[i]);
			}
		}
	}
}
