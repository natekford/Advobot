using Advobot.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds information about spam prevention, such as how much is considered spam, required spam instances, and votes to kick.
	/// </summary>
	[NamedArgumentType]
	public sealed class SpamPrev : TimedPrev<SpamType>
	{
		/// <summary>
		/// The required amount of times a user must spam before they can be voted to be kicked.
		/// </summary>
		[JsonProperty]
		public int SpamInstances { get; set; }
		/// <summary>
		/// The amount of votes needed to kick a user.
		/// </summary>
		[JsonProperty]
		public int VotesForKick { get; set; }
		/// <summary>
		/// The required amount of content before a message is considered spam.
		/// </summary>
		[JsonProperty]
		public int SpamPerMessage { get; set; }

		/// <summary>
		/// Wehther the message should count as spam.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool IsSpam(IUserMessage message)
		{
			var spamAmount = Type switch
			{
				SpamType.Message => int.MaxValue,
				SpamType.LongMessage => message.Content?.Length,
				SpamType.Link => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)),
				SpamType.Image => message.Attachments.Count(x => x.Height != null || x.Width != null) + message.Embeds.Count(x => x.Image != null || x.Video != null),
				SpamType.Mention => message.MentionedUserIds.Distinct().Count(),
				_ => throw new ArgumentException(nameof(Type)),
			};

			return spamAmount >= SpamPerMessage;
		}
		/// <inheritdoc />
		public override string Format(SocketGuild? guild = null)
		{
			return $"**Punishment:** `{Punishment.ToString()}`\n" +
				$"**Spam Instances:** `{SpamInstances}`\n" +
				$"**Votes For Punishment:** `{VotesForKick}`\n" +
				$"**Spam Amount:** `{SpamPerMessage}`\n" +
				$"**Time Interval:** `{TimeInterval}`";
		}
	}
}
