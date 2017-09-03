using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	public class Slowmode : ISetting
	{
		[JsonProperty]
		public int BaseMessages { get; }
		[JsonProperty]
		public int Interval { get; }
		[JsonProperty]
		public ulong[] ImmuneRoleIds { get; }
		[JsonIgnore]
		public List<SlowmodeUser> Users { get; }
		[JsonIgnore]
		public bool Enabled { get; private set; }

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			Users = new List<SlowmodeUser>();
			ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
			Enabled = false;
		}

		public void Disable()
		{
			Enabled = false;
		}
		public void Enable()
		{
			Enabled = true;
		}

		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
					$"**Time interval:** `{Interval}`\n" +
					$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds)}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class SlowmodeUser : ITimeInterface
	{
		public IGuildUser User { get; }
		public int BaseMessages { get; }
		public int Interval { get; }
		public int CurrentMessagesLeft { get; private set; }
		public DateTime Time { get; private set; }

		public SlowmodeUser(IGuildUser user, int baseMessages, int interval)
		{
			User = user;
			BaseMessages = baseMessages;
			Interval = interval;
			CurrentMessagesLeft = baseMessages;
		}

		public void LowerMessagesLeft()
		{
			--CurrentMessagesLeft;
		}
		public void ResetMessagesLeft()
		{
			CurrentMessagesLeft = BaseMessages;
		}
		public void SetNewTime()
		{
			Time = DateTime.UtcNow.AddSeconds(Interval);
		}
		public DateTime GetTime()
		{
			return Time;
		}
	}
}
