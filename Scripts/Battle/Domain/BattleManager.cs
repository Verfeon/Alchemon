using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;

using Creature = Game.Creatures.Domain.Creature;
using StatEnum = Game.Creatures.Domain.StatEnum;
using AbilityCategory = Game.Creatures.Domain.AbilityCategory;
using Type = Game.Creatures.Data.Type;
using Ability = Game.Creatures.Data.Ability;
using BattleScene = Game.Battle.UI.BattleScene;
using GameManager = Game.Core.Autoload.GameManager;
using MergeManager = Game.Merge.Domain.MergeManager;
using InventoryManager = Game.Items.Domain.InventoryManager;
using System.Linq;

namespace Game.Battle.Domain;

/// <summary>
/// Manages the battle logic, including turn order, ability usage, damage calculation and battle flow.
/// Handles the interaction between the player's creature and the enemy creature, as well as the integration with the merge system to allow the player to merge a new creature if their current creature faints during battle.
/// </summary>
public partial class BattleManager : Node
{
	[Export] private PackedScene _battleScenePath;
	[Export] private PackedScene _creatureScenePath;
	
	private Creature _playerCreature;
	private Creature _enemyCreature;
	private BattleScene _battleScene;
	private int _turnCount = 0;
	
	private Dictionary<string, int> _playerStatModifiers = new();
	private Dictionary<string, int> _enemyStatModifiers = new();

	[Signal]
	public delegate void BattleStartedEventHandler();
	
	[Signal]
	public delegate void BattleEndedEventHandler(bool playerWon);
	
	[Signal]
	public delegate void TurnEndedEventHandler();
	
	[Signal]
	public delegate void DamageDealtEventHandler(string attackerName, string defenderName, int damage);
	
	[Signal]
	public delegate void CreatureFaintedEventHandler(string creatureName);
	
	/// <summary>
	/// Waits for the player to merge a creature and returns the merged creature once the merge is completed. 
	/// The method listens for the CreatureMerged event from the MergeManager and resolves the task when the event is triggered. 
	/// It also simulates an input event to open the merge UI, allowing the player to perform the merge action. 
	/// This method is used in the battle flow to allow the player to merge a new creature if their current creature faints during battle.
	/// </summary>
	/// <param name="mergeManager"></param>
	/// <returns></returns>
	private Task<Creature> WaitForCreatureMergedAsync(MergeManager mergeManager)
	{
	 	var tcs = new TaskCompletionSource<Creature>();
		
		var ev = new InputEventAction // open merge UI
		{
			Action = "open_merge_ui",
			Pressed = true
		};
		Input.ParseInputEvent(ev);

		void Handler(Creature creature)
		{
			mergeManager.CreatureMerged -= Handler;
			tcs.SetResult(creature);
		}
		
		mergeManager.CreatureMerged += Handler;
		
		return tcs.Task;
	}
	
	/// <summary>
	/// Starts a new battle against the specified enemy creature.
	/// </summary>
	/// <param name="enemyCreature"></param>
	/// <returns></returns>
	public async Task StartBattle(Creature enemyCreature)
	{
		GD.Print("Start Battle");
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager == null) return;
		if (gameManager.Merge == null) return;
		
		GetTree().Paused = true;
		_battleScene = _battleScenePath.Instantiate<BattleScene>();
		
		CanvasLayer overlayLayer = GetTree().Root.GetNode<CanvasLayer>("Main/OverlayLayer");

		_battleScene.SetEnemyCreature(enemyCreature);
		
		overlayLayer.CallDeferred(MethodName.AddChild, _battleScene);
		overlayLayer.CallDeferred(MethodName.MoveChild, _battleScene, 0);
		
		Creature playerCreature = await WaitForCreatureMergedAsync(gameManager.Merge);
		_battleScene.SetPlayerCreature(playerCreature);
		
		_battleScene.Init(this);
		_battleScene.GetActiveAbilityButtons().ForEach(button => button.AbilitySelected += OnAbilitySelected);

		_playerCreature = playerCreature;
		_enemyCreature = enemyCreature;
		_turnCount = 0;
		
		EmitSignal(SignalName.BattleStarted);
		
