using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds information about raid prevention such as how long the interval is, and how many users to target.
	/// </summary>
	[NamedArgumentType]
	public sealed class RaidPrev : TimedPrev<RaidType>
	{
		private static readonly Punisher _Giver = new Punisher(TimeSpan.FromMinutes(0), default);
		private static readonly RequestOptions _Reason = DiscordUtils.GenerateRequestOptions("Raid prevention.");

		/// <summary>
		/// The amount of users to count for a raid.
		/// </summary>
		[JsonProperty]
		public int UserCount { get; set; }
		/// <summary>
		/// The times at which something that could be part of raiding happened.
		/// </summary>
		[JsonIgnore]
		public ImmutableArray<ulong> TimeList => _TimeList.ToImmutableArray();

		[JsonIgnore]
		private List<ulong> _TimeList = new List<ulong>();

		/// <summary>
		/// Counts how many instances have happened in the supplied interval inside the time list.
		/// </summary>
		/// <returns></returns>
		public int GetSpamCount()
			=> DiscordUtils.CountItemsInTimeFrame(_TimeList, TimeInterval);
		/// <summary>
		/// Adds the time to the list.
		/// </summary>
		/// <param name="time"></param>
		public void Add(DateTime time)
		{
			lock (_TimeList)
			{
				_TimeList.Add(SnowflakeUtils.ToSnowflake(time));
			}
		}
		/// <summary>
		/// Removes every value from the time list.
		/// </summary>
		public void Reset()
			=> Interlocked.Exchange(ref _TimeList, new List<ulong>());
		/// <summary>
		/// Punishes a user.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task PunishAsync(IGuildSettings settings, SocketGuildUser user)
			=> await _Giver.GiveAsync(Punishment, user.Guild, user.Id, settings.MuteRoleId, _Reason).CAF();
		/// <inheritdoc />
		public override string Format(SocketGuild? guild = null)
		{
			return $"**Enabled:** `{Enabled}`\n" +
				$"**Users:** `{UserCount}`\n" +
				$"**Time Interval:** `{TimeInterval}`\n" +
				$"**Punishment:** `{Punishment.ToString()}`";
		}
	}
}