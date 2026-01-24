using Godot;
using System;
using System.Collections.Generic;

using CreatureStats = Game.Creatures.CreatureStats;
using CreatureType = Game.Enums.CreatureType;

namespace Game.Resources;

[GlobalClass]
public partial class CreatureData : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string Name { get; set; }
	[Export] public Texture2D Texture2d { get; set; }
	[Export] public CreatureStats BaseStats { get; set; }  // intrinsic to the species
	[Export] public CreatureType Type { get; set; }
	[Export] public Godot.Collections.Array<Ability> Abilities { get; set; }
	public CreatureStats RealStats { get; set; }           // computed on merge
	
	public void Validate()
	{
		GD.Print("enter notification");
		RemoveDuplicateIds();
	}

	private void RemoveDuplicateIds()
	{
		GD.Print("enter remove duplicate");
		var seenIds = new HashSet<string>();
		var unique = new Godot.Collections.Array<Ability>();

		foreach (var ability in Abilities)
		{
			if (seenIds.Contains(ability.Id))
			{
				GD.PushWarning($"Duplicate ability ID '{ability.Id}' found in CreatureData '{Id}'. It will be removed.");
				continue;
			}

			if (ability == null)
				continue;

			if (string.IsNullOrEmpty(ability.Id))
				continue;

			seenIds.Add(ability.Id);
			unique.Add(ability);
		}
		Abilities = unique;
	}
}
