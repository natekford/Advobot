using Advobot.Core.Actions.Formatting;
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

			this.Name = name;
			this.Usage = usage ?? "";
			this.BasePerm = String.IsNullOrWhiteSpace(basePerm) ? "N/A" : basePerm;
			this.Description = String.IsNullOrWhiteSpace(description) ? "N/A" : description;
			this.Aliases = aliases ?? new[] { "N/A" };
			this.Category = category;
			this.DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"**Aliases:** {String.Join(", ", this.Aliases)}")
			.AppendLineFeed($"**Usage:** {Constants.PLACEHOLDER_PREFIX}{this.Name} {this.Usage}")
			.AppendLineFeed($"**Enabled By Default:** {(this.DefaultEnabled ? "Yes" : "No")}")
			.AppendLineFeed($"\n**Base Permission(s):**\n{this.BasePerm}")
			.AppendLineFeed($"\n**Description:**\n{this.Description}").ToString();
	}
}