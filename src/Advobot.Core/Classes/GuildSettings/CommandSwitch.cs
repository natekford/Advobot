using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Advobot.Core.Classes.GuildSettings
{
	/*
	/// <summary>
	/// A setting on guilds that states whether a command is on or off.
	/// </summary>
	public class CommandSwitch : IGuildSetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public CommandCategory Category { get; }
		[JsonIgnore]
		public ImmutableList<string> Aliases { get; }

		public CommandSwitch(HelpEntry helpEntry)
		{
			Name = helpEntry?.Name;
			Category = helpEntry?.Category ?? default;
			Aliases = helpEntry?.Aliases?.ToImmutableList();
		}

		public override string ToString()
		{
			return $"`{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}*/
}
