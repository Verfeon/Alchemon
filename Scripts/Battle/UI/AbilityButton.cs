using Godot;
using System;

using Ability = Game.Creatures.Data.Ability;

namespace Game.Battle.UI;

public partial class AbilityButton : Node2D
{
	[Export] private int _id;
	[Export] private Label _abilityLabel;
	[Export] private TextureButton _textureButton;
	[Export] private Sprite2D _iconSprite;
	
	private Ability _ability;
	
	public override void _Ready() 
	{
		GD.Print("ready");
		Bitmap bitmap = new Bitmap();
		Texture2D texture = _textureButton.GetTextureNormal();
		Image image = texture.GetImage();
		bitmap.CreateFromImageAlpha(image);
		_textureButton.SetClickMask(bitmap);
		_abilityLabel.Text = "";
	}
	
	public void Bind(Ability ability)
	{
		GD.Print("bind");
		if (ability == null)
		{
			throw new ArgumentNullException(nameof(ability));
		}
		
		if (ability == _ability) return;
		
		Unbind();
		
		_ability = ability;
		_abilityLabel.Text = _ability.Name;
		_textureButton.Modulate = ability.Type.Color;
		_iconSprite.Texture = ability.Type.Icon;
	}
	
	private void Unbind()
	{
		if (_ability == null) return;
		
		_ability = null;
		_abilityLabel.Text = "";
		_textureButton.Modulate = Colors.White;
		_iconSprite.Texture = null;
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
