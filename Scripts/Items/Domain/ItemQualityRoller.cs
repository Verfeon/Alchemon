using Godot;
using System;

using ItemQuality = Game.Items.Domain.ItemQuality;
using ItemQualityWeight = Game.Items.Data.ItemQualityWeight;
using ItemQualityWeights = Game.Items.Data.ItemQualityWeights;

namespace Game.Items.Domain;

public static class ItemQualityRoller
{
	private static readonly RandomNumberGenerator rng = new();
	
	static ItemQualityRoller()
	{
		rng.Randomize();
	}

	public static ItemQuality Roll(ItemQualityWeights itemQualityWeights)
	{
		if (itemQualityWeights.Weights.Length == 0)
		{
			return ((ItemQuality[])Enum.GetValues(typeof(ItemQuality)))[0];
		}
		
		float totalWeight = 0f;
		foreach (ItemQualityWeight entry in itemQualityWeights.Weights)
			totalWeight += entry.Weight;

		float r = (float)GD.Randf() * totalWeight;

		foreach (ItemQualityWeight entry in itemQualityWeights.Weights)
		{
			if (r < entry.Weight)
				return entry.Quality;
			r -= entry.Weight;
		}

		return itemQualityWeights.Weights[^1].Quality; // fallback
	}
}
