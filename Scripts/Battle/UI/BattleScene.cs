using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Domain.Creature;
using AbilityPanel = Game.Battle.UI.AbilityPanel;
using BattleManager = Game.Battle.Domain.BattleManager;
using Ability = Game.Creatures.Data.Ability;
using GameManager = Game.Core.Autoload.GameManager;

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
	
	public void Init(Creature playerCreature, BattleManager battleManager)
	{
		_abilityPanel.BindButtons(playerCreature);
		GetActiveAbilityButtons().ForEach(button => button.AbilitySelected += (Ability ability) => HideUI());
		battleManager.TurnEnded += ShowUI;
	}
	
	private void ShowUI()
	{
		_abilityPanel.Visible = true;
	}
	
	private void HideUI()
	{
		_abilityPanel.Visible = false;
	}

	public void EndBattle()
	{
		var tween = CreateTween();
		tween.TweenProperty(_background, "modulate:a", 0, 0.5f);
		GetActiveAbilityButtons().ForEach(button => button.AbilitySelected -= (Ability ability) => HideUI());
		GetNode<GameManager>("/root/GameManager").Battle.TurnEnded -= ShowUI;
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

	public List<AbilityButton> GetActiveAbilityButtons()
	{
		return _abilityPanel.GetActiveAbilityButtons();
	}
}
