using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;

using System.Collections.Concurrent;

namespace Advobot.Tests.Commands.AutoMod;

public sealed class FakeTimedPunishmentDatabase : ITimedPunishmentDatabase
{
	public ConcurrentDictionary<Key, TimedPunishment> Punishments { get; } = new();

	public event Action<bool, IEnumerable<TimedPunishment>> PunishmentsModified;

	public Task<int> AddTimedPunishmentAsync(TimedPunishment punishment)
	{
		Punishments.AddOrUpdate(new(punishment), punishment, (_, _) => punishment);
		PunishmentsModified?.Invoke(true, [punishment]);
		return Task.FromResult(1);
	}

	public Task<int> DeleteTimedPunishmentAsync(TimedPunishment punishment)
	{
		var existed = Punishments.TryRemove(new(punishment), out _);
		PunishmentsModified?.Invoke(false, [punishment]);
		return Task.FromResult(existed ? 1 : 0);
	}

	public Task<int> DeleteTimedPunishmentsAsync(IEnumerable<TimedPunishment> punishments)
	{
		var count = 0;
		foreach (var punishment in punishments)
		{
			if (Punishments.TryRemove(new(punishment), out _))
			{
				++count;
			}
		}
		PunishmentsModified?.Invoke(false, punishments);
		return Task.FromResult(count);
	}

	public Task<IReadOnlyList<TimedPunishment>> GetExpiredPunishmentsAsync(long ticks)
	{
		var list = Punishments.Values
			.Where(x => x.EndTime.Ticks < ticks)
			.ToList();
		return Task.FromResult<IReadOnlyList<TimedPunishment>>(list);
	}

	public readonly struct Key(TimedPunishment punishment)
	{
		public ulong GuildId { get; } = punishment.GuildId;
		public PunishmentType PunishmentType { get; } = punishment.PunishmentType;
		public ulong UserId { get; } = punishment.UserId;
	}
}