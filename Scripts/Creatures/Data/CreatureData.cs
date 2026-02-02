using Godot;
using System;
using System.Collections.Generic;

using BaseCreatureStats = Game.Creatures.Data.BaseCreatureStats;
using CreatureType = Game.Creatures.Domain.CreatureType;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class CreatureData : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string Name { get; set; }
	[Export] public Texture2D Texture2d { get; set; }
	[Export] public BaseCreatureStats BaseStats { get; set; }  // intrinsic to the species
	[Export] public CreatureType Type { get; set; }
	[Export] public Godot.Collections.Array<Ability> Abilities { get; set; }
	
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
}
