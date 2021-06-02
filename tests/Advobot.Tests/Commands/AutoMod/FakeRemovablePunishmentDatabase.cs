using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;

namespace Advobot.Tests.Commands.AutoMod
{
	public sealed class FakeRemovablePunishmentDatabase : IRemovablePunishmentDatabase
	{
		private readonly ConcurrentDictionary<Key, RemovablePunishment> _Punishments = new();

		public event Action<bool, IEnumerable<RemovablePunishment>> PunishmentsModified;

		public Task<int> AddRemovablePunishmentAsync(RemovablePunishment punishment)
		{
			_Punishments.AddOrUpdate(new Key(punishment), punishment, (_, _) => punishment);
			PunishmentsModified?.Invoke(true, new[] { punishment });
			return Task.FromResult(1);
		}

		public Task<int> DeleteRemovablePunishmentAsync(RemovablePunishment punishment)
		{
			var existed = _Punishments.TryRemove(new Key(punishment), out _);
			PunishmentsModified?.Invoke(false, new[] { punishment });
			return Task.FromResult(existed ? 1 : 0);
		}

		public Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<RemovablePunishment> punishments)
		{
			var count = 0;
			foreach (var punishment in punishments)
			{
				if (_Punishments.TryRemove(new Key(punishment), out _))
				{
					++count;
				}
			}
			PunishmentsModified?.Invoke(false, punishments);
			return Task.FromResult(count);
		}

		public Task<IReadOnlyList<RemovablePunishment>> GetOldPunishmentsAsync(long ticks)
		{
			var list = _Punishments.Values
				.Where(x => x.EndTime.Ticks < ticks)
				.ToList();
			return Task.FromResult<IReadOnlyList<RemovablePunishment>>(list);
		}

		private readonly struct Key
		{
			public ulong GuildId { get; }
			public PunishmentType PunishmentType { get; }
			public ulong UserId { get; }

			public Key(RemovablePunishment punishment)
			{
				GuildId = punishment.GuildId;
				UserId = punishment.UserId;
				PunishmentType = punishment.PunishmentType;
			}
		}
	}
}