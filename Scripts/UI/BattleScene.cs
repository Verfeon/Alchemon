using Godot;
using System;

public partial class BattleScene : CanvasLayer
{
	[Export] private TextureRect background;
	[Export] private Node2D playerPosition;
	[Export] private Node2D enemyPosition;
	
	public override void _Ready()
	{
		var tween = CreateTween();
		background.Modulate = new Color(Colors.White, 0);
		tween.TweenProperty(background, "modulate:a", 1, 0.5f);
	}

	public void EndBattle()
	{
		var tween = CreateTween();
		tween.TweenProperty(background, "modulate:a", 0, 0.5f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	public Node2D GetPlayerPosition()
	{
		return playerPosition;
	}

	public Node2D GetEnemyPosition()
	{
		return enemyPosition;
	}
}
