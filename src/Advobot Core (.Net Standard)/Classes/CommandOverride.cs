using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// A setting on a guild that states that the command is off for whatever Discord entity has that Id.
	/// </summary>
	public class CommandOverride : ISetting
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
			return $"**Command:** `{Name}`\n**ID:** `{Id}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
