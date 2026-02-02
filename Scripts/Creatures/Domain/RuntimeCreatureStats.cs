using Godot;
using System;

using BaseCreatureStats = Game.Creatures.Data.BaseCreatureStats;

namespace Game.Creatures.Domain;

public class RuntimeCreatureStats
{
	public int MaxHP { get; }
	public int Attack { get; }
	public int Defense { get; }
	public int SpecialAttack { get; }
	public int SpecialDefense { get; }
	public int Speed { get; }

	public RuntimeCreatureStats(BaseCreatureStats baseStats, int level)
	{
		MaxHP = (baseStats.MaxHP * 2 * level) / 100 + level + 10;
		Attack = (baseStats.Attack * 2 * level) / 100 + 5;
		Defense = (baseStats.Defense * 2 * level) / 100 + 5;
		Speed = (baseStats.Speed * 2 * level) / 100 + 5;
		SpecialAttack = (baseStats.SpecialAttack * 2 * level) / 100 + 5;
		SpecialDefense = (baseStats.SpecialDefense * 2 * level) / 100 + 5;
	}
}
