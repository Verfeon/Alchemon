using Godot;
using System;

using BaseCreatureStats = Game.Creatures.Data.BaseCreatureStats;

namespace Game.Creatures.Domain;

public class RuntimeCreatureStats
{
	private readonly int[] _stats = new int[6];

	public int Get(StatEnum stat) => _stats[(int)stat];

	public RuntimeCreatureStats(BaseCreatureStats baseStats, int level)
	{
		_stats[(int)StatEnum.MaxHP] =
			(baseStats.MaxHP * 2 * level) / 100 + level + 10;

		_stats[(int)StatEnum.Attack] =
			(baseStats.Attack * 2 * level) / 100 + 5;

		_stats[(int)StatEnum.Defense] =
			(baseStats.Defense * 2 * level) / 100 + 5;

		_stats[(int)StatEnum.SpecialAttack] =
			(baseStats.SpecialAttack * 2 * level) / 100 + 5;

		_stats[(int)StatEnum.SpecialDefense] =
			(baseStats.SpecialDefense * 2 * level) / 100 + 5;

		_stats[(int)StatEnum.Speed] =
			(baseStats.Speed * 2 * level) / 100 + 5;
	}
}
