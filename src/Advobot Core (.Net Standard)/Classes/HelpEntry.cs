using Advobot.Enums;
using Advobot.Interfaces;
using System;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public class HelpEntry : IDescription
	{
		private const string PLACE_HOLDER_STR = "N/A";
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Description { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string description, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
			Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
			Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.PLACEHOLDER_PREFIX + " " + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
			Description = String.IsNullOrWhiteSpace(description) ? PLACE_HOLDER_STR : description;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
			var usageStr = $"**Usage:** {Usage}";
			var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
			var descStr = $"\n**Description:**\n{Description}";
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}
}