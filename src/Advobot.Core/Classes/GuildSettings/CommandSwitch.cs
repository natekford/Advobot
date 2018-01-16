using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Advobot.Core.Classes.GuildSettings
{
	/// <summary>
	/// A setting on guilds that states whether a command is on or off.
	/// </summary>
	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public bool Value;
		[JsonIgnore]
		public ImmutableArray<string> Aliases { get; }
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
			Aliases = helpEntry.Aliases.ToImmutableArray();
		}

		public override string ToString()
		{
			return $"`{ValueAsString.PadRight(3)}` `{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
