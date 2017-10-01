using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.SpamPrevention
{
	public class SpamPreventionUser
	{
		public readonly IGuildUser User;
		public readonly List<ulong> UsersWhoHaveAlreadyVoted = new List<ulong>();
		public readonly Dictionary<SpamType, List<BasicTimeInterface>> SpamLists = new Dictionary<SpamType, List<BasicTimeInterface>>();

		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public PunishmentType Punishment { get; private set; } = default;

		public SpamPreventionUser(IGuildUser user)
		{
			User = user;
			foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
			{
				SpamLists.Add(spamType, new List<BasicTimeInterface>());
			}
		}

		public void IncreaseVotesToKick(ulong Id)
		{
			UsersWhoHaveAlreadyVoted.ThreadSafeAdd(Id);
		}
		public void ChangeVotesRequired(int newVotesRequired)
		{
			VotesRequired = Math.Min(newVotesRequired, VotesRequired);
		}
		public void ChangePunishmentType(PunishmentType newPunishment)
		{
			if (Constants.PUNISHMENT_SEVERITY[newPunishment] > Constants.PUNISHMENT_SEVERITY[Punishment])
			{
				Punishment = newPunishment;
			}
		}
		public void EnablePunishable()
		{
			PotentialPunishment = true;
		}
		public void ResetSpamUser()
		{
			//Don't reset already kicked since KickThenBan requires it
			UsersWhoHaveAlreadyVoted.Clear();
			foreach (var spamList in SpamLists.Values)
			{
				spamList.Clear();
			}

			VotesRequired = int.MaxValue;
			PotentialPunishment = false;
			Punishment = default;
		}
		public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
		{
			return SpamLists[spamType].CountItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task SpamPreventionPunishment(IGuildSettings guildSettings)
		{
			//TODO: make this not 0
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(Punishment, User, guildSettings.MuteRole);
		}
	}
}