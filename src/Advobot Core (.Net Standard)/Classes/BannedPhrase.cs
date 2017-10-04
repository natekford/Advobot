using Advobot.Actions;
using Advobot.Classes.Punishments;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.BannedPhrases
{
	/// <summary>
	/// Holds a phrase and punishment.
	/// </summary>
	public class BannedPhrase : ISetting
	{
		[JsonProperty]
		public string Phrase { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			ChangePunishment(punishment);
		}

		/// <summary>
		/// Sets <see cref="Punishment"/> to <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		public void ChangePunishment(PunishmentType punishment)
		{
			Punishment = punishment;
		}

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public async Task HandleBannedPhrasePunishment(IGuildSettings guildSettings, IMessage message, ITimersService timers = null)
		{
			await MessageActions.DeleteMessage(message);

			var users = guildSettings.BannedPhraseUsers;
			var user = users.SingleOrDefault(x => x.User.Id == message.Author.Id);
			if (user == null)
			{
				guildSettings.BannedPhraseUsers.Add(user = new BannedPhraseUserInformation(message.Author as IGuildUser));
			}

			//Update the count
			var count = user.IncrementValue(Punishment);

			var punishments = guildSettings.BannedPhrasePunishments;
			var punishment = punishments.SingleOrDefault(x => x.Punishment == Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			var giver = new AutomaticPunishmentGiver(punishment.PunishmentTime, timers);
			await giver.AutomaticallyPunishAsync(Punishment, user.User, punishment.GetRole(guildSettings.Guild));

			//Reset the user's number of removes for that given type.
			user.ResetValue(Punishment);
		}

		public override string ToString()
		{
			var punishmentChar = Punishment == default ? "N" : Punishment.EnumName().Substring(0, 1);
			return $"`{punishmentChar}` `{Phrase}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}