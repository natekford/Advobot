using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds a phrase and punishment.
	/// </summary>
	public class BannedPhrase : IGuildSetting
	{
		[JsonProperty]
		public string Phrase { get; }
		[JsonProperty]
		public Punishment Punishment;

		public BannedPhrase(string phrase, Punishment punishment = default)
		{
			Phrase = phrase;
			Punishment = punishment;
		}

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public async Task PunishAsync(IGuildSettings settings, SocketUserMessage message, ITimersService timers)
		{
			if (!(message.Author is SocketGuildUser user))
			{
				return;
			}

			var bannedPhraseUser = timers.GetBannedPhraseUser(user);
			if (bannedPhraseUser == null)
			{
				await timers.AddAsync(bannedPhraseUser = new BannedPhraseUserInfo(user)).CAF();
			}

			bannedPhraseUser.IncrementValue(Punishment);
			var count = bannedPhraseUser[Punishment];
			var punishment = settings.BannedPhrasePunishments.SingleOrDefault(x => x.Punishment == Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			var giver = new PunishmentGiver(punishment.PunishmentTime, timers);
			var role = user.Guild.GetRole(punishment.RoleId);
			await giver.PunishAsync(Punishment, user, role, ClientUtils.CreateRequestOptions("banned phrase")).CAF();
			bannedPhraseUser.ResetValue(Punishment);
		}

		public override string ToString()
		{
			var punishmentChar = Punishment == default ? "N" : Punishment.ToString().Substring(0, 1);
			return $"`{punishmentChar}` `{Phrase}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}