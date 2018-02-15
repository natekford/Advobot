using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : IGuildSetting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public string Description { get; private set; }

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