using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds a phrase and punishment.
	/// </summary>
	public class BannedPhrase : IGuildSetting
	{
		private static readonly RequestOptions _Options = ClientUtils.CreateRequestOptions("banned phrase");

		/// <summary>
		/// The phrase which is banned. Can be string or regex pattern.
		/// </summary>
		[JsonProperty]
		public string Phrase { get; }
		/// <summary>
		/// The type of punishment associated with this phrase.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment;

		/// <summary>
		/// Creates an instance of banned phrase.
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
		public async Task PunishAsync(IGuildSettings settings, SocketGuild guild, BannedPhraseUserInfo info, ITimerService timers)
		{
			var count = info.Increment(Punishment);
			var punishment = settings.BannedPhrasePunishments.SingleOrDefault(x => x.Punishment == Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			var giver = new Punisher(TimeSpan.FromMinutes(punishment.Time), timers);
			await giver.GiveAsync(Punishment, guild, info.UserId, punishment.RoleId, _Options).CAF();
			info.Reset(Punishment);
		}
		/// <inheritdoc />
		public override string ToString()
			=> $"`{(Punishment == default ? 'N' : Punishment.ToString()[0])}` `{Phrase}`";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> ToString();
	}
}