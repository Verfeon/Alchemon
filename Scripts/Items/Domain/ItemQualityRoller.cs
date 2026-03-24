using Godot;
using System;

using ItemQualityWeight = Game.Items.Data.ItemQualityWeight;
using ItemQualityWeights = Game.Items.Data.ItemQualityWeights;

namespace Game.Items.Domain;

/// <summary>
/// Provides functionality for rolling item quality based on defined weights.
/// </summary>
public static class ItemQualityRoller
{
	private static readonly RandomNumberGenerator rng = new();
	
	static ItemQualityRoller()
	{
		rng.Randomize();
	}

	/// <summary>
	/// Rolls a random item quality based on the provided weights.
	/// </summary>
	/// <param name="itemQualityWeights"></param>
	/// <returns></returns>
	public static ItemQuality Roll(ItemQualityWeights itemQualityWeights)
	{
		if (itemQualityWeights.Weights.Length == 0)
		{
			GD.PushWarning("Item Quality Weights empty. Using first quality");
			return ((ItemQuality[])Enum.GetValues(typeof(ItemQuality)))[0];
		}
		
		float totalWeight = 0f;
		foreach (ItemQualityWeight entry in itemQualityWeights.Weights)
		{
			totalWeight += entry.Weight;
		}

		float r = (float)GD.Randf() * totalWeight;

		foreach (ItemQualityWeight entry in itemQualityWeights.Weights)
		{
			if (r < entry.Weight)
			{
				return entry.Quality;
			}
			r -= entry.Weight;
		}

		return itemQualityWeights.Weights[^1].Quality; // fallback
	}
}
