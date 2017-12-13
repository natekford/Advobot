using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text;

namespace Advobot.Core.Classes.SpamPrevention
{
	/// <summary>
	/// Holds information about spam prevention, such as how much is considered spam, required spam instances, and votes to kick.
	/// </summary>
	public class SpamPreventionInfo : ISetting
	{
		private const int MSG_COUNT_MIN_LIM = 0;
		private const int MSG_COUNT_MAX_LIM = 25;
		private const int VOTE_COUNT_MIN_LIM = 0;
		private const int VOTE_COUNT_MAX_LIM = 50;
		private const int SPAM_TIME_AMT_MIN_LIM = 0;
		private const int TIME_INTERVAL_MAX_LIM = 180;
		private const int OTHERS_MAX_LIM = 100;
		private const int LONG_MESSAGE_MAX_LIM = 2000;

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

		private SpamPreventionInfo(PunishmentType punishmentType, int requiredSpamInstances, int requiredSpamPerMessageOrTimeInterval, int votesForKick)
		{
			PunishmentType = punishmentType;
			RequiredSpamInstances = requiredSpamInstances;
			RequiredSpamPerMessageOrTimeInterval = requiredSpamPerMessageOrTimeInterval;
			VotesForKick = votesForKick;
			Enabled = false;
		}

		public static bool TryCreateSpamPreventionInfo(SpamType spamType,
			PunishmentType punishmentType,
			int requiredSpamInstances,
			int requiredSpamPerMessageOrTimeInterval,
			int votesForKick,
			out SpamPreventionInfo spamPreventionInfo,
			out ErrorReason errorReason)
		{
			spamPreventionInfo = default;
			errorReason = default;

			if (requiredSpamInstances <= MSG_COUNT_MIN_LIM)
			{
				errorReason = new ErrorReason($"The message count must be greater than `{MSG_COUNT_MIN_LIM}`.");
				return false;
			}
			else if (requiredSpamInstances > MSG_COUNT_MAX_LIM)
			{
				errorReason = new ErrorReason($"The message count must be less than `{MSG_COUNT_MAX_LIM}`.");
				return false;
			}
			else if (votesForKick <= VOTE_COUNT_MIN_LIM)
			{
				errorReason = new ErrorReason($"The vote count must be greater than `{VOTE_COUNT_MIN_LIM}`.");
				return false;
			}
			else if (votesForKick > VOTE_COUNT_MAX_LIM)
			{
				errorReason = new ErrorReason($"The vote count must be less than `{VOTE_COUNT_MAX_LIM}`.");
				return false;
			}
			else if (requiredSpamPerMessageOrTimeInterval <= SPAM_TIME_AMT_MIN_LIM)
			{
				errorReason = new ErrorReason($"The spam amount or time interval must be greater than `{VOTE_COUNT_MIN_LIM}`.");
				return false;
			}

			switch (spamType)
			{
				case SpamType.Message:
				{
					if (requiredSpamPerMessageOrTimeInterval > TIME_INTERVAL_MAX_LIM)
					{
						errorReason = new ErrorReason($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`.");
						return false;
					}
					break;
				}
				case SpamType.LongMessage:
				{
					if (requiredSpamPerMessageOrTimeInterval > LONG_MESSAGE_MAX_LIM)
					{
						errorReason = new ErrorReason($"The message length must be less than `{LONG_MESSAGE_MAX_LIM}`.");
						return false;
					}
					break;
				}
				case SpamType.Link:
				{
					if (requiredSpamPerMessageOrTimeInterval > OTHERS_MAX_LIM)
					{
						errorReason = new ErrorReason($"The link count must be less than `{OTHERS_MAX_LIM}`.");
						return false;
					}
					break;
				}
				case SpamType.Image:
				{
					if (requiredSpamPerMessageOrTimeInterval > TIME_INTERVAL_MAX_LIM)
					{
						errorReason = new ErrorReason($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`.");
						return false;
					}
					break;
				}
				case SpamType.Mention:
				{
					if (requiredSpamPerMessageOrTimeInterval > OTHERS_MAX_LIM)
					{
						errorReason = new ErrorReason($"The mention count must be less than `{OTHERS_MAX_LIM}`.");
						return false;
					}
					break;
				}
			}

			spamPreventionInfo = new SpamPreventionInfo(punishmentType, requiredSpamInstances, requiredSpamPerMessageOrTimeInterval, votesForKick);
			return true;
		}

		public void Enable() => Enabled = true;
		public void Disable() => Enabled = false;

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"**Punishment:** `{PunishmentType.EnumName()}`")
			.AppendLineFeed($"**Spam Instances:** `{RequiredSpamInstances}`")
			.AppendLineFeed($"**Votes For Punishment:** `{VotesForKick}`")
			.Append($"**Spam Amt/Time Interval:** `{RequiredSpamPerMessageOrTimeInterval}`").ToString();
		public string ToString(SocketGuild guild) => ToString();
	}
}
