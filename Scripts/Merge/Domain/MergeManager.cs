using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Creature = Game.Creatures.Domain.Creature;
using GameManager = Game.Core.Autoload.GameManager;
using GodotRandomAdapter = Game.Utils.Random.GodotRandomAdapter;
using MergeRecipe = Game.Merge.Data.MergeRecipe;

namespace Game.Merge.Domain;

/// <summary>
/// Manages the merge operations for the game.
/// </summary>
public partial class MergeManager : Node
{
	[Export] private PackedScene _creatureScenePath;

	public event Action<Creature> CreatureMerged; 

	/// <summary>
	/// Determines if the given items can be merged together.
	/// </summary>
	/// <param name="items">list of item IDs</param>
	/// <returns></returns>
	public bool CanMerge(List<string> items)
	{
		MergeRecipeRegistry mergeRecipeRegistry = new MergeRecipeRegistry();
		var recipes = mergeRecipeRegistry.FindRecipes(items);
		return !(recipes.Count == 0 || recipes[0].RequiredItems.Count != items.Count);
	}

	/// <summary>
	/// Determines if there is a valid merge combination among the given items.
	/// </summary>
	/// <param name="items">list of item IDs</param>
	/// <returns></returns>
	public bool IsMergePossible(List<string> items)
	{
		MergeRecipeRegistry mergeRecipeRegistry = new MergeRecipeRegistry();
		var recipes = mergeRecipeRegistry.LoadAllBlocking().Values;

		var itemCounts = items
			.GroupBy(i => i)
			.ToDictionary(g => g.Key, g => g.Count());

		foreach (var recipe in recipes)
		{
			var requiredCounts = recipe.RequiredItems
				.GroupBy(i => i.Id)
				.ToDictionary(g => g.Key, g => g.Count());

			bool canMerge = true;

			foreach (var req in requiredCounts)
			{
				if (!itemCounts.TryGetValue(req.Key, out int available) || available < req.Value)
				{
					canMerge = false;
					break;
				}
			}

			if (canMerge)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Gets a list of items that can be merged with the selected items.
	/// </summary>
	/// <param name="selectedItems">list of selected item IDs</param>
	/// <returns>list of compatible item IDs</returns>
	public List<string> GetCompatibleItems(List<string> selectedItems)
	{
		MergeRecipeRegistry mergeRecipeRegistry = new MergeRecipeRegistry();
		var compatibleItems = mergeRecipeRegistry.FindCompatibleItems(selectedItems);
		return compatibleItems;
	}

	/// <summary>
	/// Attempts to merge the given items and returns the result, including success status and the resulting creature if successful.
	/// </summary>
	/// <param name="items">list of item IDs to merge</param>
	/// <returns></returns> <summary>
	public MergeResult AttemptMerge(List<string> items)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		var inventory = gameManager.Inventory;

		MergeRecipeRegistry mergeRecipeRegistry = new MergeRecipeRegistry();
		var recipes = mergeRecipeRegistry.FindRecipes(items);
		if (recipes.Count == 0 || recipes[0].RequiredItems.Count != items.Count)
		{
			return new MergeResult { Success = false, Message = "Invalid combination" };
		}
		var recipe = recipes[0];

		var creature = CreateCreatureFromRecipe(recipe);

		CreatureMerged?.Invoke(creature);
		return new MergeResult { Success = true, Creature = creature };
	}

	private Creature CreateCreatureFromRecipe(MergeRecipe recipe)
	{
		Creature creature = new Creature(recipe.ResultCreature, 10, new GodotRandomAdapter());

		return creature;
	}
}

/// <summary>
/// Represents the result of a merge operation, including whether it was successful, an optional message, and the resulting creature if the merge was successful.
/// </summary>
public struct MergeResult
{
	public bool Success;
	public string Message;
	public Creature Creature;
}
