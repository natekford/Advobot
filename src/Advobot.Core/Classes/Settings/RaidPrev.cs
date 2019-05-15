using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
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
		/// <summary>
		/// The amount of users to count for a raid.
		/// </summary>
		[JsonProperty]
		public int RaidCount { get; set; }

		[JsonIgnore]
		private ConcurrentDictionary<ulong, byte> _Instances = new ConcurrentDictionary<ulong, byte>();

		/// <summary>
		/// Punishes a user.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public Task PunishAsync(IGuildUser user)
		{
			if (!Enabled)
			{
				return Task.CompletedTask;
			}

			_Instances.GetOrAdd(SnowflakeUtils.ToSnowflake(user.JoinedAt?.UtcDateTime ?? default), 0);
			if (GetInstanceCount() >= RaidCount)
			{
				var punishmentArgs = new PunishmentArgs()
				{
					Options = DiscordUtils.GenerateRequestOptions("Raid prevention."),
				};
				return PunishmentUtils.GiveAsync(Punishment, user.Guild, user.Id, RoleId, punishmentArgs);
			}
			return Task.CompletedTask;
		}
		private int GetInstanceCount() => Type switch
		{
			RaidType.Regular => int.MaxValue,
			RaidType.RapidJoins => CountItemsInTimeFrame(_Instances.Keys, TimeInterval),
			_ => throw new ArgumentException(nameof(Type)),
		};
		/// <inheritdoc />
		public override string Format(SocketGuild? guild = null)
		{
			return $"**Enabled:** `{Enabled}`\n" +
				$"**Users:** `{RaidCount}`\n" +
				$"**Time Interval:** `{TimeInterval}`\n" +
				$"**Punishment:** `{Punishment.ToString()}`";
		}
		/// <inheritdoc />
		public override async Task EnableAsync(IGuild guild)
		{
			Enabled = true;
			if (Type == RaidType.Regular)
			{
				//Mute the newest joining users
				var users = (await guild.GetUsersAsync().CAF()).OrderByJoinDate().Reverse().ToArray();
				for (var i = 0; i < new[] { RaidCount, users.Length, 25 }.Min(); ++i)
				{
					await PunishAsync(users[i]).CAF();
				}
			}
		}
		/// <inheritdoc />
		public override Task DisableAsync(IGuild guild)
		{
			Enabled = false;
			return Task.CompletedTask;
		}
	}
}