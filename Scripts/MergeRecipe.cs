using Godot;
using System;
using System.Collections.Generic;

namespace Game;

public class MergeRecipe
{
	public List<string> Ingredients { get; set; } // IDs des objets (2 objets)
	public string ResultCreatureId { get; set; }
	public bool IsDiscovered { get; set; }
	public string UnlockCondition { get; set; } // Optionnel
	public string HintText { get; set; }
}
