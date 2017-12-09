using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes
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
			this.Name = name;
			this.Id = id;
		}

		public override string ToString() => $"**Command:** `{this.Name}`\n**ID:** `{this.Id}`";
		public string ToString(SocketGuild guild) => ToString();
	}
}
