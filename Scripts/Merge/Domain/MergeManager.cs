using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Domain.Creature;
using CreatureNode = Game.Creatures.Presentation.CreatureNode;
using GameManager = Game.Core.Autoload.GameManager;
using GodotRandomAdapter = Game.Utils.GodotRandomAdapter;
using MergeRecipe = Game.Merge.Data.MergeRecipe;
using MergeRecipeDatabase = Game.Core.Autoload.MergeRecipeDatabase;

namespace Game.Merge.Domain;

public partial class MergeManager : Node
{
	[Export] private PackedScene _creatureScenePath;

	public event Action<Creature> CreatureMerged; 


	public bool CanMerge(List<string> items)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		var inventory = gameManager.Inventory;

		MergeRecipeDatabase mergeRecipeDatabase = GetNode<MergeRecipeDatabase>("/root/MergeRecipeDatabase");
		var recipes = mergeRecipeDatabase.FindRecipes(items);
		return !(recipes.Count == 0 || recipes[0].RequiredItems.Count != items.Count);
	}

	public List<string> GetCompatibleItems(List<string> selectedItems)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		var inventory = gameManager.Inventory;

		MergeRecipeDatabase mergeRecipeDatabase = GetNode<MergeRecipeDatabase>("/root/MergeRecipeDatabase");
		var compatibleItems = mergeRecipeDatabase.FindCompatibleItems(selectedItems);
		return compatibleItems;
	}

	public MergeResult AttemptMerge(List<string> items)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		var inventory = gameManager.Inventory;

		MergeRecipeDatabase mergeRecipeDatabase = GetNode<MergeRecipeDatabase>("/root/MergeRecipeDatabase");
		var recipes = mergeRecipeDatabase.FindRecipes(items);
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
		Creature creature = new Creature(recipe.ResultCreature, 1, new GodotRandomAdapter());

		return creature;
	}
}

public struct MergeResult
{
	public bool Success;
	public string Message;
	public Creature Creature;
}
