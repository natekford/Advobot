using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A setting on guilds that states whether a command is on or off.
	/// </summary>
	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public ImmutableList<string> Aliases { get; }
		[JsonIgnore]
		public CommandCategory Category { get; }
		[JsonIgnore]
		public string ValueAsString => Value ? "ON" : "OFF";

		public CommandSwitch(string name, bool value)
		{
			var helpEntry = Constants.HELP_ENTRIES[name];
			if (helpEntry == null)
			{
				Category = default;
				return;
			}

			Name = name;
			Value = value;
			Category = helpEntry.Category;
			Aliases = helpEntry.Aliases.ToImmutableList();
		}

		/// <summary>
		/// Sets <see cref="Value"/> to its opposite.
		/// </summary>
		public void ToggleEnabled() => Value = !Value;

		public override string ToString() => $"`{ValueAsString.PadRight(3)}` `{Name}`";
		public string ToString(SocketGuild guild) => ToString();
	}
}
