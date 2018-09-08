using System;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : IGuildSetting, INameable
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
			Name = name ?? throw new ArgumentException(name, nameof(name));
			Description = description ?? throw new ArgumentException(description, nameof(description));
		}

		/// <inheritdoc />
		public override string ToString()
			=> $"`{Name}`";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> ToString();
	}
}