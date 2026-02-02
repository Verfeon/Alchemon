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

	[Signal] public delegate void MergeSuccessEventHandler(string creatureId);
	[Signal] public delegate void MergeFailedEventHandler(string reason);
	[Signal] public delegate void RecipeDiscoveredEventHandler(string recipeId);

	public MergeResult AttemptMerge(List<string> items)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		var inventory = gameManager.Inventory;

		MergeRecipeDatabase mergeRecipeDatabase = GetNode<MergeRecipeDatabase>("/root/MergeRecipeDatabase");
		var recipes = mergeRecipeDatabase.FindRecipes(items);
		if (recipes.Count == 0 || recipes[0].RequiredItems.Count != items.Count)
		{
			EmitSignal(SignalName.MergeFailed, "No recipe found for this combination");
			return new MergeResult { Success = false, Message = "Invalid combination" };
		}
		var recipe = recipes[0];

		var creature = CreateCreatureFromRecipe(recipe);

		EmitSignal(SignalName.MergeSuccess, creature.Data.Id);
		return new MergeResult { Success = true, Creature = creature };
	}

	private Creature CreateCreatureFromRecipe(MergeRecipe recipe)
	{
		Creature creature = new Creature(recipe.ResultCreature, 1, new GodotRandomAdapter());

		CreatureNode newCreature = _creatureScenePath.Instantiate<CreatureNode>();
		newCreature.Bind(creature);

		GetTree().Root.AddChild(newCreature);

		return creature;
	}
}

public struct MergeResult
{
	public bool Success;
	public string Message;
	public Creature Creature;
}
