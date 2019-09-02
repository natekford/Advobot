using System;

namespace Advobot.Levels.Service
{
	public sealed class LevelServiceConfig
	{
		public int BaseXp { get; set; } = 25;
		public int CacheSize { get; set; } = 10;
		public double Log { get; set; } = 9;
		public double Pow { get; set; } = 2.3;
		public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(30);
	}
}