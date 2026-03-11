using Godot;
using System;
using System.Collections.Generic;

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
			if (abilityIdx >= 6) 
			{
				GD.PushError($"Too many abilities in creature {creature}");
			}
			_buttons[abilityIdx].Bind(ability);
			abilityIdx++;
		}
	}
	
	public List<AbilityButton> GetActiveAbilityButtons()
	{
		List<AbilityButton> activeButtons = new List<AbilityButton>();
		foreach (AbilityButton button in _buttons)
		{
			if (button.IsBound())
			{
				activeButtons.Add(button);
			}
		}
		return activeButtons;
	}
}
