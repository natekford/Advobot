using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Keeps track how much spam this user has said, how many people need to vote, who has voted, and what punishment to give.
	/// </summary>
	public sealed class SpamPreventionUserInfo : UserDatabaseEntry
	{
		private static Dictionary<Punishment, int> _PunishmentSeverity = new Dictionary<Punishment, int>
		{
			{ default, -1 },
			{ Punishment.Deafen, 0 },
			{ Punishment.VoiceMute, 1 },
			{ Punishment.RoleMute, 2 },
			{ Punishment.Kick, 3 },
			{ Punishment.Softban, 4 },
			{ Punishment.Ban, 5 },
		};
		[BsonIgnore]
		private int _VotesRequired = int.MaxValue;
		[BsonIgnore]
		private Punishment _Punishment;

		/// <summary>
		/// The votes required to punish a user.
		/// Setting sets to the lowest of the new value or old value.
		/// </summary>
		public int VotesRequired
		{
			get => _VotesRequired;
			set => _VotesRequired = Math.Max(1, Math.Min(_VotesRequired, value));
		}
		/// <summary>
		/// The punishment to do on a user.
		/// Setting sets to whatever is the most severe punishment.
		/// </summary>
		public Punishment Punishment
		{
			get => _Punishment;
			set => _Punishment = _PunishmentSeverity[value] > _PunishmentSeverity[_Punishment] ? value : _Punishment;
		}
		/// <summary>
		/// Who has voted to punish the user.
		/// </summary>
		public List<ulong> UsersWhoHaveAlreadyVoted { get; set; } = new List<ulong>();
		/// <summary>
		/// The amount of spam instances associated with the amount of messages sent.
		/// </summary>
		public List<ulong> Message { get; set; } = new List<ulong>();
		/// <summary>
		/// The amount of spam instances associated with the length of messages sent.
		/// </summary>
		public List<ulong> LongMessage { get; set; } = new List<ulong>();
		/// <summary>
		/// The amount of spam instances associated with how many links are in messages sent.
		/// </summary>
		public List<ulong> Link { get; set; } = new List<ulong>();
		/// <summary>
		/// The amount of spam instances associated with how many images are in messages sent.
		/// </summary>
		public List<ulong> Image { get; set; } = new List<ulong>();
		/// <summary>
		/// The amount of spam instances associated with how many mentions are in messages sent.
		/// </summary>
		public List<ulong> Mention { get; set; } = new List<ulong>();

		public SpamPreventionUserInfo() { }
		public SpamPreventionUserInfo(SocketGuildUser user) : base(user) { }

		/// <summary>
		/// Returns true if the user should be punished at this point in time.
		/// </summary>
		/// <returns></returns>
		public bool IsPunishable()
		{
			return _Punishment != default && _VotesRequired != int.MaxValue;
		}
		/// <summary>
		/// Returns the spam amount for the supplied spam type. Time frame limits the max count by how close instances are.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="timeFrame"></param>
		/// <returns></returns>
		public int GetSpamAmount(SpamType type, int timeFrame)
		{
			switch (type)
			{
				case SpamType.Message:
					return DiscordUtils.CountItemsInTimeFrame(Message, timeFrame);
				case SpamType.LongMessage:
					return DiscordUtils.CountItemsInTimeFrame(LongMessage, timeFrame);
				case SpamType.Link:
					return DiscordUtils.CountItemsInTimeFrame(Link, timeFrame);
				case SpamType.Image:
					return DiscordUtils.CountItemsInTimeFrame(Image, timeFrame);
				case SpamType.Mention:
					return DiscordUtils.CountItemsInTimeFrame(Mention, timeFrame);
				default:
					throw new ArgumentException("Invalid spam type provided.", nameof(type));
			}
		}
		/// <summary>
		/// Adds a spam instance to the supplied spam type's list.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		public void AddSpamInstance(SpamType type, IMessage message)
		{
			switch (type)
			{
				case SpamType.Message:
					lock (Message)
					{
						if (!Message.Any(x => x == message.Id))
						{
							Message.Add(message.Id);
						}
					}
					return;
				case SpamType.LongMessage:
					lock (LongMessage)
					{
						if (!LongMessage.Any(x => x == message.Id))
						{
							LongMessage.Add(message.Id);
						}
					}
					return;
				case SpamType.Link:
					lock (Link)
					{
						if (!Link.Any(x => x == message.Id))
						{
							Link.Add(message.Id);
						}
					}
					return;
				case SpamType.Image:
					lock (Image)
					{
						if (!Image.Any(x => x == message.Id))
						{
							Image.Add(message.Id);
						}
					}
					return;
				case SpamType.Mention:
					lock (Mention)
					{
						if (!Mention.Any(x => x == message.Id))
						{
							Mention.Add(message.Id);
						}
					}
					return;
			}
		}
		/// <summary>
		/// Sets everything back to default values.
		/// </summary>
		public void Reset()
		{
			UsersWhoHaveAlreadyVoted = new List<ulong>();
			Message = new List<ulong>();
			LongMessage = new List<ulong>();
			Link = new List<ulong>();
			Image = new List<ulong>();
			Mention = new List<ulong>();
			VotesRequired = int.MaxValue;
			Punishment = default;
		}
	}
}