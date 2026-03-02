using Godot;
using System;

using Creature = Game.Creatures.Domain.Creature;
using AbilityPanel = Game.Battle.UI.AbilityPanel;

namespace Game.Battle.UI;

public partial class BattleScene : CanvasLayer
{
	[Export] private TextureRect _background;
	[Export] private Node2D _playerPosition;
	[Export] private Node2D _enemyPosition;
	[Export] private AbilityPanel _abilityPanel;
	
	public override void _Ready()
	{
		var tween = CreateTween();
		_background.Modulate = new Color(Colors.White, 0);
		tween.TweenProperty(_background, "modulate:a", 1, 0.5f);
	}
	
	public void Init(Creature playerCreature)
	{
		_abilityPanel.BindButtons(playerCreature);
	}

	public void EndBattle()
	{
		var tween = CreateTween();
		tween.TweenProperty(_background, "modulate:a", 0, 0.5f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	public Node2D GetPlayerPosition()
	{
		return _playerPosition;
	}

	public Node2D GetEnemyPosition()
	{
		return _enemyPosition;
	}
}