		GD.Print($"Battle started! {_playerCreature.Data.Name} vs {_enemyCreature.Data.Name}");
	}
	
	/// <summary>
	/// Determines if the player starts first based on the speed stats of the player's creature and the enemy creature.
	/// If the speed stats are equal, it randomly decides who goes first.
	/// </summary>
	/// <returns></returns>
	private bool PlayerGoesFirst()
	{
		if (_playerCreature.GetBattleStat(StatEnum.Speed) == _enemyCreature.GetBattleStat(StatEnum.Speed))
		{
			return GD.Randf() > 0.5f;
		}
		return _playerCreature.GetBattleStat(StatEnum.Speed) >= _enemyCreature.GetBattleStat(StatEnum.Speed);
	}
	
	/// <summary>
	/// Manages the turn order and actions for the battle.
	/// Checks for creature fainting and handles the end of the battle, including victory and defeat conditions.
	/// If the player's creature faints, it checks if a merge is possible with the unused items in the inventory and allows the player to merge a new creature to continue the battle.
	/// </summary>
	/// <param name="playerSelectedAbility"></param>
	/// <returns></returns>
	private async Task ManageTurn(Ability playerSelectedAbility)
	{
		_turnCount++;
		GD.Print($"Turn {_turnCount}");
		
		var actions = PlayerGoesFirst()
			? new Action[] { () => UseAbility(playerSelectedAbility, _playerCreature, _enemyCreature), EnemyTurn }
		: new Action[] { EnemyTurn, () => UseAbility(playerSelectedAbility, _playerCreature, _enemyCreature) };
		
		foreach (var action in actions)
		{
			action();
			
			if (_playerCreature.CurrentHP <= 0)
			{
				GameManager gameManager = GetNode<GameManager>("/root/GameManager");
				MergeManager mergeManager = gameManager.Merge;
				InventoryManager inventoryManager = gameManager.Inventory;
				var unusedIds = inventoryManager.GetUnusedMergeableItems().ToList().ConvertAll(item => item.Data.Id);
				if (mergeManager.IsMergePossible(unusedIds))
				{
					_playerCreature = await WaitForCreatureMergedAsync(gameManager.Merge);
					_battleScene.SetPlayerCreature(_playerCreature);

					break;
				}
				else
				{
					EndBattle(false);
					return;
				}
			}

			if (_enemyCreature.CurrentHP <= 0)
			{
				EndBattle(true);
				return;
			}
		}
		
		EmitSignal(SignalName.TurnEnded);
	}
	
	/// <summary>
	/// Uses the specified ability from the attacker creature on the defender creature.
	/// </summary>
	/// <param name="ability"></param>
	/// <param name="attacker"></param>
	/// <param name="defender"></param>
	private void UseAbility(Ability ability, Creature attacker, Creature defender)
	{
		GD.Print($"{attacker.Data.Name} uses {ability.Name}!");
		
		if (ability.Category != AbilityCategory.Status)
		{
			int damage = CalculateDamage(ability, attacker, defender);
			
			defender.CurrentHP = Math.Max(0, defender.CurrentHP - damage);
			
			EmitSignal(SignalName.DamageDealt, attacker.Data.Name, defender.Data.Name, damage);
			GD.Print($"{defender.Data.Name} takes {damage} damage! HP: {defender.CurrentHP}/{defender.GetBattleStat(StatEnum.MaxHP)}");
		}
		ability.Effect?.ApplyEffect(attacker, defender);
	}
	
	/// <summary>
	/// Calculates the damage dealt by an ability from the attacker to the defender based on the ability's category, power, type effectiveness, and STAB (Same Type Attack Bonus).
	/// </summary>
	/// <param name="ability"></param>
	/// <param name="attacker"></param>
	/// <param name="defender"></param>
	/// <returns></returns>
	private int CalculateDamage(Ability ability, Creature attacker, Creature defender)
	{
		int attackStat = 1;
		int defenseStat = 1;
		
		switch (ability.Category)
		{
			case AbilityCategory.Physical:
				attackStat = attacker.GetBattleStat(StatEnum.Attack);
				defenseStat = defender.GetBattleStat(StatEnum.Defense);
				break;
			
			case AbilityCategory.Special:
				attackStat = attacker.GetBattleStat(StatEnum.SpecialAttack);
				defenseStat = defender.GetBattleStat(StatEnum.SpecialDefense);
				break;
			
			case AbilityCategory.Real:
				attackStat = Math.Max(attacker.GetBattleStat(StatEnum.Attack),  attacker.GetBattleStat(StatEnum.SpecialAttack));
				defenseStat = 1;
				break;
			
			case AbilityCategory.Status:
				break;
			
			default:
				throw new InvalidEnumArgumentException($"Unknown ability category: {ability.Category}");
		}
		
		
		int power = ability.Power;
		
		float baseDamage = (power * attackStat / defenseStat) / 50f;
		
		float typeMultiplier = GetTypeEffectiveness(ability.Type, defender.Data.Type);
		
		float stabMultiplier = (ability.Type == attacker.Data.Type) ? 1.5f : 1.0f;
		
		int finalDamage = (int)Math.Ceiling(baseDamage * typeMultiplier * stabMultiplier);
		
		if (typeMultiplier > 1.0f)
			GD.Print("It's super effective!");
		else if (typeMultiplier < 1.0f)
			GD.Print("It's not very effective...");
		
		if (stabMultiplier > 1.0f)
			GD.Print($"STAB bonus applied! ({attacker.Data.Type} type attack)");
		
		return finalDamage;
	}
	
	/// <summary>
	/// Gets the effectiveness of an attack type against a defense type.
	/// </summary>
	/// <param name="attackType"></param>
	/// <param name="defenseType"></param>
	/// <returns></returns>
	private float GetTypeEffectiveness(Type attackType, Type defenseType)
	{
		if (attackType.IsStrongAgainst(defenseType))
		{
			return 2f;
		}
		else if (attackType.IsWeakAgainst(defenseType))
		{
			return 0.5f;
		}
		else if (attackType.IsUselessAgainst(defenseType))
		{
			return 0f;
		}
		
		return 1.0f;
	}
	
	/// <summary>
	/// Ends the battle and handles the aftermath.
	/// </summary>
	/// <param name="playerWon"></param>
	private void EndBattle(bool playerWon)
	{
		GD.Print($"Battle ended! {(playerWon ? "Victory!" : "Defeat...")}");
		
		 if (playerWon)
		 {
			InventoryManager inventoryManager = GetNode<GameManager>("/root/GameManager").Inventory;
			_enemyCreature.Data.RollLoot(inventoryManager);
		 }
		
		EmitSignal(SignalName.BattleEnded, playerWon);
		_battleScene.GetActiveAbilityButtons().ForEach(button => button.AbilitySelected -= OnAbilitySelected);
		_battleScene.EndBattle();
		_battleScene = null;
		GetTree().Paused = false;
	}
	
	// private int CalculateExpReward(Creature defeatedCreature)
	// {
	// 	// Formule : (niveau de l'ennemi * 50) + bonus de rareté
	// 	int baseExp = defeatedCreature.Level * 50;
		
	// 	// Bonus selon les stats totales
	// 	int totalStats = defeatedCreature.Stats.MaxHP + defeatedCreature.Stats.Attack + 
	// 					defeatedCreature.Stats.Defense + defeatedCreature.Stats.SpecialAttack +
	// 					defeatedCreature.Stats.SpecialDefense + defeatedCreature.Stats.Speed;
		
	// 	int bonus = totalStats / 10;
		
	// 	return baseExp + bonus;
	// }
	
	// private void CheckLevelUp(Creature creature)
	// {
	// 	// Formule simple : EXP requis = niveau^3
	// 	int expRequired = (int)Math.Pow(creature.Level + 1, 3);
		
	// 	while (creature.Experience >= expRequired)
	// 	{
	// 		creature.Level++;
	// 		GD.Print($"{creature.Name} leveled up to level {creature.Level}!");
			
	// 		// Augmenter les stats
	// 		var stats = creature.Stats;
	// 		stats.MaxHP += 5;
	// 		stats.CurrentHP = stats.MaxHP; // Soigner lors du level up
	// 		stats.Attack += 3;
	// 		stats.Defense += 3;
	// 		stats.SpecialAttack += 3;
	// 		stats.SpecialDefense += 3;
	// 		stats.Speed += 2;
	// 		creature.Stats = stats;
			
	// 		GD.Print($"Stats increased! HP: {stats.MaxHP}, ATK: {stats.Attack}, DEF: {stats.Defense}");
	// 		GD.Print($"SP.ATK: {stats.SpecialAttack}, SP.DEF: {stats.SpecialDefense}, SPD: {stats.Speed}");
			
	// 		expRequired = (int)Math.Pow(creature.Level + 1, 3);
	// 	}
	// }
	
	/// <summary>
	/// Performs the enemy's turn by selecting a random ability from the enemy creature's abilities.
	/// </summary>
	private void EnemyTurn()
	{
		if (_enemyCreature != null && _enemyCreature.Abilities.Count > 0)
		{
			int randomIndex = GD.RandRange(0, _enemyCreature.Abilities.Count - 1);
			Ability chosenAbility = _enemyCreature.Abilities[randomIndex];
			
			UseAbility(chosenAbility, _enemyCreature, _playerCreature);
		}
	}
	
	/// <summary>
	/// Handles the selection of an ability by the player.
	/// </summary>
	/// <param name="ability"></param>
	private void OnAbilitySelected(Ability ability)
	{
        _ = ManageTurn(ability);
	}
}
