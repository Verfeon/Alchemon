using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Domain.Creature;
using CreatureType = Game.Creatures.Domain.CreatureType;
using Ability = Game.Creatures.Data.Ability;

namespace Game.Battle.Domain;

public partial class BattleManager : Node
{
	private Creature _playerCreature;
	private Creature _enemyCreature;
	private bool _isPlayerTurn = true;
	private int _turnCount = 0;
	
	// Modificateurs de stats temporaires
	private Dictionary<string, int> _playerStatModifiers = new();
	private Dictionary<string, int> _enemyStatModifiers = new();
	
	[Signal]
	public delegate void BattleStartedEventHandler();
	
	[Signal]
	public delegate void BattleEndedEventHandler(bool playerWon);
	
	[Signal]
	public delegate void TurnStartedEventHandler(bool isPlayerTurn);
	
	[Signal]
	public delegate void DamageDealtEventHandler(string attackerName, string defenderName, int damage);
	
	[Signal]
	public delegate void CreatureFaintedEventHandler(string creatureName);
	
	public void StartBattle(Creature playerCreature, Creature enemyCreature)
	{
		_playerCreature = playerCreature;
		_enemyCreature = enemyCreature;
		_turnCount = 0;
		
		// Réinitialiser les modificateurs
		_playerStatModifiers.Clear();
		_enemyStatModifiers.Clear();
		
		// Déterminer qui commence
		_isPlayerTurn = DetermineFirstTurn();
		
		EmitSignal(SignalName.BattleStarted);
		EmitSignal(SignalName.TurnStarted, _isPlayerTurn);
		
		GD.Print($"Battle started! {_playerCreature.Data.Name} vs {_enemyCreature.Data.Name}");
	}
	
	private bool DetermineFirstTurn()
	{
		// Si vitesse égale, c'est aléatoire
		if (_playerCreature.RealStats.Speed == _enemyCreature.RealStats.Speed)
		{
			return GD.Randf() > 0.5f;
		}
		return _playerCreature.RealStats.Speed >= _enemyCreature.RealStats.Speed;
	}
	
	public void UseAbility(Ability ability, Creature attacker, Creature defender)
	{
		_turnCount++;
		
		bool isPlayerAttacking = attacker == _playerCreature;
		var attackerMods = isPlayerAttacking ? _playerStatModifiers : _enemyStatModifiers;
		var defenderMods = isPlayerAttacking ? _enemyStatModifiers : _playerStatModifiers;
		
		// Vérifier si l'attaquant a assez de mana (si implémenté)
		// Pour l'instant, on suppose que oui
		
		GD.Print($"{attacker.Data.Name} uses {ability.Name}!");
		
		// Calculer les dégâts
		int damage = CalculateDamage(ability, attacker, defender, attackerMods, defenderMods);
		
		// Appliquer les dégâts
		var defenderStats = defender.RealStats;
		defender.CurrentHP = Math.Max(0, defender.CurrentHP - damage);
		
		EmitSignal(SignalName.DamageDealt, attacker.Data.Name, defender.Data.Name, damage);
		GD.Print($"{defender.Data.Name} takes {damage} damage! HP: {defender.CurrentHP}/{defenderStats.MaxHP}");
		
		// Vérifier si le défenseur est KO
		if (defender.CurrentHP <= 0)
		{
			EmitSignal(SignalName.CreatureFainted, defender.Data.Name);
			EndBattle(defender == _enemyCreature);
			return;
		}
		
		// Passer au tour suivant
		_isPlayerTurn = !_isPlayerTurn;
		EmitSignal(SignalName.TurnStarted, _isPlayerTurn);
	}
	
