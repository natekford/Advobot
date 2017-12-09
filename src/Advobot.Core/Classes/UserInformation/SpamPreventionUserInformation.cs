using Advobot.Core.Actions;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.SpamPrevention;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.UserInformation
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
				this.SpamLists.TryAdd(spamType, new ConcurrentQueue<BasicTimeInterface>());
			}
		}

		public void IncreaseVotesToKick(ulong id)
		{
			if (!this.UsersWhoHaveAlreadyVoted.Any(x => x == id))
			{
				this.UsersWhoHaveAlreadyVoted.Add(id);
			}
		}
		public void ChangeVotesRequired(int newVotesRequired) => this.VotesRequired = Math.Min(newVotesRequired, this.VotesRequired);
		public void EnablePunishable() => this.PotentialPunishment = true;
		public void ChangePunishmentType(PunishmentType newPunishment)
		{
			if (_PunishmentSeverity[newPunishment] > _PunishmentSeverity[this.Punishment])
			{
				this.Punishment = newPunishment;
			}
		}

		public void ResetSpamUser()
		{
			this.UsersWhoHaveAlreadyVoted = new ConcurrentBag<ulong>();
			foreach (var kvp in this.SpamLists)
			{
				while (!kvp.Value.IsEmpty)
				{
					kvp.Value.TryDequeue(out var dequeueResult);
				}
			}

			this.VotesRequired = int.MaxValue;
			this.PotentialPunishment = false;
			this.Punishment = default;
		}
		public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
			=> this.SpamLists[spamType].CountItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
		public async Task PunishAsync(IGuildSettings guildSettings)
		{
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(this.Punishment, this.User, guildSettings.MuteRole).CAF();
		}
	}
}