using System;
using System.Collections.Generic;

namespace Game.Creatures.Domain;

/// <summary>
/// Represents the current modifiers for a creature's battle stats (like Attack, Defense, etc.) during a battle.
/// Each stat can have a modifier level between -6 and +6, which corresponds to a specific multiplier that affects the creature's performance in battle. 
/// The modifiers can be changed by certain abilities or effects, allowing for dynamic changes in the creature's stats and strategy during battle. 
/// </summary>
public class CreatureStatModifiers
{
	private const int _minLevel = -6;
	private const int _maxLevel = 6;

	private int[] _modifiers = new int[Enum.GetValues<StatEnum>().Length];
	private static readonly IReadOnlyList<double> Multipliers = new double[]
	{
		0.25, 0.2857, 0.3333, 0.4, 0.5, 0.6667,
		1,
		1.5, 2, 2.5, 3, 3.5, 4
	};

	/// <summary>
	/// Gets the multiplier for a specific battle stat based on its current modifier level.
	/// </summary>
	/// <param name="stat"></param>
	/// <returns></returns>
	public double GetMultiplier(StatEnum stat)
	{
		return Multipliers[_modifiers[(int)stat] - _minLevel];
	}

	/// <summary>
	/// Changes the modifier level for a specific battle stat.
	/// </summary>
	/// <param name="stat"></param>
	/// <param name="change"></param>
	public void ChangeModifier(StatEnum stat, int change)
	{
		int i = (int)stat;
		_modifiers[i] = Math.Clamp(_modifiers[i] + change, _minLevel, _maxLevel);
	}
}
