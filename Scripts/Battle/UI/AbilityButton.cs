using Godot;
using System;

using Ability = Game.Creatures.Data.Ability;

namespace Game.Battle.UI;

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
	
	private void Unbind()
	{
		if (_ability == null) return;
		
		_ability = null;
		_abilityLabel.Text = "";
		_textureButton.Modulate = Colors.White;
		_textureButton.Disabled = true;
		_iconSprite.Texture = null;
	}

	public bool IsBound()
	{
		return _ability != null;
	}
	
	public void OnButtonPressed() 
	{
		if (_ability != null) 
		{
			GD.Print($"ability : {_ability.Name}");
			EmitSignal(SignalName.AbilitySelected, _ability);
		} else 
		{
			GD.Print("No Ability here");
		}
	}
}
