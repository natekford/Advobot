using System;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyTimedPrevention : IReadOnlyPunishment
	{
		bool Enabled { get; }
		TimeSpan Interval { get; }
		int Size { get; }
	}
}