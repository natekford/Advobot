using System.Diagnostics;

namespace Advobot.Tests;

[DebuggerDisplay("Average = {Average}")]
internal sealed class SpeedTest(Action func)
{
	public Action Function = func;
	public int Iterations = 10;
	public int Times = 100000;
	private readonly List<Stopwatch> _Watches = new();

	public double Average => _Watches.Average(s => s.ElapsedMilliseconds);

	public long Max => _Watches.Max(s => s.ElapsedMilliseconds);

	public long Min => _Watches.Min(s => s.ElapsedMilliseconds);

	public void Test()
	{
		_Watches.Clear();
		for (var i = 0; i < Times; i++)
		{
			var sw = Stopwatch.StartNew();
			for (var o = 0; o < Iterations; o++)
			{
				Function();
			}
			sw.Stop();
			_Watches.Add(sw);
		}
	}
}