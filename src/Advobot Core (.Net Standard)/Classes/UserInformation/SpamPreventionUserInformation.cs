using Advobot.Classes.Punishments;
using Advobot.Classes.SpamPrevention;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.UserInformation
{
	/// <summary>
	/// Keeps track how much spam this user has said, how many people need to vote, who has voted, and what punishment to give.
	/// </summary>
	public class SpamPreventionUserInformation : UserInfo
	{
		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		private static ImmutableDictionary<PunishmentType, int> _PunishmentSeverity = new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.VoiceMute, 100 },
			{ PunishmentType.RoleMute, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.Softban, 750 },
			{ PunishmentType.Ban, 1000 },
		}.ToImmutableDictionary();

		public ConcurrentBag<ulong> UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
		public ConcurrentDictionary<SpamType, ConcurrentQueue<BasicTimeInterface>> SpamLists = new ConcurrentDictionary<SpamType, ConcurrentQueue<BasicTimeInterface>>();

		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public PunishmentType Punishment { get; private set; } = default;

		public SpamPreventionUserInformation(IGuildUser user) : base(user)
		{
			foreach (SpamType spamType in Enum.GetValues(typeof(SpamType)))
			{
				SpamLists.TryAdd(spamType, new ConcurrentQueue<BasicTimeInterface>());
			}
		}

		public void IncreaseVotesToKick(ulong id)
		{
			if (!UsersWhoHaveAlreadyVoted.Any(x => x == id))
			{
				UsersWhoHaveAlreadyVoted.Add(id);
			}
		}
		public void ChangeVotesRequired(int newVotesRequired)
		{
			VotesRequired = Math.Min(newVotesRequired, VotesRequired);
		}
		public void EnablePunishable()
		{
			PotentialPunishment = true;
		}
		public void ChangePunishmentType(PunishmentType newPunishment)
		{
			if (_PunishmentSeverity[newPunishment] > _PunishmentSeverity[Punishment])
			{
				Punishment = newPunishment;
			}
		}

		public void ResetSpamUser()
		{
			UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
			foreach (var kvp in SpamLists)
			{
				while (!kvp.Value.IsEmpty)
				{
					kvp.Value.TryDequeue(out var dequeueResult);
				}
			}

			VotesRequired = int.MaxValue;
			PotentialPunishment = false;
			Punishment = default;
		}
		public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
		{
			return SpamLists[spamType].CountItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task PunishAsync(IGuildSettings guildSettings)
		{
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(Punishment, User, guildSettings.MuteRole).CAF();
		}
	}
}