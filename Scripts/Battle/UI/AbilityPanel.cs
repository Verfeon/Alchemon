using Godot;
using System;

using Ability = Game.Creatures.Data.Ability;
using Creature = Game.Creatures.Domain.Creature;

namespace Game.Battle.UI;

public partial class AbilityPanel : Node2D
{
	[Export] private Godot.Collections.Array<AbilityButton> _buttons;
	
	public void BindButtons(Creature creature)
	{
		int abilityIdx = 0;
		foreach (Ability ability in creature.GetAbilities())
		{
			_buttons[abilityIdx].Bind(ability);
			abilityIdx++;
			if (abilityIdx >= 6) 
			{
				GD.PushError($"Too many abilities in creature {creature}");
			}
		}
	}
}
