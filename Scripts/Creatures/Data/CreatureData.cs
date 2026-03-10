using Godot;
using System;
using System.Collections.Generic;

using BaseCreatureStats = Game.Creatures.Data.BaseCreatureStats;
using Type = Game.Creatures.Data.Type;
using BaseItemData = Game.Items.Data.BaseItemData;
using LootEntry = Game.Items.Data.LootEntry;
using InventoryManager = Game.Items.Domain.InventoryManager;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class CreatureData : Resource
{
	[Export] public string Id { get; private set; }
	[Export] public string Name { get; private set; }
	[Export] public Texture2D Texture2d { get; private set; }
	[Export] public BaseCreatureStats BaseStats { get; private set; }
	[Export] public Type Type { get; private set; }
	[Export] public Godot.Collections.Array<Ability> Abilities { get; private set; }
	[Export] public Godot.Collections.Array<LootEntry> LootItems { get ; private set; } = new();
	
	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		if ((string)property["name"] == nameof(Abilities))
		{	
			RemoveDuplicateAbilities();
		}
	}

	private void RemoveDuplicateAbilities()
	{
		var seenIds = new HashSet<string>();
		var unique = new Godot.Collections.Array<Ability>();

		if (Abilities == null) return;
		foreach (var ability in Abilities)
		{
			if (ability == null)
			{
				continue;
			}
			
			if (seenIds.Contains(ability.Id))
			{
				GD.PushWarning($"Duplicate ability ID '{ability.Id}' found in CreatureData '{Id}'. It will be removed.");
				continue;
			}

			if (string.IsNullOrEmpty(ability.Id))
			{
				GD.PushWarning($"Ability without ID found in CreatureData '{Id}'. It will be removed.");
				continue;
			}	

			seenIds.Add(ability.Id);
			unique.Add(ability);
		}
		Abilities = unique;
	}
	
	public void RollLoot(InventoryManager inventoryManager)
	{
		GD.Print("Try looting items");
		
		if (LootItems == null || LootItems.Count == 0) return;
		
		var rnd = new RandomNumberGenerator();
		rnd.Randomize();
		
		foreach (LootEntry lootEntry in LootItems)
		{
			if (rnd.Randf() <= lootEntry.Probability)
			{
				inventoryManager.AddItem(lootEntry.Item);
				GD.Print($"Loot {lootEntry.Item.Name}");
			}
		}
	}
}
