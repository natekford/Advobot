﻿using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Services.Timers;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Holds a phrase and punishment.
	/// </summary>
	public sealed class BannedPhrase : IGuildFormattable
	{
		/// <summary>
		/// The phrase which is banned. Can be string or regex pattern.
		/// </summary>
		[JsonProperty("Phrase")]
		public string? Phrase { get; set; }
		/// <summary>
		/// The type of punishment associated with this phrase.
		/// </summary>
		[JsonProperty("Punishment")]
		public Punishment Punishment { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="BannedPhrase"/>.
		/// </summary>
		public BannedPhrase() { }
		/// <summary>
		/// Creates an instance of <see cref="BannedPhrase"/>.
		/// </summary>
		/// <param name="phrase"></param>
		/// <param name="punishment"></param>
		public BannedPhrase(string phrase, Punishment punishment)
		{
			Phrase = phrase;
			Punishment = punishment;
		}

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="guild"></param>
		/// <param name="info"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public Task PunishAsync(IGuildSettings settings, IGuild guild, BannedPhraseUserInfo info, ITimerService timers)
		{
			var count = info.Increment(Punishment);
			if (!settings.BannedPhrasePunishments.TryGetSingle(x => x.Punishment == Punishment && x.NumberOfRemoves == count, out var punishment))
			{
				return Task.CompletedTask;
			}

			info.Reset(Punishment);
			var punishmentArgs = new PunishmentArgs(timers, TimeSpan.FromMinutes(punishment.Time))
			{
				Options = DiscordUtils.GenerateRequestOptions("Banned phrase."),
			};
			return PunishmentUtils.GiveAsync(Punishment, guild, info.UserId, punishment.RoleId, punishmentArgs);
		}

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
			=> new DiscordFormattableString($"{Punishment.ToString()[0]} {Phrase}");
	}
}