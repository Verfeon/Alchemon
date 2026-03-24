namespace Game.Utils.Random;

/// <summary>
/// Adapts the Godot RandomNumberGenerator to the IRandom interface.
/// </summary>
public sealed class GodotRandomAdapter : IRandom
{
	private readonly Godot.RandomNumberGenerator _rng = new();

	public GodotRandomAdapter()
	{
		_rng.Randomize();
	}

	/// <summary>
	/// Generates a random integer within the specified range.
	/// </summary>
	/// <param name="minInclusive"></param>
	/// <param name="maxInclusive"></param>
	/// <returns></returns>
	public int NextInt(int minInclusive, int maxInclusive)
	{
		return _rng.RandiRange(minInclusive, maxInclusive);
	}

	/// <summary>
	/// Generates a random float within the specified range.
	/// </summary>
	/// <param name="minInclusive"></param>
	/// <param name="maxInclusive"></param>
	/// <returns></returns>
	public float NextFloat(float minInclusive, float maxInclusive)
	{
		return _rng.RandfRange(minInclusive, maxInclusive);
	}
}
