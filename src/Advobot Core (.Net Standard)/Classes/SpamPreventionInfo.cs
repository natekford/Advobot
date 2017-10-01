using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.SpamPrevention
{
	public class SpamPreventionInfo : ISetting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int RequiredSpamInstances { get; }
		[JsonProperty]
		public int RequiredSpamPerMessageOrTimeInterval { get; }
		[JsonProperty]
		public int VotesForKick { get; }
		[JsonIgnore]
		public bool Enabled { get; private set; }

		public SpamPreventionInfo(PunishmentType punishmentType, int requiredSpamInstances, int requiredSpamPerMessageOrTimeInterval, int votesForKick)
		{
			PunishmentType = punishmentType;
			RequiredSpamInstances = requiredSpamInstances;
			RequiredSpamPerMessageOrTimeInterval = requiredSpamPerMessageOrTimeInterval;
			VotesForKick = votesForKick;
			Enabled = false;
		}

		public void Enable()
		{
			Enabled = true;
		}
		public void Disable()
		{
			Enabled = false;
		}

		public override string ToString()
		{
			return $"**Punishment:** `{PunishmentType.EnumName()}`\n" +
					$"**Spam Instances:** `{RequiredSpamInstances}`\n" +
					$"**Votes For Punishment:** `{VotesForKick}`\n" +
					$"**Spam Amt/Time Interval:** `{RequiredSpamPerMessageOrTimeInterval}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
