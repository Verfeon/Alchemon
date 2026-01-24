using Godot;
using System;

using CreatureType = Game.Enums.CreatureType;

namespace Game.Resources;

[GlobalClass]
public partial class Ability : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string Name { get; set; }
	[Export] public bool IsPhysical { get; set;}
	[Export] public int Power { get; set; }
	[Export] public CreatureType Type { get; set; }
	[Export(PropertyHint.MultilineText)] public string Description { get; set; }
}
