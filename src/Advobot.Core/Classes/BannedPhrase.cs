using Advobot.Core.Actions;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.BannedPhrases
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

		public BannedPhrase(string phrase, PunishmentType punishment = default)
		{
			this.Phrase = phrase;
			ChangePunishment(punishment);
		}

		/// <summary>
		/// Sets <see cref="Punishment"/> to <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		public void ChangePunishment(PunishmentType punishment) => this.Punishment = punishment;

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public async Task PunishAsync(IGuildSettings guildSettings, IMessage message, ITimersService timers = null)
		{
			await MessageActions.DeleteMessageAsync(message, new AutomaticModerationReason("banned phrase")).CAF();

			var user = guildSettings.BannedPhraseUsers.SingleOrDefault(x => x.User.Id == message.Author.Id);
			if (user == null)
			{
				guildSettings.BannedPhraseUsers.Add(user = new BannedPhraseUserInformation(message.Author as IGuildUser));
			}

			var count = user.IncrementValue(this.Punishment);
			var punishment = guildSettings.BannedPhrasePunishments.SingleOrDefault(x => x.Punishment == this.Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			var giver = new AutomaticPunishmentGiver(punishment.PunishmentTime, timers);
			await giver.AutomaticallyPunishAsync(this.Punishment, user.User, punishment.GetRole(guildSettings.Guild)).CAF();
			user.ResetValue(this.Punishment);
		}

		public override string ToString()
		{
			var punishmentChar = this.Punishment == default ? "N" : this.Punishment.EnumName().Substring(0, 1);
			return $"`{punishmentChar}` `{this.Phrase}`";
		}
		public string ToString(SocketGuild guild) => ToString();
	}
}