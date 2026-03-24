using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Domain.Creature;
using CreatureNode = Game.Creatures.Presentation.CreatureNode;
using BattleManager = Game.Battle.Domain.BattleManager;
using Ability = Game.Creatures.Data.Ability;
using GameManager = Game.Core.Autoload.GameManager;
using System.Linq;

namespace Game.Battle.UI;

/// <summary>
/// Represents the UI for the battle scene, managing the display and interaction with the player's and enemy's creatures, as well as the ability panel.
/// </summary>
public partial class BattleScene : CanvasLayer
{
	[Export] private PackedScene _creatureScenePath;

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
	
	/// <summary>
	/// Initializes the battle scene with the given battle manager.
	/// </summary>
	/// <param name="battleManager"></param>
	public void Init(BattleManager battleManager)
	{
		GetActiveAbilityButtons().ForEach(button => button.AbilitySelected += (Ability ability) => HideUI());
		battleManager.TurnEnded += ShowUI;
	}
	
	/// <summary>
	/// Shows the battle UI, allowing the player to select an ability for their creature to use in battle.
	/// </summary>
	private void ShowUI()
	{
		_abilityPanel.Visible = true;
	}
	
	/// <summary>
	/// Hides the battle UI, preventing the player from selecting an ability while a turn is being processed.
	/// </summary>
	private void HideUI()
	{
		_abilityPanel.Visible = false;
	}

	/// <summary>
	/// Cleans up the battle scene and frees it from memory when the battle ends, removing any creatures from the scene and disconnecting any event handlers to prevent memory leaks.
	/// Also plays a fade-out animation on the background to smoothly transition out of the battle scene
	/// </summary>
	public void EndBattle()
	{
		var tween = CreateTween();
		tween.TweenProperty(_background, "modulate:a", 0, 0.5f);
		GetActiveAbilityButtons().ForEach(button => button.AbilitySelected -= (Ability ability) => HideUI());
		GetNode<GameManager>("/root/GameManager").Battle.TurnEnded -= ShowUI;
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	/// <summary>
	/// Sets the player's creature and binds its abilities to the ability panel.
	/// </summary>
	/// <param name="playerCreature"></param>
	public void SetPlayerCreature(Creature playerCreature)
	{
		_playerPosition.GetChildren().ToList().ForEach(child => child.QueueFree());
		var playerNode = _creatureScenePath.Instantiate<CreatureNode>();
		playerNode.Setup(playerCreature, false);
		_playerPosition.AddChild(playerNode);

		_abilityPanel.BindButtons(playerCreature);
	}

	/// <summary>
	/// Sets the enemy's creature.
	/// </summary>
	/// <param name="enemyCreature"></param>
	public void SetEnemyCreature(Creature enemyCreature)
	{
		_enemyPosition.GetChildren().ToList().ForEach(child => child.QueueFree());
		var enemyNode = _creatureScenePath.Instantiate<CreatureNode>();
		enemyNode.Setup(enemyCreature, false);
		_enemyPosition.AddChild(enemyNode);
	}

	/// <summary>
	/// Returns the list of active ability buttons in the ability panel.
	/// </summary>
	/// <returns></returns>
	public List<AbilityButton> GetActiveAbilityButtons()
	{
		return _abilityPanel.GetActiveAbilityButtons();
	}
}
