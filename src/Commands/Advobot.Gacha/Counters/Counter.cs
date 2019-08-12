using System.Collections.Concurrent;

namespace Advobot.Gacha.Counters
{
	public sealed class Counter : ICounter<ulong>
	{
		private readonly ConcurrentDictionary<ulong, int> _AmountLeft
			= new ConcurrentDictionary<ulong, int>();
		private readonly int _DefaultAmount;

		public Counter(int defaultAmount)
		{
			_DefaultAmount = defaultAmount;
		}

		public bool CanDo(ulong id)
			=> _AmountLeft.GetOrAdd(id, _DefaultAmount) > 0;
		public void HasBeenDone(ulong id)
			=> _AmountLeft.AddOrUpdate(id, _DefaultAmount, (key, value) => --value);
	}
}
