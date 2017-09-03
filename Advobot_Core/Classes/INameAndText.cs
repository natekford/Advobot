using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace Advobot.Classes
{
	public class Quote : ISetting, INameAndText
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Text { get; }

		public Quote(string name, string text)
		{
			Name = name;
			Text = text;
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

	public class HelpEntry : INameAndText
	{
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Text { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }
		private const string PLACE_HOLDER_STR = "N/A";

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
			Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
			Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.BOT_PREFIX + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
			Text = String.IsNullOrWhiteSpace(text) ? PLACE_HOLDER_STR : text;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
			var usageStr = $"**Usage:** {Usage}";
			var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
			var descStr = $"\n**Description:**\n{Text}";
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}
}
