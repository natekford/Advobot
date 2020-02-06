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
		private static readonly RequestOptions _Options
			= DiscordUtils.GenerateRequestOptions("Raid prevention.");

		[JsonIgnore]
		private readonly ConcurrentDictionary<ulong, byte> _Instances = new ConcurrentDictionary<ulong, byte>();

		/// <summary>
		/// The amount of users to count for a raid.
		/// </summary>
		[JsonProperty("RaidCount")]
		public int RaidCount { get; set; }

		/// <inheritdoc />
		public override Task DisableAsync(IGuild guild)
		{
			Enabled = false;
			return Task.CompletedTask;
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

			var joined = SnowflakeUtils.ToSnowflake(user.JoinedAt.GetValueOrDefault().UtcDateTime);
			_Instances.GetOrAdd(joined, 0);
			if (GetInstanceCount() >= RaidCount)
			{
				var punisher = new PunishmentManager(user.Guild, null);
				var args = new PunishmentArgs
				{
					Options = _Options,
					Role = punisher.Guild.GetRole(RoleId ?? 0),
				};
				return punisher.GiveAsync(Punishment, user.AsAmbiguous(), args);
			}
			return Task.CompletedTask;
		}

		private int GetInstanceCount() => Type switch
		{
			RaidType.Regular => int.MaxValue,
			RaidType.RapidJoins => CountItemsInTimeFrame(_Instances.Keys, TimeInterval),
			_ => throw new ArgumentOutOfRangeException(nameof(Type)),
		};
	}
}