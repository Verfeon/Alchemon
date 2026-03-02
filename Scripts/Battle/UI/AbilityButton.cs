using Godot;
using System;

using Ability = Game.Creatures.Data.Ability;

namespace Game.Battle.UI;

public partial class AbilityButton : Node2D
{
	[Export] private int _id;
	[Export] private Label _abilityLabel;
	
	private Ability _ability;
	
	public void Bind(Ability ability)
	{
		if (ability == null)
		{
			throw new ArgumentNullException(nameof(ability));
		}
		
		if (ability == _ability) return;
		
		Unbind();
		
		_ability = ability;
		_abilityLabel.Text = _ability.Name;
	}
	
	private void Unbind()
	{
		if (_ability == null) return;
		
		_ability = null;
		_abilityLabel.Text = "";
	}
	
	public void OnButtonPressed() 
	{
		if (_ability != null) 
		{
			GD.Print($"ability : {_ability.Name}");
		} else 
		{
			GD.Print("No Ability here");
		}
	}
}
