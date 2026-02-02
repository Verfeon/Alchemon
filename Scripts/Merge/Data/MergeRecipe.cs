using Godot;
using System;
using System.Collections.Generic;

using MergeableItemData = Game.Items.Data.MergeableItemData;
using CreatureData = Game.Creatures.Data.CreatureData;

namespace Game.Merge.Data;

[GlobalClass, Tool]
public partial class MergeRecipe : Resource
{
	[Export] public string Id { get; private set; } = "";
	[Export] public string Name { get; set; } = "";

	[Export] public Godot.Collections.Array<MergeableItemData> RequiredItems { get; set; } = new();
	[Export] public CreatureData ResultCreature { get; set; }
	
	[ExportToolButton("Refresh")]
	public Callable RefreshButton => Callable.From(Refresh);

	public void Refresh()
	{
		GenerateId();
		GenerateName();
	}

	private void GenerateId()
	{
		if (RequiredItems.Count == 0 || ResultCreature == null)
		{
			Id = "";
			return;
		}

		// Convertir en List pour trier
		var itemList = new List<string>();
		foreach (var item in RequiredItems)
		{
			if (item != null && !string.IsNullOrEmpty(item.Id))
				itemList.Add(item.Id);
		}

		itemList.Sort(); // tri alphabétique pour un ID stable
		Id = string.Join("_", itemList) + "_" + ResultCreature.Id;
	}

	private void GenerateName()
	{
		if (!string.IsNullOrEmpty(Name)) return;

		if (RequiredItems.Count == 0 || ResultCreature == null)
		{
			Name = "Undefined Recipe";
			return;
		}

		var itemNames = new List<string>();
		foreach (var item in RequiredItems)
		{
			if (item != null && !string.IsNullOrEmpty(item.Name))
				itemNames.Add(item.Name);
		}

		Name = string.Join(" + ", itemNames) + " → " + ResultCreature.Name;
	}
}
