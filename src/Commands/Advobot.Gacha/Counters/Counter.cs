﻿using System.Collections.Concurrent;

namespace Advobot.Gacha.Counters;

public sealed class Counter(int defaultAmount) : ICounter<ulong>
{
	private readonly ConcurrentDictionary<ulong, int> _AmountLeft = new();
	private readonly int _DefaultAmount = defaultAmount;

	public bool CanDo(ulong id)
		=> _AmountLeft.GetOrAdd(id, _DefaultAmount) > 0;

	public void HasBeenDone(ulong id)
		=> _AmountLeft.AddOrUpdate(id, _DefaultAmount, (_, value) => --value);
}