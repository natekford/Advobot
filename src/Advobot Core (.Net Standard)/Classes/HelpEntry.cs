using Advobot.Enums;
using Advobot.Interfaces;
using System;
using System.Text;
using Advobot.Actions.Formatting;

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
			return new StringBuilder()
				.AppendLineFeed($"**Aliases:** {String.Join(", ", Aliases)}")
				.AppendLineFeed($"**Usage:** {Usage}")
				.AppendLineFeed($"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}")
				.AppendLineFeed($"\n**Base Permission(s):**\n{BasePerm}")
				.AppendLineFeed($"\n**Description:**\n{Description}")
				.ToString();
		}
	}
}