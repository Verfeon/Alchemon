using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MergeRecipe = Game.Merge.Data.MergeRecipe;

namespace Game.Core.Autoload;

public partial class MergeRecipeDatabase : Node
{
	[Export] private string MergeRecipeDataFolder = "res://Resources/MergeRecipes";
	private Dictionary<string, MergeRecipe> _byId;

	public override void _EnterTree()
	{
		_byId = new Dictionary<string, MergeRecipe>();
		List<MergeRecipe> recipes = LoadAllMergeRecipes();

		foreach (var recipe in recipes)
		{
			if (string.IsNullOrEmpty(recipe.Id))
			{
				GD.PrintErr("MergeRecipe without ID detected");
				continue;
			}

			if (_byId.ContainsKey(recipe.Id))
			{
				GD.PrintErr($"MergeRecipe ID duplicated: {recipe.Id}");
				continue;
			}

			_byId[recipe.Id] = recipe;
		}

		GD.Print($"Loaded {_byId.Count} MergeRecipes");
	}

	private List<MergeRecipe> LoadAllMergeRecipes()
	{
		var resources = new List<MergeRecipe>();
		var dir = DirAccess.Open(MergeRecipeDataFolder);
		if (dir == null)
		{
			GD.PrintErr($"Unable to open folder: {MergeRecipeDataFolder}");
			return resources;
		}

		dir.ListDirBegin();
		string fileName;
		while ((fileName = dir.GetNext()) != "")
		{
			if (dir.CurrentIsDir()) continue;
			if (!fileName.EndsWith(".tres") && !fileName.EndsWith(".res")) continue;

			string fullPath = MergeRecipeDataFolder + "/" + fileName;
			var res = ResourceLoader.Load<MergeRecipe>(fullPath);
			if (res != null)
				resources.Add(res);
		}
		dir.ListDirEnd();
		return resources;
	}

	public MergeRecipe Get(string id)
	{
		if (_byId == null)
		{
			GD.PrintErr("MergeRecipeDatabase not initialized");
			return null;
		}

		_byId.TryGetValue(id, out var recipe);
		if (recipe == null)
			GD.PrintErr($"Couldn't find MergeRecipe with id: {id}");

		return recipe;
	}

	public MergeRecipe GetRequired(string id)
	{
		var recipe = Get(id);
		if (recipe == null)
			throw new KeyNotFoundException($"MergeRecipe ID not found: {id}");
		return recipe;
	}
	
	public List<MergeRecipe> FindRecipes(List<string> itemIds)
	{
		List<MergeRecipe> foundRecipes = _byId.Values.ToList().FindAll(recipe =>
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
		
		foreach (var recipe in _byId.Values)
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
