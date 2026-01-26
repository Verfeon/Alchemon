using Godot;
using System;

namespace Game.Creatures;

[GlobalClass]
public partial class CreatureStats : Resource
{
	[Export(PropertyHint.Range, "1,255")] public int MaxHP;
	[Export(PropertyHint.Range, "1,255")] public int Attack;
	[Export(PropertyHint.Range, "1,255")] public int Defense;
	[Export(PropertyHint.Range, "1,255")] public int SpecialAttack;
	[Export(PropertyHint.Range, "1,255")] public int SpecialDefense;
	[Export(PropertyHint.Range, "1,255")] public int Speed;
}
