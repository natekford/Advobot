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
		//Because the enum values might change in the future. These are never saved in json so these can be modified
		private static Dictionary<Punishment, int> _PunishmentSeverity = new Dictionary<Punishment, int>
		{
			{ Punishment.Deafen, 0 },
			{ Punishment.VoiceMute, 100 },
			{ Punishment.RoleMute, 250 },
			{ Punishment.Kick, 500 },
			{ Punishment.Softban, 750 },
			{ Punishment.Ban, 1000 }
		};

		//TODO: check if this serializes correctly
		private List<ulong> _UsersWhoHaveAlreadyVoted = new List<ulong>();
		private Dictionary<SpamType, List<ulong>> _Spam = CreateDictionary();
		private int _VotesRequired = int.MaxValue;
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
		/// Returns true if <see cref="Punishment"/> is not default and <see cref="VotesRequired"/> is greater than 0.
		/// </summary>
		public bool PotentialPunishment => _Punishment != default && _VotesRequired != int.MaxValue;
		/// <summary>
		/// Returns the count of people who have voted to punish the user.
		/// </summary>
		public int Votes => _UsersWhoHaveAlreadyVoted.Count;

		public SpamPreventionUserInfo() { }
		public SpamPreventionUserInfo(SocketGuildUser user) : base(user) { }

		public bool HasUserAlreadyVoted(ulong id)
		{
			return _UsersWhoHaveAlreadyVoted.Contains(id);
		}
		public int GetSpamAmount(SpamType type, int timeFrame)
		{
			return timeFrame < 1 ? _Spam[type].Count : DiscordUtils.CountItemsInTimeFrame(_Spam[type], timeFrame);
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
			var list = _Spam[type];
			lock (list)
			{
				if (!list.Any(x => x == message.Id))
				{
					list.Add(message.Id);
				}
			}
		}
		public void Reset()
		{
			Interlocked.Exchange(ref _UsersWhoHaveAlreadyVoted, new List<ulong>());
			Interlocked.Exchange(ref _Spam, CreateDictionary());

			VotesRequired = -1;
			Punishment = default;
		}

		private static Dictionary<SpamType, List<ulong>> CreateDictionary()
		{
			return Enum.GetValues(typeof(SpamType)).Cast<SpamType>().ToDictionary(x => x, x => new List<ulong>());
		}
	}
}