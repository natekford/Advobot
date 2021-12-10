﻿using Discord;

using System.Collections.Concurrent;

namespace Advobot.AutoMod;

public sealed class GuildSpecific<TKey, TValue>
	where TKey : notnull
	where TValue : new()
{
	private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<TKey, TValue>> _Dict = new();

	public TValue Get(IGuild guild, TKey key)
	{
		return _Dict
			.GetOrAdd(guild.Id, _ => new ConcurrentDictionary<TKey, TValue>())
			.GetOrAdd(key, _ => new TValue());
	}

	public void Reset(IGuild guild)
		=> _Dict.TryRemove(guild.Id, out _);
}