	private int CalculateDamage(Ability ability, Creature attacker, Creature defender, 
								Dictionary<string, int> attackerMods, Dictionary<string, int> defenderMods)
	{
		// Déterminer si c'est une attaque physique ou spéciale
		bool isPhysical = true; //IsPhysicalAbility(ability);
		
		int attackStat;
		int defenseStat;
		
		if (isPhysical)
		{
			// Attaque physique : utilise Attack vs Defense
			attackStat = attacker.RealStats.Attack;
			defenseStat = defender.RealStats.Defense;
			
			// Appliquer les modificateurs
			if (attackerMods.ContainsKey("Attack"))
				attackStat += attackerMods["Attack"];
			if (defenderMods.ContainsKey("Defense"))
				defenseStat += defenderMods["Defense"];
		}
		else
		{
			// Attaque spéciale : utilise SpecialAttack vs SpecialDefense
			attackStat = attacker.RealStats.SpecialAttack;
			defenseStat = defender.RealStats.SpecialDefense;
			
			// Appliquer les modificateurs
			if (attackerMods.ContainsKey("SpecialAttack"))
				attackStat += attackerMods["SpecialAttack"];
			if (defenderMods.ContainsKey("SpecialDefense"))
				defenseStat += defenderMods["SpecialDefense"];
		}
		
		// Formule de dégâts inspirée de Pokémon
		// Damage = ((2 * Level / 5 + 2) * Power * Attack / Defense / 50 + 2) * Modifiers
		
		int level = 1; // Pour l'instant, on suppose que toutes les créatures sont au niveau 1
		int power = ability.Power;
		
		// Si la capacité n'inflige pas de dégâts (buff/debuff)
		if (power == 0)
		{
			ApplyStatusEffect(ability, attacker, defender, attackerMods, defenderMods);
			return 0;
		}
		
		// Calcul de base
		float baseDamage = ((2f * level / 5f + 2f) * power * attackStat / defenseStat / 50f + 2f);
		
		// Multiplicateur de type
		float typeMultiplier = GetTypeEffectiveness(ability.Type, defender.Data.Type);
		
		// STAB (Same Type Attack Bonus) - bonus si le type de l'attaque correspond au type de la créature
		float stabMultiplier = (ability.Type == attacker.Data.Type) ? 1.5f : 1.0f;
		
		// Variation aléatoire (85% - 100%)
		float randomFactor = (float)GD.RandRange(0.85, 1.0);
		
		// Calcul final
		int finalDamage = (int)(baseDamage * typeMultiplier * stabMultiplier * randomFactor);
		
		// Minimum 1 dégât
		finalDamage = Math.Max(1, finalDamage);
		
		// Messages de type
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
	
	private float GetTypeEffectiveness(CreatureType attackType, CreatureType defenseType)
	{
		// Table des efficacités de types
		// Super efficace = 1.5x, Normal = 1.0x, Peu efficace = 0.75x
		
		switch (attackType)
		{
			case CreatureType.Fire:
				if (defenseType == CreatureType.Nature || defenseType == CreatureType.Tech)
					return 1.5f;
				if (defenseType == CreatureType.Water || defenseType == CreatureType.Earth)
					return 0.75f;
				break;
				
			case CreatureType.Water:
				if (defenseType == CreatureType.Fire || defenseType == CreatureType.Earth)
					return 1.5f;
				if (defenseType == CreatureType.Nature || defenseType == CreatureType.Air)
					return 0.75f;
				break;
				
			case CreatureType.Earth:
				if (defenseType == CreatureType.Air || defenseType == CreatureType.Tech)
					return 1.5f;
				if (defenseType == CreatureType.Nature || defenseType == CreatureType.Water)
					return 0.75f;
				break;
				
			case CreatureType.Air:
				if (defenseType == CreatureType.Water || defenseType == CreatureType.Nature)
					return 1.5f;
				if (defenseType == CreatureType.Earth || defenseType == CreatureType.Tech)
					return 0.75f;
				break;
				
			case CreatureType.Nature:
				if (defenseType == CreatureType.Water || defenseType == CreatureType.Earth)
					return 1.5f;
				if (defenseType == CreatureType.Fire || defenseType == CreatureType.Air)
					return 0.75f;
				break;
				
			case CreatureType.Light:
				if (defenseType == CreatureType.Dark)
					return 1.5f;
				break;
				
			case CreatureType.Dark:
				if (defenseType == CreatureType.Light)
					return 1.5f;
				break;
				
			case CreatureType.Tech:
				if (defenseType == CreatureType.Earth || defenseType == CreatureType.Air)
					return 1.5f;
				if (defenseType == CreatureType.Fire || defenseType == CreatureType.Water)
					return 0.75f;
				break;
		}
		
		return 1.0f; // Dégâts normaux
	}
	
	private void EndBattle(bool playerWon)
	{
		GD.Print($"Battle ended! {(playerWon ? "Victory!" : "Defeat...")}");
		
		// if (playerWon)
		// {
		// 	// Donner de l'expérience à la créature du joueur
		// 	int expGained = CalculateExpReward(_enemyCreature);
		// 	_playerCreature.Experience += expGained;
		// 	GD.Print($"{_playerCreature.Name} gained {expGained} EXP!");
			
		// 	// Vérifier si level up
		// 	CheckLevelUp(_playerCreature);
		// }
		
		EmitSignal(SignalName.BattleEnded, playerWon);
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
		if (!_isPlayerTurn && _enemyCreature != null && _enemyCreature.Abilities.Count > 0)
		{
			// Choisir une capacité aléatoirement (IA simple)
			int randomIndex = GD.RandRange(0, _enemyCreature.Abilities.Count - 1);
			Ability chosenAbility = _enemyCreature.Abilities[randomIndex];
			
			UseAbility(chosenAbility, _enemyCreature, _playerCreature);
		}
	}
}
