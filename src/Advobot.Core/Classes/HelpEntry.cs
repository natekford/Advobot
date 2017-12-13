using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using System;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public class HelpEntry : IDescription
	{
		public string Name { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Description { get; }
		public string[] Aliases { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }

		public HelpEntry(string name, string usage, string basePerm, string description, string[] aliases, CommandCategory category, bool defaultEnabled)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Invalid name.");
			}

			Name = name;
			Usage = usage ?? "";
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? "N/A" : basePerm;
			Description = String.IsNullOrWhiteSpace(description) ? "N/A" : description;
			Aliases = aliases ?? new[] { "N/A" };
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"**Aliases:** {String.Join(", ", Aliases)}")
			.AppendLineFeed($"**Usage:** {Constants.PLACEHOLDER_PREFIX}{Name} {Usage}")
			.AppendLineFeed($"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}")
			.AppendLineFeed($"\n**Base Permission(s):**\n{BasePerm}")
			.AppendLineFeed($"\n**Description:**\n{Description}").ToString();
	}
}