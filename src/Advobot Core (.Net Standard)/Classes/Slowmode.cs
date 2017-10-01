using Advobot.Actions;
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
	/// <summary>
	/// Limits the amount of messages users are allowed to send in a given time interval. Initially created with <see cref="Enabled"/> set to false.
	/// </summary>
	public class Slowmode : ISetting
	{
		[JsonProperty]
		public int BaseMessages { get; }
		[JsonProperty]
		public int Interval { get; }
		[JsonProperty]
		public ulong[] ImmuneRoleIds { get; }
		[JsonIgnore]
		public bool Enabled { get; private set; }
		[JsonIgnore]
		private Dictionary<ulong, SlowmodeUserInformation> Users;

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
			Enabled = false;
		}

		public void Disable()
		{
			Users.Clear();
			Enabled = false;
		}
		public void Enable()
		{
			Users = new Dictionary<ulong, SlowmodeUserInformation>();
			Enabled = true;
		}

		/// <summary>
		/// Adds the user to the list of slowmode users then lowers messages by one. If no more messages remain this will delete the message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="author"></param>
		/// <returns></returns>
		public async Task HandleMessage(IMessage message)
		{
			SlowmodeUserInformation info;
			lock (Users)
			{
				if (!Users.TryGetValue(message.Author.Id, out info))
				{
					Users.Add(message.Author.Id, info = new SlowmodeUserInformation(BaseMessages, Interval));
				}
			}

			if (info.CurrentMessagesLeft > 0)
			{
				if (info.CurrentMessagesLeft == BaseMessages)
				{
					info.UpdateTime(Interval);
				}

				info.LowerMessagesLeft();
			}
			else
			{
				await MessageActions.DeleteMessage(message);
			}
		}
		/// <summary>
		/// For each user in the slowmode users list that has a reset time of less than now this will reset their message count to default.
		/// </summary>
		public void ResetUsers()
		{
			var usersToBeReset = Users.Select(x => x.Value).Where(x => x.GetTime() < DateTime.UtcNow);
			foreach (var user in usersToBeReset)
			{
				user.ResetMessagesLeft(BaseMessages);
			}
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

		/// <summary>
		/// Holds information for a user in relation to slowmode.
		/// </summary>
		private class SlowmodeUserInformation : IHasTime
		{
			public int CurrentMessagesLeft { get; private set; }
			private DateTime _Time;

			public SlowmodeUserInformation(int baseMessages, int interval)
			{
				CurrentMessagesLeft = baseMessages;
				_Time = DateTime.UtcNow.AddSeconds(interval);
			}

			public void LowerMessagesLeft()
			{
				--CurrentMessagesLeft;
			}
			public void ResetMessagesLeft(int messagesLeft)
			{
				CurrentMessagesLeft = messagesLeft;
			}
			public void UpdateTime(int interval)
			{
				_Time = DateTime.UtcNow.AddSeconds(interval);
			}

			public DateTime GetTime() => _Time;
		}
	}
}
