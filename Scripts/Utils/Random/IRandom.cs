using System;

namespace Game.Utils.Random;

public interface IRandom
{
	int NextInt(int minInclusive, int maxInclusive);
	float NextFloat(float minInclusive, float maxInclusive);
}
