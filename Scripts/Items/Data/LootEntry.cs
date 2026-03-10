using Godot;
using System;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class LootEntry : Resource
{
	[Export] public BaseItemData Item { get; private set; }
	[Export(PropertyHint.Range, "0, 1, 0.001")] public float Probability { get; private set; } = 0.5f;
}
