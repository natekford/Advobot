using Advobot.Classes.Punishments;
using Advobot.Classes.SpamPrevention;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.UserInformation
{
	public class SpamPreventionUserInformation : UserInfo
	{
		public ConcurrentBag<ulong> UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
		public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists = new Dictionary<SpamType, List<BasicTimeInterface>>();

		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public PunishmentType Punishment { get; private set; } = default;

		public SpamPreventionUserInformation(IGuildUser user) : base(user)
		{
			foreach (SpamType spamType in Enum.GetValues(typeof(SpamType)))
			{
				SpamLists.Add(spamType, new List<BasicTimeInterface>());
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
			if (Constants.PUNISHMENT_SEVERITY[newPunishment] > Constants.PUNISHMENT_SEVERITY[Punishment])
			{
				Punishment = newPunishment;
			}
		}

		public void ResetSpamUser()
		{
			UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
			foreach (var kvp in SpamLists)
			{
				kvp.Value.Clear();
			}

			VotesRequired = int.MaxValue;
			PotentialPunishment = false;
			Punishment = default;
		}
		public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
		{
			return SpamLists[spamType].CountItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task Punish(IGuildSettings guildSettings)
		{
			//TODO: make this not 0
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(Punishment, User, guildSettings.MuteRole);
		}
	}
}