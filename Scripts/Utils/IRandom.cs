using System;

namespace Game.Utils;

public interface IRandom
{
	int NextInt(int minInclusive, int maxInclusive);
}
