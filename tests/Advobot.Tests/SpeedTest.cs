using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Advobot.Tests
{
	[DebuggerDisplay("Average = {Average}")]
	internal sealed class SpeedTest
	{
		public Action Function;
		public int Iterations = 10;
		public int Times = 100000;
		private readonly List<Stopwatch> _Watches = new List<Stopwatch>();

		public double Average => _Watches.Average(s => s.ElapsedMilliseconds);

		public long Max => _Watches.Max(s => s.ElapsedMilliseconds);

		public long Min => _Watches.Min(s => s.ElapsedMilliseconds);

		public SpeedTest(Action func)
		{
			Function = func;
		}

		public void Test()
		{
			_Watches.Clear();
			for (var i = 0; i < Times; i++)
			{
				var sw = Stopwatch.StartNew();
				for (int o = 0; o < Iterations; o++)
				{
					Function();
				}
				sw.Stop();
				_Watches.Add(sw);
			}
		}
	}
}