using Godot;
using System.Collections.Generic;

using Ability = Game.Creatures.Data.Ability;
using Creature = Game.Creatures.Domain.Creature;

namespace Game.Battle.UI;

/// <summary>
/// Represents a panel for displaying and using the abilities of a creature in battle.
/// </summary>
public partial class AbilityPanel : Node2D
{
	[Export] private Godot.Collections.Array<AbilityButton> _buttons;
	
	/// <summary>
	/// Binds the ability buttons in the panel to the abilities of the given creature, allowing the player to select and use those abilities in battle.
	/// </summary>
	/// <param name="creature"></param>
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
	
	/// <summary>
	/// Returns a list of the ability buttons in the panel that are currently bound to an ability, indicating that they are active and can be selected by the player.
	/// </summary>
	/// <returns></returns>
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
