using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Advobot.Enums;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.UserInformation
{
	/// <summary>
	/// Keeps track how much spam this user has said, how many people need to vote, who has voted, and what punishment to give.
	/// </summary>
	public sealed class SpamPreventionUserInfo : UserInfo
	{
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
			set => _Punishment = Math.Max((int)value, _Punishment);
		}
		/// <summary>
		/// The amount of people who have voted for this person to be kicked.
		/// </summary>
		public int VotesReceived => _UsersWhoHaveAlreadyVoted.Count;

		private int _VotesRequired = int.MaxValue;
		private int _Punishment = int.MinValue;
		private ConcurrentDictionary<ulong, byte> _UsersWhoHaveAlreadyVoted = new ConcurrentDictionary<ulong, byte>();
		private ConcurrentDictionary<SpamType, ConcurrentDictionary<ulong, byte>> _Dictionary = new ConcurrentDictionary<SpamType, ConcurrentDictionary<ulong, byte>>();

		/// <summary>
		/// Creates an instance of <see cref="SpamPreventionUserInfo"/>.
		/// </summary>
		/// <param name="user"></param>
		public SpamPreventionUserInfo(SocketGuildUser user) : base(user) { }

		/// <summary>
		/// Returns true if the user should be kicked after the passed in vote has been added.
		/// </summary>
		/// <param name="vote"></param>
		/// <returns></returns>
		public bool ShouldBePunished(IUserMessage vote)
		{
			return VotesReceived >= VotesRequired
				|| (_Punishment != int.MinValue //If still default value, don't do anything
				&& _VotesRequired != int.MaxValue //If still default value, don't do anything
				&& vote.Author.Id != UserId //Don't allow voting on self
				&& _UsersWhoHaveAlreadyVoted.TryAdd(vote.Author.Id, 0) //Don't allow duplicate votes
				&& vote.MentionedUserIds.Contains(UserId) //Don't count the vote if the user isn't mentioned
				&& VotesReceived >= VotesRequired); //Allow the user to be punished if there are enough votes for them
		}
		/// <summary>
		/// Returns the spam amount for the supplied spam type. Time frame limits the max count by how close instances are.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public int GetSpamAmount(SpamType type, TimeSpan? time)
			=> DiscordUtils.CountItemsInTimeFrame(_Dictionary.GetOrAdd(type, new ConcurrentDictionary<ulong, byte>()).Keys, time);
		/// <summary>
		/// Adds a spam instance to the supplied spam type's list.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		public bool AddSpamInstance(SpamType type, IMessage message)
			=> _Dictionary.GetOrAdd(type, new ConcurrentDictionary<ulong, byte>()).TryAdd(message.Id, 0);
		/// <inheritdoc />
		public override void Reset()
		{
			Interlocked.Exchange(ref _VotesRequired, int.MaxValue);
			Interlocked.Exchange(ref _Punishment, (int)default(Punishment));
			Interlocked.Exchange(ref _UsersWhoHaveAlreadyVoted, new ConcurrentDictionary<ulong, byte>());
			Interlocked.Exchange(ref _Dictionary, new ConcurrentDictionary<SpamType, ConcurrentDictionary<ulong, byte>>());
		}
	}
}