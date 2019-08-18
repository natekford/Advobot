using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings.Settings
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
		[JsonProperty("RaidCount")]
		public int RaidCount { get; set; }

		private readonly ConcurrentDictionary<ulong, byte> _Instances = new ConcurrentDictionary<ulong, byte>();

		/// <summary>
		/// Punishes a user.
		/// </summary>
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
			_ => throw new ArgumentOutOfRangeException(nameof(Type)),
		};
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

		/// <inheritdoc />
		public override IDiscordFormattableString GetFormattableString()
		{
			return new Dictionary<string, object>
			{
				{ "Enabled", Enabled },
				{ "Interval", TimeInterval },
				{ "Punishment", Punishment },
				{ "Users", RaidCount },
			}.ToDiscordFormattableStringCollection();
		}
	}
}