using Godot;
using System;
using System.Collections.Generic;

using CreatureData = Game.Resources.CreatureData;
using Ability = Game.Resources.Ability;

namespace Game.Creatures;

public partial class Creature
{
	private int _currentHP;
	
	public CreatureData Data { get; set; }
	public CreatureStats RealStats { get; set; }   
	public int CurrentHP
	{
		get => _currentHP;
		set => _currentHP = Mathf.Clamp(value, 0, RealStats.MaxHP);
	}        // computed on merge
	public List<Ability> Abilities { get; set; }
	
	public Creature(CreatureData data, int level = 1)
	{
		Data = data;
		//CalculateRealStats(level);
	}
	
	public void CalculateRealStats(int level)
	{
		CreatureStats baseStats = Data.BaseStats;
		
		RealStats = new CreatureStats
		{
			MaxHP = (int)((baseStats.MaxHP * 2 * level) / 100) + level + 10,
			Attack = (int)((baseStats.Attack * 2 * level) / 100) + 5,
			Defense = (int)((baseStats.Defense * 2 * level) / 100) + 5,
			Speed = (int)((baseStats.Speed * 2 * level) / 100) + 5,
			SpecialAttack = (int)((baseStats.SpecialAttack * 2 * level) / 100) + 5,
			SpecialDefense = (int)((baseStats.SpecialDefense * 2 * level) / 100) + 5
		};
		_currentHP = RealStats.MaxHP;
	}
}
