using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MergeRecipe = Game.Merge.Data.MergeRecipe;

namespace Game.Core.Registries;

public class MergeRecipeRegistry : YARD.RegistryWrapper<MergeRecipe>
{
	public MergeRecipeRegistry() : base("res://Resources/Registries/MergeRecipeRegistry.tres") {}
	
	public List<MergeRecipe> FindRecipes(List<string> itemIds)
	{
		List<MergeRecipe> foundRecipes = LoadAllBlocking().Values.ToList().FindAll(recipe =>
		{
			var requiredIds = recipe.RequiredItems.Select(item => item.Id).ToList();
			return itemIds.All(item => requiredIds.Contains(item));
		});
		
		// if multiple recipes are found, sort by number of required items (ascending)
		foundRecipes.Sort((a, b) =>
		{
			int aCount = a.RequiredItems.Count;
			int bCount = b.RequiredItems.Count;
			return aCount.CompareTo(bCount);
		});
		return foundRecipes;
	}

	public List<string> FindCompatibleItems(List<string> selectedItemIds)
	{
		HashSet<string> compatibleItemIds = new HashSet<string>();
		
		foreach (var recipe in LoadAllBlocking().Values)
		{
			var requiredIds = recipe.RequiredItems.Select(item => item.Id).ToList();
			if (selectedItemIds.All(id => requiredIds.Contains(id)))
			{
				var requiredCounts = requiredIds.GroupBy(id => id)
					.ToDictionary(g => g.Key, g => g.Count());

				var selectedCounts = selectedItemIds.GroupBy(id => id)
					.ToDictionary(g => g.Key, g => g.Count());

				foreach (var kvp in requiredCounts)
				{
					var id = kvp.Key;
					var requiredCount = kvp.Value;
					selectedCounts.TryGetValue(id, out int selectedCount);

					if (selectedCount < requiredCount)
					{
						compatibleItemIds.Add(id);
					}
				}
			}
		}
		
		return compatibleItemIds.ToList();
	}
}
