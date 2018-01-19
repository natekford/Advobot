using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Keeps track how much spam this user has said, how many people need to vote, who has voted, and what punishment to give.
	/// </summary>
	public sealed class SpamPreventionUserInfo : UserInfo
	{
		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		private static Dictionary<PunishmentType, int> _PunishmentSeverity = new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.VoiceMute, 100 },
			{ PunishmentType.RoleMute, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.Softban, 750 },
			{ PunishmentType.Ban, 1000 },
		};

		private ConcurrentBag<ulong> _UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
		private ConcurrentDictionary<SpamType, ConcurrentQueue<SpamInstance>> _Spam = CreateDictionary();

		private int _VotesRequired = -1;
		/// <summary>
		/// The votes required to punish a user.
		/// Setting sets to the lowest of the new value or old value.
		/// </summary>
		public int VotesRequired
		{
			get => _VotesRequired;
			set => _VotesRequired = Math.Min(_VotesRequired, value);
		}
		private PunishmentType _Punishment = default;
		/// <summary>
		/// The punishment to do on a user.
		/// Setting sets to whatever is the most severe punishment.
		/// </summary>
		public PunishmentType Punishment
		{
			get => _Punishment;
			set => _Punishment = _PunishmentSeverity[value] > _PunishmentSeverity[_Punishment] ? value : _Punishment;
		}
		public bool PotentialPunishment => _Punishment != default && _VotesRequired > 0;
		public int Votes => _UsersWhoHaveAlreadyVoted.Count;

		public SpamPreventionUserInfo(IGuildUser user) : base(user) { }

		public bool HasUserAlreadyVoted(ulong id)
		{
			return _UsersWhoHaveAlreadyVoted.Contains(id);
		}
		public int GetSpamAmount(SpamType type, int timeFrame)
		{
			return timeFrame < 1 ? _Spam[type].Count : _Spam[type].CountItemsInTimeFrame(timeFrame);
		}
		public void IncreaseVotes(ulong id)
		{
			if (!HasUserAlreadyVoted(id))
			{
				_UsersWhoHaveAlreadyVoted.Add(id);
			}
		}
		public void AddSpamInstance(SpamType type, IMessage message)
		{
			var queue = _Spam[type];
			if (!queue.Any(x => x.MessageId == message.Id))
			{
				queue.Enqueue(new SpamInstance(message));
			}
		}
		public void Reset()
		{
			Interlocked.Exchange(ref _UsersWhoHaveAlreadyVoted, new ConcurrentBag<ulong>());
			Interlocked.Exchange(ref _Spam, CreateDictionary());

			VotesRequired = -1;
			Punishment = default;
		}

		private static ConcurrentDictionary<SpamType, ConcurrentQueue<SpamInstance>> CreateDictionary()
		{
			var temp = new ConcurrentDictionary<SpamType, ConcurrentQueue<SpamInstance>>();
			foreach (SpamType spamType in Enum.GetValues(typeof(SpamType)))
			{
				temp.TryAdd(spamType, new ConcurrentQueue<SpamInstance>());
			}
			return temp;
		}

		private struct SpamInstance : ITime
		{
			public ulong MessageId { get; }
			public DateTime Time { get; }

			public SpamInstance(IMessage message)
			{
				MessageId = message.Id;
				Time = message.CreatedAt.UtcDateTime;
			}
		}
	}
}