namespace Game.Utils.Random;

/// <summary>
/// A deterministic random number generator that produces the same sequence of numbers for a given seed.
/// This is useful for testing, debugging, or any scenario where reproducibility is desired.
/// </summary>
public sealed class DeterministicRandom : IRandom
{
	private readonly System.Random _random;

	public DeterministicRandom(int seed)
	{
		_random = new System.Random(seed);
	}

	/// <summary>
	/// Generates a random integer within the specified range.
	/// </summary>
	/// <param name="minInclusive"></param>
	/// <param name="maxInclusive"></param>
	/// <returns></returns>
	public int NextInt(int minInclusive, int maxInclusive)
	{
		return _random.Next(minInclusive, maxInclusive + 1);
	}

	/// <summary>
	/// Generates a random float within the specified range.
	/// </summary>
	/// <param name="minInclusive"></param>
	/// <param name="maxInclusive"></param>
	/// <returns></returns>
	public float NextFloat(float minInclusive, float maxInclusive)
	{
		return (float)_random.NextDouble() * (maxInclusive - minInclusive) + minInclusive;
	}
}
