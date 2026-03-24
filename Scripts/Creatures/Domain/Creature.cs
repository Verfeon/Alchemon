using System;
using System.Collections.Generic;

using CreatureData = Game.Creatures.Data.CreatureData;
using Ability = Game.Creatures.Data.Ability;
using IRandom = Game.Utils.Random.IRandom;

namespace Game.Creatures.Domain;

/// <summary>
/// Represents a creature in the game. 
/// It is initialized with a CreatureData which defines its base stats, abilities, and other properties.
/// The creature's current HP and stat modifiers can change during battle, and it can faint when its HP reaches 0.
/// </summary>
public partial class Creature
{
	public event Action<int> HpChanged;
	public event Action<Creature> Fainted;
	
	private const int MaxAbilities = 6;
	
	private int _currentHP;
	private RuntimeCreatureStats _realStats { get; }   
	private CreatureStatModifiers _statModifiers { get; } = new();
	
	public CreatureData Data { get; }
	
	public int CurrentHP
	{
		get => _currentHP;
		set
		{
			int clamped = Math.Clamp(value, 0, _realStats.Get(StatEnum.MaxHP));
			if (_currentHP == clamped) return;

			_currentHP = clamped;
			HpChanged?.Invoke(_currentHP);

			if (_currentHP == 0 && !IsFainted)
			{
				IsFainted = true;
				Fainted?.Invoke(this);
			}
		}
	}
	
	public bool IsFainted { get; private set; }
	
	public IReadOnlyList<Ability> Abilities => _abilities;
	private readonly List<Ability> _abilities;
	private IRandom _rng;
	
	public Creature(CreatureData data, int level, IRandom rng)
	{
		if (data == null)
		{
			throw new ArgumentNullException(nameof(data));
		}
		
		Data = data;
		_rng = rng;
		_realStats = new RuntimeCreatureStats(Data.BaseStats, level);
		IsFainted = false;
		_currentHP = _realStats.Get(StatEnum.MaxHP);

		_abilities = new List<Ability>(MaxAbilities);
		InitializeAbilities();
	}
	
	/// <summary>
	/// Chooses a random set of abilities for the creature among the ones defined in its CreatureData, up to the maximum allowed.
	/// This is done by shuffling the list of available abilities and taking the first ones, ensuring that the creature has a random but valid set of abilities each time it is created.
	/// </summary>
	private void InitializeAbilities()
	{
		if (Data.Abilities == null || Data.Abilities.Count == 0) return;

		var pool = new List<Ability>(Data.Abilities.Count);
		foreach (var ability in Data.Abilities)
		{
			pool.Add(ability);
		}
		
		// Shuffle (Fisher–Yates)
		for (int i = pool.Count - 1; i > 0; i--)
		{
			int j = _rng.NextInt(0, i);
			(pool[i], pool[j]) = (pool[j], pool[i]);
		}
		
		int count = Math.Min(MaxAbilities, pool.Count);
		for (int i = 0; i < count; i++)
		{
			_abilities.Add(pool[i]);
		}
	}
	
	public List<Ability> GetAbilities ()
	{
		return _abilities;
	}
	
	/// <summary>
	/// Gets the current value of a battle stat (like Attack or Defense) for the creature, taking into account its real stats and any active modifiers.
	/// The real stat is calculated based on the creature's base stats and level, while the modifiers can be changed during battle by certain abilities or effects, allowing for dynamic changes in the creature's performance.
	/// </summary>
	/// <param name="stat"></param>
	/// <returns></returns> <summary>
	/// 
	/// </summary>
	/// <param name="stat"></param>
	/// <returns></returns>
	public int GetBattleStat(StatEnum stat)
	{
		int realStat = _realStats.Get(stat);
		double multiplier = _statModifiers.GetMultiplier(stat);
		
		return (int)Math.Round(realStat * multiplier, MidpointRounding.AwayFromZero);
	}

	/// <summary>
	/// Changes the modifier for a specific battle stat.
	/// </summary>
	/// <param name="stat"></param>
	/// <param name="change"></param>
	public void ChangeStatModifier(StatEnum stat, int change)
	{
		_statModifiers.ChangeModifier(stat, change);
	}
}
