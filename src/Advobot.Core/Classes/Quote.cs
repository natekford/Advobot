using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : ISetting, IDescription
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Description { get; }

		public Quote(string name, string description)
		{
			this.Name = name;
			this.Description = description;
		}

		public override string ToString() => $"`{this.Name}`";
		public string ToString(SocketGuild guild) => ToString();
	}
}