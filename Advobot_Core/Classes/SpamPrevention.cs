using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	public class SpamPreventionInfo : ISetting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int RequiredSpamInstances { get; }
		[JsonProperty]
		public int RequiredSpamPerMessageOrTimeInterval { get; }
		[JsonProperty]
		public int VotesForKick { get; }
		[JsonIgnore]
		public bool Enabled { get; private set; }

		public SpamPreventionInfo(PunishmentType punishmentType, int requiredSpamInstances, int requiredSpamPerMessageOrTimeInterval, int votesForKick)
		{
			PunishmentType = punishmentType;
			RequiredSpamInstances = requiredSpamInstances;
			RequiredSpamPerMessageOrTimeInterval = requiredSpamPerMessageOrTimeInterval;
			VotesForKick = votesForKick;
			Enabled = false;
		}

		public void Enable()
		{
			Enabled = true;
		}
		public void Disable()
		{
			Enabled = false;
		}

		public override string ToString()
		{
			return $"**Punishment:** `{PunishmentType.EnumName()}`\n" +
					$"**Spam Instances:** `{RequiredSpamInstances}`\n" +
					$"**Votes For Punishment:** `{VotesForKick}`\n" +
					$"**Spam Amt/Time Interval:** `{RequiredSpamPerMessageOrTimeInterval}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class RaidPreventionInfo : ISetting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int Interval { get; }
		[JsonProperty]
		public int UserCount { get; }
		[JsonProperty]
		public bool Enabled { get; private set; }
		[JsonIgnore]
		public List<BasicTimeInterface> TimeList { get; }

		public RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
		{
			PunishmentType = punishmentType;
			UserCount = userCount;
			Interval = interval;
			TimeList = new List<BasicTimeInterface>();
			Enabled = true;
		}

		public int GetSpamCount()
		{
			return TimeList.GetCountOfItemsInTimeFrame(Interval);
		}
		public void Add(DateTime time)
		{
			TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
		}
		public void Remove(DateTime time)
		{
			TimeList.ThreadSafeRemoveAll(x => x.GetTime().Equals(time));
		}
		public void Reset()
		{
			TimeList.Clear();
		}
		public async Task RaidPreventionPunishment(IGuildSettings guildSettings, IGuildUser user, ITimersModule timers = null)
		{
			//TODO: make this not 0
			await PunishmentActions.AutomaticPunishments(guildSettings, user, PunishmentType, false, 0, timers);
		}

		public void Enable()
		{
			Enabled = true;
		}
		public void Disable()
		{
			Enabled = false;
		}

		public override string ToString()
		{
			return $"**Enabled:** `{Enabled}`\n" +
					$"**Users:** `{UserCount}`\n" +
					$"**Time Interval:** `{Interval}`\n" +
					$"**Punishment:** `{PunishmentType.EnumName()}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class SpamPreventionUser
	{
		public IGuildUser User { get; }
		public List<ulong> UsersWhoHaveAlreadyVoted { get; } = new List<ulong>();
		public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; } = new Dictionary<SpamType, List<BasicTimeInterface>>();

		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public bool AlreadyKicked { get; private set; } = false;
		public PunishmentType Punishment { get; private set; } = default(PunishmentType);

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
			Punishment = default(PunishmentType);
		}
		public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
		{
			return SpamLists[spamType].GetCountOfItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task SpamPreventionPunishment(IGuildSettings guildSettings, ITimersModule timers = null)
		{
			//TODO: make this not 0
			await PunishmentActions.AutomaticPunishments(guildSettings, User, Punishment, AlreadyKicked, 0, timers);
		}
	}
}
