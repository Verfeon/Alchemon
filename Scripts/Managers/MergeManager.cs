using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Creature;
using CreatureNode = Game.Creatures.CreatureNode;
using GameManager = Game.Autoload.GameManager;
using CreatureDatabase = Game.Autoload.CreatureDatabase;

namespace Game.Managers;

public partial class MergeManager : Node
{
	[Export] private PackedScene _creatureScenePath;
	
	private List<MergeRecipe> _recipes = new();
	private HashSet<string> _discoveredRecipes = new();
	
	[Signal]
	public delegate void MergeSuccessEventHandler(string creatureId);
	
	[Signal]
	public delegate void MergeFailedEventHandler(string reason);
	
	[Signal]
	public delegate void RecipeDiscoveredEventHandler(string recipeId);
	
	public void LoadRecipes(string jsonPath)
	{
		var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
		if (file != null)
		{
			try
			{
				var json = file.GetAsText();
				var parseResult = Json.ParseString(json);
				
				if (parseResult.VariantType != Variant.Type.Dictionary)
				{
					GD.PrintErr("Failed to parse merge_recipes.json - invalid JSON format");
					file.Close();
					return;
				}
				
				var data = parseResult.AsGodotDictionary();
				
				if (!data.ContainsKey("recipes"))
				{
					GD.PrintErr("merge_recipes.json missing 'recipes' key");
					file.Close();
					return;
				}
				
				var recipesArray = data["recipes"].AsGodotArray();
				
				foreach (var recipe in recipesArray)
				{
					var recipeDict = recipe.AsGodotDictionary();
					
					var MergeRecipe = new MergeRecipe
					{
						Ingredients = new List<string>(),
						ResultCreatureId = recipeDict.ContainsKey("result") ? recipeDict["result"].AsString() : "",
						IsDiscovered = recipeDict.ContainsKey("is_discovered") ? recipeDict["is_discovered"].AsBool() : false,
						UnlockCondition = recipeDict.ContainsKey("unlock_condition") && recipeDict["unlock_condition"].VariantType != Variant.Type.Nil 
							? recipeDict["unlock_condition"].AsString() : null,
						HintText = recipeDict.ContainsKey("hint") ? recipeDict["hint"].AsString() : ""
					};
					
					if (recipeDict.ContainsKey("ingredients"))
					{
						var ingredientsArray = recipeDict["ingredients"].AsGodotArray();
						foreach (var ingredient in ingredientsArray)
						{
							MergeRecipe.Ingredients.Add(ingredient.AsString());
						}
					}
					
					_recipes.Add(MergeRecipe);
				}
				
				GD.Print($"Loaded {_recipes.Count} merge recipes");
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error loading merge recipes: {e.Message}");
			}
			finally
			{
				file.Close();
			}
		}
		else
		{
			GD.PrintErr($"Could not open file: {jsonPath}");
		}
	}
	
	public MergeResult AttemptMerge(string objectId1, string objectId2)
	{
		var inventory = GameManager.Instance.Inventory;
		
		// Vérifier si on a les objets
		if (!inventory.HasItem(objectId1) || !inventory.HasItem(objectId2))
		{
			return new MergeResult { Success = false, Message = "Objets manquants" };
		}
		
		// Chercher une recette correspondante (ordre indépendant)
		var recipe = FindRecipe(objectId1, objectId2);
		
		if (recipe == null)
		{
			EmitSignal(SignalName.MergeFailed, "Aucune recette trouvée pour cette combinaison");
			return new MergeResult { Success = false, Message = "Combinaison invalide" };
		}
		
		// Vérifier les conditions de déblocage
		if (!string.IsNullOrEmpty(recipe.UnlockCondition) && !IsConditionMet(recipe.UnlockCondition))
		{
			return new MergeResult { Success = false, Message = "Recette non débloquée" };
		}
		
		// Consommer les objets
		inventory.RemoveItem(objectId1, 1);
		inventory.RemoveItem(objectId2, 1);
		
		// Créer la créature
		var creature = CreateCreatureFromRecipe(recipe);
		GameManager.Instance.Creatures.AddCreature(creature);
		
		// Marquer comme découverte
		if (!_discoveredRecipes.Contains(recipe.ResultCreatureId))
		{
			_discoveredRecipes.Add(recipe.ResultCreatureId);
			EmitSignal(SignalName.RecipeDiscovered, recipe.ResultCreatureId);
		}
		
		EmitSignal(SignalName.MergeSuccess, creature.Data.Id);
		return new MergeResult { Success = true, Creature = creature };
	}
	
	private MergeRecipe FindRecipe(string obj1, string obj2)
	{
		return _recipes.Find(r => 
			(r.Ingredients.Contains(obj1) && r.Ingredients.Contains(obj2)) &&
			r.Ingredients.Count == 2
		);
	}
	
	private bool IsConditionMet(string condition)
	{
		// Implémenter la logique de vérification des conditions
		return true;
	}
	
	private Creature CreateCreatureFromRecipe(MergeRecipe recipe)
	{
		Creature creature =  new Creature(GetNode<CreatureDatabase>("/root/CreatureDatabase").Get(recipe.ResultCreatureId));
		// créer une scène creature

		CreatureNode newCreature = _creatureScenePath.Instantiate<CreatureNode>();
		newCreature.Init(creature);
		
		GetTree().Root.AddChild(newCreature);

		return creature;
	}
	
	public List<string> GetHintsForObjects(string objectId)
	{
		var hints = new List<string>();
		foreach (var recipe in _recipes)
		{
			if (recipe.Ingredients.Contains(objectId) && !string.IsNullOrEmpty(recipe.HintText))
			{
				hints.Add(recipe.HintText);
			}
		}
		return hints;
	}
}

public struct MergeResult
{
	public bool Success;
	public string Message;
	public Creature Creature;
}
