using Godot;
using System;

public partial class BattleScene : CanvasLayer
{
	[Export] TextureRect background;
	
	public override void _Ready()
	{
		var tween = CreateTween();
		background.Modulate = new Color(Colors.White, 0);
		tween.TweenProperty(background, "modulate:a", 1, 0.5f);
	}
}
