using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Formatting;

using Discord;

using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// A prevention of some type which is timed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class TimedPrev<T> : IGuildFormattable where T : struct, Enum
	{
		/// <summary>
		/// Whether or not this raid prevention is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; protected set; }

		/// <summary>
		/// The punishment to give raiders.
		/// </summary>
		[JsonProperty("Punishment")]
		public PunishmentType Punishment { get; set; }

		/// <summary>
		/// The role to give as a punishment.
		/// </summary>
		[JsonProperty("Role")]
		public ulong? RoleId { get; set; }

		/// <summary>
		/// How long the prevention should look at.
		/// </summary>
		[JsonProperty("Time")]
		public TimeSpan TimeInterval { get; set; }

		/// <summary>
		/// The type of thing this is preventing.
		/// </summary>
		[JsonProperty("Type")]
		public T Type { get; set; }

		/// <summary>
		/// Disables this timed prevention.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public abstract Task DisableAsync(IGuild guild);

		/// <summary>
		/// Enables this timed prevention.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public abstract Task EnableAsync(IGuild guild);

		/// <inheritdoc />
		public abstract IDiscordFormattableString GetFormattableString();
	}
}