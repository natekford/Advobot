using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds information about spam prevention, such as how much is considered spam, required spam instances, and votes to kick.
	/// </summary>
	public class SpamPreventionInfo : IGuildSetting
	{
		[JsonProperty]
		public PunishmentType Punishment { get; }
		/// <summary>
		/// The required amount of times a user must spam before they can be voted to be kicked.
		/// </summary>
		[JsonProperty]
		public int SpamInstances { get; }
		/// <summary>
		/// The amount of votes needed to kick a user.
		/// </summary>
		[JsonProperty]
		public int VotesForKick { get; }
		/// <summary>
		/// The required amount of content before a message is considered spam.
		/// </summary>
		[JsonProperty]
		public int SpamPerMessage { get; }
		/// <summary>
		/// The time limit that all messages need to be sent in for them to count.
		/// </summary>
		[JsonProperty]
		public int TimeInterval { get; }
		[JsonIgnore]
		public bool Enabled;

		private SpamPreventionInfo(PunishmentType punishment, int instances, int votes, int timeInterval, int spamAmount)
		{
			Punishment = punishment;
			SpamInstances = instances;
			VotesForKick = votes;
			TimeInterval = timeInterval;
			SpamPerMessage = spamAmount;
		}

		/// <summary>
		/// Attempts to create spam prevention.
		/// </summary>
		/// <param name="spam"></param>
		/// <param name="punishment"></param>
		/// <param name="instances"></param>
		/// <param name="votes"></param>
		/// <param name="timeInterval"></param>
		/// <param name="spamAmount"></param>
		/// <param name="info"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static bool TryCreate(SpamType spam, PunishmentType punishment, int instances, int votes, int timeInterval, int spamAmount,
			out SpamPreventionInfo info, out Error error)
		{
			info = default;
			error = default;

			if (IsError("spam instances count", instances, 1, 25, out error)
				|| IsError("vote count", votes, 1, 50, out error)
				|| IsError("time interval", timeInterval, 1, 180, out error))
			{
				return false;
			}
			switch (spam)
			{
				case SpamType.LongMessage:
					if (IsError("message length", spamAmount, 1, 2000, out error)) { return false; }
					break;
				case SpamType.Link:
					if (IsError("link count", spamAmount, 1, 50, out error)) { return false; }
					break;
				case SpamType.Image:
					if (IsError("image count", spamAmount, 1, 50, out error)) { return false; }
					break;
				case SpamType.Mention:
					if (IsError("mention count", spamAmount, 1, 200, out error)) { return false; }
					break;
			}

			info = new SpamPreventionInfo(punishment, instances, votes, timeInterval, spamAmount);
			return true;
		}
		private static bool IsError(string name, int inputValue, int minValue, int maxValue, out Error error)
		{
			error = default;
			if (inputValue > maxValue)
			{
				error = new Error($"The {name} must be less than or equal to `{maxValue}`.");
			}
			else if (inputValue < minValue)
			{
				error = new Error($"The {name} must be greater than or equal to `{minValue}`.");
			}
			return !String.IsNullOrWhiteSpace(error.Reason);
		}

		public override string ToString()
		{
			return $"**Punishment:** `{Punishment.ToString()}`\n" +
				$"**Spam Instances:** `{SpamInstances}`\n" +
				$"**Votes For Punishment:** `{VotesForKick}`\n" +
				(SpamPerMessage != 0 ? $"**Spam Amount:** `{SpamPerMessage}`" : $"**Time Interval:** `{TimeInterval}`");
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
