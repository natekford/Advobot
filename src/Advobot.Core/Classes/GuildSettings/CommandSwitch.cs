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
	public class CommandSwitch : IGuildSetting
	{
		[JsonIgnore]
		public CommandCategory Category { get; }
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public ImmutableList<string> Aliases { get; }
		[JsonProperty]
		public bool Value;

		public CommandSwitch(HelpEntry helpEntry, bool value)
		{
			Name = helpEntry?.Name;
			Value = value;
			Category = helpEntry?.Category ?? default;
			Aliases = helpEntry?.Aliases?.ToImmutableList();
		}

		public override string ToString()
		{
			return $"`{(Value ? "ON" : "OFF").PadRight(3)}` `{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
