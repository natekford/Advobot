﻿using Advobot.Enums;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds information about spam prevention, such as how much is considered spam, required spam instances, and votes to kick.
	/// </summary>
	[NamedArgumentType]
	public sealed class SpamPrev : IGuildFormattable
	{
		/// <summary>
		/// The spam this is preventing.
		/// </summary>
		[JsonProperty]
		public SpamType Type { get; set; }
		/// <summary>
		/// The punishment for spamming.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment { get; set; }
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
		/// The time limit that all messages need to be sent in for them to count.
		/// </summary>
		[JsonProperty]
		public int TimeInterval { get; set; }
		/// <summary>
		/// Whether or not this spam prevention is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; set; }

		/// <inheritdoc />
		public string Format(SocketGuild? guild = null)
		{
			return $"**Punishment:** `{Punishment.ToString()}`\n" +
				$"**Spam Instances:** `{SpamInstances}`\n" +
				$"**Votes For Punishment:** `{VotesForKick}`\n" +
				$"**Spam Amount:** `{SpamPerMessage}`\n" +
				$"**Time Interval:** `{TimeInterval}`";
		}
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}
