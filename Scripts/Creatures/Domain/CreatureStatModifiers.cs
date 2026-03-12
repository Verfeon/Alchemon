using Godot;
using System;
using System.Collections.Generic;

namespace Game.Creatures.Domain;

public class CreatureStatModifiers
{
	private const int _minLevel = -6;
	private const int _maxLevel = 6;

	private int[] _modifiers = new int[StatEnum.GetValues<StatEnum>().Length];
	private static readonly IReadOnlyList<double> Multipliers = new double[]
	{
		0.25, 0.2857, 0.3333, 0.4, 0.5, 0.6667,
		1,
		1.5, 2, 2.5, 3, 3.5, 4
	};

	public double GetMultiplier(StatEnum stat)
	{
		return Multipliers[_modifiers[(int)stat] - _minLevel];
	}

	public void ChangeModifier(StatEnum stat, int change)
	{
		int i = (int)stat;
		_modifiers[i] = Math.Clamp(_modifiers[i] + change, _minLevel, _maxLevel);
	}
}
