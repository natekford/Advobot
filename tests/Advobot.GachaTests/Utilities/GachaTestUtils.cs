using System;

namespace Advobot.GachaTests.Utilities
{
	public static class GachaTestUtils
	{
		public static ulong NextUlong(this Random rng)
		{
			var buffer = new byte[sizeof(ulong)];
			rng.NextBytes(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}
		public static bool NextBool(this Random rng)
			=> rng.NextDouble() >= 0.5;
	}
}
