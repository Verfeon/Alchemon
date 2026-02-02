using Godot;
using System;

using CreatureType = Game.Creatures.Domain.CreatureType;
using AbilityCategory = Game.Creatures.Domain.AbilityCategory;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class Ability : Resource
{
	[Export] public string Id { get; private set; }
	[Export] public string Name { get; private set; }
	[Export] public AbilityCategory Category { get; private set;}
	[Export(PropertyHint.Range, "0,300")] public int Power { get; private set; }
	[Export] public CreatureType Type { get; private set; }
	[Export(PropertyHint.MultilineText)] public string Description { get; private set; }
}
