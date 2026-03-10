using System;

namespace Game.Utils.Random;

public sealed class DeterministicRandom : IRandom
{
	private readonly System.Random _random;

	public DeterministicRandom(int seed)
	{
		_random = new System.Random(seed);
	}

	public int NextInt(int minInclusive, int maxInclusive)
	{
		return _random.Next(minInclusive, maxInclusive + 1);
	}

	public float NextFloat(float minInclusive, float maxInclusive)
	{
		return (float)_random.NextDouble() * (maxInclusive - minInclusive) + minInclusive;
	}
}
