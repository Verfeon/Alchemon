using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Creature = Game.Creatures.Domain.Creature;
using TypeEnum = Game.Creatures.Domain.TypeEnum;
using AbilityCategory = Game.Creatures.Domain.AbilityCategory;
using Type = Game.Creatures.Data.Type;
using Ability = Game.Creatures.Data.Ability;
using Game.Creatures.Presentation;
using System.ComponentModel;
using BattleScene = Game.Battle.UI.BattleScene;
using GameManager = Game.Core.Autoload.GameManager;
using MergeManager = Game.Merge.Domain.MergeManager;
using InventoryManager = Game.Items.Domain.InventoryManager;

namespace Game.Battle.Domain;

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
	
	private Task<Creature> WaitForCreatureMergedAsync(MergeManager mergeManager)
	{
	 	var tcs = new TaskCompletionSource<Creature>();
		
		void Handler(Creature creature)
		{
			mergeManager.CreatureMerged -= Handler;
			tcs.SetResult(creature);
		}
		
		mergeManager.CreatureMerged += Handler;
		
		return tcs.Task;
	}
	
	public async Task StartBattle(Creature enemyCreature)
	{
		GD.Print("Start Battle");
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager == null) return;
		if (gameManager.Merge == null) return;
		
		GetTree().Paused = true;
		_battleScene = _battleScenePath.Instantiate<BattleScene>();
		
		CanvasLayer overlayLayer = GetTree().Root.GetNode<CanvasLayer>("Main/OverlayLayer");

		
		CreatureNode enemyCreatureNode = _creatureScenePath.Instantiate<CreatureNode>();
		_battleScene.GetEnemyPosition().CallDeferred("add_child", enemyCreatureNode);
		enemyCreatureNode.Bind(enemyCreature);
		
		overlayLayer.AddChild(_battleScene);
		overlayLayer.MoveChild(_battleScene, 0);
		
		var ev = new InputEventAction();
		ev.Action = "open_merge_ui";
		ev.Pressed = true;
		Input.ParseInputEvent(ev);
		GD.Print("Action ui_merged Pressed");
		
		Creature playerCreature = await WaitForCreatureMergedAsync(gameManager.Merge);
		CreatureNode playerCreatureNode = _creatureScenePath.Instantiate<CreatureNode>();
		_battleScene.GetPlayerPosition().CallDeferred("add_child", playerCreatureNode);
		playerCreatureNode.Bind(playerCreature);
		
		_battleScene.Init(playerCreature, this);
		_battleScene.GetActiveAbilityButtons().ForEach(button => button.AbilitySelected += OnAbilitySelected);

		_playerCreature = playerCreature;
		_enemyCreature = enemyCreature;
		_turnCount = 0;
		
		EmitSignal(SignalName.BattleStarted);
		
		GD.Print($"Battle started! {_playerCreature.Data.Name} vs {_enemyCreature.Data.Name}");
	}
	
	private bool PlayerGoesFirst()
	{
		if (_playerCreature.RealStats.Speed == _enemyCreature.RealStats.Speed)
		{
			return GD.Randf() > 0.5f;
		}
		return _playerCreature.RealStats.Speed >= _enemyCreature.RealStats.Speed;
	}

	private bool IsAnyoneDead()
	{
		return ((_playerCreature.CurrentHP <= 0) ||(_enemyCreature.CurrentHP <= 0));
	}
	
	private void ManageTurn(Ability playerSelectedAbility)
	{
		_turnCount++;
		
		var actions = PlayerGoesFirst()
			? new Action[] { () => UseAbility(playerSelectedAbility, _playerCreature, _enemyCreature), EnemyTurn }
		: new Action[] { EnemyTurn, () => UseAbility(playerSelectedAbility, _playerCreature, _enemyCreature) };
		
		foreach (var action in actions)
		{
			action();
			
			if (IsAnyoneDead())
			{
				EndBattle(_enemyCreature.IsFainted == true);
				return;
			}
		}
		
		EmitSignal(SignalName.TurnEnded);
	}
	
	private void UseAbility(Ability ability, Creature attacker, Creature defender)
	{
		GD.Print($"{attacker.Data.Name} uses {ability.Name}!");
		
		int damage = CalculateDamage(ability, attacker, defender);
		
		defender.CurrentHP = Math.Max(0, defender.CurrentHP - damage);
		
		EmitSignal(SignalName.DamageDealt, attacker.Data.Name, defender.Data.Name, damage);
		GD.Print($"{defender.Data.Name} takes {damage} damage! HP: {defender.CurrentHP}/{defender.RealStats.MaxHP}");
	}
	
	private int CalculateDamage(Ability ability, Creature attacker, Creature defender)
	{
		int attackStat = 1;
		int defenseStat = 1;
		
		switch (ability.Category)
		{
			case AbilityCategory.Physical:
				attackStat = attacker.RealStats.Attack;
				defenseStat = defender.RealStats.Defense;
				break;
			
			case AbilityCategory.Special:
				attackStat = attacker.RealStats.SpecialAttack;
				defenseStat = defender.RealStats.SpecialDefense;
				break;
			
			case AbilityCategory.Real:
				attackStat = Math.Max(attacker.RealStats.Attack,  attacker.RealStats.SpecialAttack);
				defenseStat = 1;
				break;
			
			case AbilityCategory.Status:
				//ApplyStatusEffect
				break;
			
			default:
				throw new InvalidEnumArgumentException($"Unknown ability category: {ability.Category}");
		}
		
		
		int power = ability.Power;
		
		float baseDamage = (power * attackStat / defenseStat) / 50f + 2;
		
		float typeMultiplier = GetTypeEffectiveness(ability.Type, defender.Data.Type);
		
		float stabMultiplier = (ability.Type == attacker.Data.Type) ? 1.5f : 1.0f;
		
		int finalDamage = (int)(baseDamage * typeMultiplier * stabMultiplier);
		
		finalDamage = Math.Max(1, finalDamage);
		
		if (typeMultiplier > 1.0f)
			GD.Print("It's super effective!");
		else if (typeMultiplier < 1.0f)
			GD.Print("It's not very effective...");
		
		if (stabMultiplier > 1.0f)
			GD.Print($"STAB bonus applied! ({attacker.Data.Type} type attack)");
		
		return finalDamage;
	}
	
	private void ApplyStatusEffect(Ability ability, Creature attacker, Creature defender,
								   Dictionary<string, int> attackerMods, Dictionary<string, int> defenderMods)
	{
		// Gérer les capacités qui n'infligent pas de dégâts (buffs, debuffs, soins)
		string abilityName = ability.Name.ToLower();
		
		bool isPlayerAttacking = attacker == _playerCreature;
		var targetMods = isPlayerAttacking ? _playerStatModifiers : _enemyStatModifiers;
		
		// Buffs d'attaque
		if (abilityName.Contains("rage") || abilityName.Contains("puissance"))
		{
			targetMods["Attack"] = targetMods.GetValueOrDefault("Attack", 0) + 20;
			GD.Print($"{attacker.Data.Name}'s Attack increased!");
		}
		
		// Buffs de défense
		if (abilityName.Contains("armure") || abilityName.Contains("bouclier"))
		{
			targetMods["Defense"] = targetMods.GetValueOrDefault("Defense", 0) + 25;
			targetMods["SpecialDefense"] = targetMods.GetValueOrDefault("SpecialDefense", 0) + 25;
			GD.Print($"{attacker.Data.Name}'s Defense increased!");
		}
		
		// Soins
		if (abilityName.Contains("soin") || abilityName.Contains("photosynthèse") || 
			abilityName.Contains("réparation") || abilityName.Contains("renaissance"))
		{
			var attackerStats = attacker.RealStats;
			int healAmount = attackerStats.MaxHP * 30 / 100; // 30% des HP max
			attacker.CurrentHP = Math.Min(attackerStats.MaxHP, attacker.CurrentHP + healAmount);
			GD.Print($"{attacker.Data.Name} restored {healAmount} HP!");
		}
		
		// Debuffs sur l'ennemi
		if (abilityName.Contains("brume") || abilityName.Contains("aveugle"))
		{
			var enemyMods = isPlayerAttacking ? _enemyStatModifiers : _playerStatModifiers;
			enemyMods["Attack"] = enemyMods.GetValueOrDefault("Attack", 0) - 15;
			GD.Print($"{defender.Data.Name}'s accuracy decreased!");
		}
	}
	
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
	
	// Méthode pour l'IA (tour de l'ennemi)
	public void EnemyTurn()
	{
		if (_enemyCreature != null && _enemyCreature.Abilities.Count > 0)
		{
			// Choisir une capacité aléatoirement (IA simple)
			int randomIndex = GD.RandRange(0, _enemyCreature.Abilities.Count - 1);
			Ability chosenAbility = _enemyCreature.Abilities[randomIndex];
			
			UseAbility(chosenAbility, _enemyCreature, _playerCreature);
		}
	}
	

	private void OnAbilitySelected(Ability ability)
	{
		ManageTurn(ability);
	}
}
