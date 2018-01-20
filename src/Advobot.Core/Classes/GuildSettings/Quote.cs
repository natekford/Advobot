using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.GuildSettings
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : IGuildSetting, IDescription
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Description { get; }

		public Quote(string name, string description)
		{
			Name = name;
			Description = description;
		}

		public override string ToString()
		{
			return $"`{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}