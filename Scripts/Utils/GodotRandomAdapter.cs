using System;

namespace Game.Utils;

public sealed class GodotRandomAdapter : IRandom
{
	private readonly Godot.RandomNumberGenerator _rng = new();

	public GodotRandomAdapter()
	{
		_rng.Randomize();
	}

	public int NextInt(int minInclusive, int maxInclusive)
	{
		return _rng.RandiRange(minInclusive, maxInclusive);
	}

	public float NextFloat(float minInclusive, float maxInclusive)
	{
		return _rng.RandfRange(minInclusive, maxInclusive);
	}
}
