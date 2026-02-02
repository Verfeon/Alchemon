using System;
using System.Collections.Generic;

using CreatureData = Game.Creatures.Data.CreatureData;
using Ability = Game.Creatures.Data.Ability;
using IRandom = Game.Utils.IRandom;

namespace Game.Creatures.Domain;

public partial class Creature
{
	public event Action<int> HpChanged;
	public event Action<Creature> Fainted;
	
	private const int MaxAbilities = 6;
	
	private int _currentHP;
	
	public CreatureData Data { get; }
	public RuntimeCreatureStats RealStats { get; }   
	
	public int CurrentHP
	{
		get => _currentHP;
		set
		{
			int clamped = Math.Clamp(value, 0, RealStats.MaxHP);
			if (_currentHP == clamped) return;

			_currentHP = clamped;
			HpChanged?.Invoke(_currentHP);

			if (_currentHP == 0 && !IsFainted)
				IsFainted = true;
				Fainted?.Invoke(this);
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
		RealStats = new RuntimeCreatureStats(Data.BaseStats, level);
		IsFainted = false;
		_currentHP = RealStats.MaxHP;

		_abilities = new List<Ability>(MaxAbilities);
		InitializeAbilities();
	}
	
	private void InitializeAbilities()
	{
		if (Data.Abilities == null || Data.Abilities.Count == 0)
			return;

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
}
