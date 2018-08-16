using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : IGuildSetting
	{
		/// <summary>
		/// The name of the quote.
		/// </summary>
		[JsonProperty]
		public string Name { get; }
		/// <summary>
		/// The description of the quote.
		/// </summary>
		[JsonProperty]
		public string Description { get; }

		/// <summary>
		/// Creates an instance of quote.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		public Quote(string name, string description)
		{
			Name = name;
			Description = description;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"`{Name}`";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}