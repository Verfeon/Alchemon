using Godot;
using System;

using Ability = Game.Creatures.Data.Ability;

namespace Game.Battle.UI;

/// <summary>
/// Represents a button in the ability panel that can be bound to a specific ability of a creature.
/// Handles the display of the ability's name, icon and the type color, as well as the click event to select the ability for use in battle.
/// </summary>
public partial class AbilityButton : Node2D
{
	[Signal] public delegate void AbilitySelectedEventHandler(Ability ability);

	[Export] private int _id;
	[Export] private Label _abilityLabel;
	[Export] private TextureButton _textureButton;
	[Export] private Sprite2D _iconSprite;
	
	private Ability _ability;
	
	public override void _Ready() 
	{
		Bitmap bitmap = new Bitmap();
		Texture2D texture = _textureButton.GetTextureNormal();
		Image image = texture.GetImage();
		bitmap.CreateFromImageAlpha(image);
		_textureButton.SetClickMask(bitmap);
		_abilityLabel.Text = "";
	}
	
	/// <summary>
	/// Binds the ability button to a specific ability.
	/// </summary>
	/// <param name="ability"></param>
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
		_textureButton.Modulate = ability.Type.Color;
		_textureButton.Disabled = false;
		_iconSprite.Texture = ability.Type.Icon;
	}
	
	/// <summary>
	/// Unbinds the ability button, clearing its display and disabling it.
	/// </summary>
	private void Unbind()
	{
		if (_ability == null) return;
		
		_ability = null;
		_abilityLabel.Text = "";
		_textureButton.Modulate = Colors.White;
		_textureButton.Disabled = true;
		_iconSprite.Texture = null;
	}

	/// <summary>
	/// Checks if the ability button is currently bound to an ability.
	/// </summary>
	/// <returns></returns>
	public bool IsBound()
	{
		return _ability != null;
	}
	
	/// <summary>
	/// Handles the button press event, emitting the AbilitySelected signal if an ability is bound to the button.
	/// </summary>
	public void OnButtonPressed() 
	{
		if (_ability != null) 
		{
			EmitSignal(SignalName.AbilitySelected, _ability);
		} else 
		{
			GD.Print("No Ability here");
		}
	}
}
