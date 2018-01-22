using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.GuildSettings
{
	/*
	/// <summary>
	/// A setting on a guild that states that the command is off for whatever Discord entity has that Id.
	/// </summary>
	public class CommandOverride : IGuildSetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public ulong Id { get; }

		public CommandOverride(string name, ulong id)
		{
			Name = name;
			Id = id;
		}

		public override string ToString()
		{
			return $"**Command:** `{Name}` **ID:** `{Id}`";
		}
		public string ToString(SocketGuild guild)
		{
			var chan = guild.GetChannel(Id);
			if (chan != null)
			{
				return $"**Command:** `{Name}` **Channel:** `{chan.Format()}`";
			}
			var role = guild.GetRole(Id);
			if (role != null)
			{
				return $"**Command:** `{Name}` **Role:** `{role.Format()}`";
			}
			var user = guild.GetUser(Id);
			if (user != null)
			{
				return $"**Command:** `{Name}` **User:** `{user.Format()}`";
			}
			return ToString();
		}
	}*/
}
