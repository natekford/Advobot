using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Keeps track how much spam this user has said, how many people need to vote, who has voted, and what punishment to give.
	/// </summary>
	public sealed class SpamPreventionUserInfo : UserInfo
	{
		private int _VotesRequired = int.MaxValue;
		private int _Punishment = -1;
		private List<ulong> _UsersWhoHaveAlreadyVoted = new List<ulong>();
		private List<ulong> _Message = new List<ulong>();
		private List<ulong> _LongMessage = new List<ulong>();
		private List<ulong> _Link = new List<ulong>();
		private List<ulong> _Image = new List<ulong>();
		private List<ulong> _Mention = new List<ulong>();

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
			get => (Punishment)_Punishment;
			set => _Punishment = Math.Min((int)value, _Punishment);
		}
		/// <summary>
		/// Who has voted to punish the user.
		/// </summary>
		public List<ulong> UsersWhoHaveAlreadyVoted
		{
			get => _UsersWhoHaveAlreadyVoted;
			set => Interlocked.Exchange(ref _UsersWhoHaveAlreadyVoted, new List<ulong>());
		}
		/// <summary>
		/// The amount of spam instances associated with the amount of messages sent.
		/// </summary>
		public List<ulong> Message
		{
			get => _Message;
			set => Interlocked.Exchange(ref _Message, new List<ulong>());
		}
		/// <summary>
		/// The amount of spam instances associated with the length of messages sent.
		/// </summary>
		public List<ulong> LongMessage
		{
			get => _LongMessage;
			set => Interlocked.Exchange(ref _LongMessage, new List<ulong>());
		}
		/// <summary>
		/// The amount of spam instances associated with how many links are in messages sent.
		/// </summary>
		public List<ulong> Link
		{
			get => _Link;
			set => Interlocked.Exchange(ref _Link, new List<ulong>());
		}
		/// <summary>
		/// The amount of spam instances associated with how many images are in messages sent.
		/// </summary>
		public List<ulong> Image
		{
			get => _Image;
			set => Interlocked.Exchange(ref _Image, new List<ulong>());
		}
		/// <summary>
		/// The amount of spam instances associated with how many mentions are in messages sent.
		/// </summary>
		public List<ulong> Mention
		{
			get => _Mention;
			set => Interlocked.Exchange(ref _Mention, new List<ulong>());
		}

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
		public override void Reset()
		{
			Interlocked.Exchange(ref _VotesRequired, int.MaxValue);
			Interlocked.Exchange(ref _Punishment, (int)default(Punishment));
			Interlocked.Exchange(ref _UsersWhoHaveAlreadyVoted, new List<ulong>());
			Interlocked.Exchange(ref _Message, new List<ulong>());
			Interlocked.Exchange(ref _LongMessage, new List<ulong>());
			Interlocked.Exchange(ref _Link, new List<ulong>());
			Interlocked.Exchange(ref _Image, new List<ulong>());
			Interlocked.Exchange(ref _Mention, new List<ulong>());
		}
	}
}