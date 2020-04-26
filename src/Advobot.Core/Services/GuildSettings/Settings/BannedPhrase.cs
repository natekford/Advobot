using System;
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
		private static readonly RequestOptions _Options
			= DiscordUtils.GenerateRequestOptions("Banned phrase.");

		/// <summary>
		/// The phrase which is banned. Can be string or regex pattern.
		/// </summary>
		[JsonProperty("Phrase")]
		public string Phrase { get; set; }

		/// <summary>
		/// The type of punishment associated with this phrase.
		/// </summary>
		[JsonProperty("Punishment")]
		public PunishmentType Punishment { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="BannedPhrase"/>.
		/// </summary>
		public BannedPhrase()
		{
			Phrase = "";
		}

		/// <summary>
		/// Creates an instance of <see cref="BannedPhrase"/>.
		/// </summary>
		/// <param name="phrase"></param>
		/// <param name="punishment"></param>
		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			Punishment = punishment;
		}

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
			=> new DiscordFormattableString($"{Punishment.ToString()[0]} {Phrase}");

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="guild"></param>
		/// <param name="info"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public Task PunishAsync(
			IGuildSettings settings,
			IGuild guild,
			BannedPhraseUserInfo info,
			ITimerService timers)
		{
			var count = info.Increment(Punishment);
			bool Predicate(BannedPhrasePunishment x)
				=> x.Punishment == Punishment && x.NumberOfRemoves == count;
			if (!settings.BannedPhrasePunishments.TryGetSingle(Predicate, out var punishment))
			{
				return Task.CompletedTask;
			}

			info.Reset(Punishment);
			var punisher = new PunishmentManager(guild, timers);
			var args = new PunishmentArgs
			{
				Time = TimeSpan.FromMinutes(punishment.Time),
				Options = _Options,
				Role = punisher.Guild.GetRole(punishment.RoleId),
			};
			return punisher.GiveAsync(Punishment, info.UserId, args);
		}
	}
}