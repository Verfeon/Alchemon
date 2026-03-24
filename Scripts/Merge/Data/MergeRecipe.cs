using Godot;
using System.Collections.Generic;

using MergeableItemData = Game.Items.Data.MergeableItemData;
using CreatureData = Game.Creatures.Data.CreatureData;

namespace Game.Merge.Data;

/// <summary>
/// Represents a recipe for merging items into a creature, containing the required items and the resulting creature.
/// Generates a unique ID based on the required items and result creature, and a name based on the item names and creature name if not explicitly set.
/// </summary>
[GlobalClass, Tool]
public partial class MergeRecipe : Resource
{
	[Export] public string Id { get; private set; } = "";
	[Export] public string Name { get; set; } = "";

	[Export] public Godot.Collections.Array<MergeableItemData> RequiredItems { get; set; } = new();
	[Export] public CreatureData ResultCreature { get; set; }
	
	[ExportToolButton("Refresh")]
	public Callable RefreshButton => Callable.From(Refresh);

	/// <summary>
	/// Refreshes the recipe's ID and name based on its required items and result creature.
	/// </summary>
	public void Refresh()
	{
		GenerateId();
		GenerateName();
	}

	/// <summary>
	/// Generates a unique ID for the recipe based on the IDs of the required items and the result creature, ensuring that the same combination of items and creature will always produce the same ID regardless of order.
	/// </summary>
	private void GenerateId()
	{
		if (RequiredItems.Count == 0 || ResultCreature == null)
		{
			Id = "";
			return;
		}

		var itemList = new List<string>();
		foreach (var item in RequiredItems)
		{
			if (item != null && !string.IsNullOrEmpty(item.Id))
			{
				itemList.Add(item.Id);
			}
		}

		itemList.Sort();
		Id = string.Join("_", itemList) + "_" + ResultCreature.Id;
	}

	/// <summary>
	/// Generates a human-readable name for the recipe based on the names of the required items and the result creature, in the format "Item1 + Item2 + ... -> ResultCreature".
	/// If any required item is missing a name, it will be skipped in the name generation, and if the result creature is missing a name or there is no required item, the name will be set to "Undefined Recipe".
	/// </summary>
	private void GenerateName()
	{
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
