using Godot;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class BaseCreatureStats : Resource
{
	[Export(PropertyHint.Range, "1,255")] public int MaxHP { get; private set; } = 1;
	[Export(PropertyHint.Range, "1,255")] public int Attack { get; private set; } = 1;
	[Export(PropertyHint.Range, "1,255")] public int Defense { get; private set; } = 1;
	[Export(PropertyHint.Range, "1,255")] public int SpecialAttack { get; private set; } = 1;
	[Export(PropertyHint.Range, "1,255")] public int SpecialDefense { get; private set; } = 1;
	[Export(PropertyHint.Range, "1,255")] public int Speed { get; private set; } = 1;
}
