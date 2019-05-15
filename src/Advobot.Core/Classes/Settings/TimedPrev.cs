using System;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	public abstract class TimedPrev<T> : IGuildFormattable where T : Enum
	{
		/// <summary>
		/// The type of thing this is preventing.
		/// </summary>
		[JsonProperty]
		public T Type { get; set; }
		/// <summary>
		/// The punishment to give raiders.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment { get; set; }
		/// <summary>
		/// How long the prevention should look at.
		/// </summary>
		[JsonProperty]
		public TimeSpan TimeInterval { get; set; }
		/// <summary>
		/// Whether or not this raid prevention is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; set; }

		//TODO: implement these
		//public abstract Task EnableAsync(SocketGuild guild);
		//public abstract Task DisableAsync(SocketGuild guild);
		/// <inheritdoc />
		public abstract string Format(SocketGuild? guild = null);
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}