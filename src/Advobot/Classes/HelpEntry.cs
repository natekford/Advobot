using System;
using System.Collections.Immutable;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public sealed class HelpEntry
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// How to use the command. This is automatically generated.
		/// </summary>
		public string Usage { get; }
		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		public string BasePerm { get; }
		/// <summary>
		/// Describes what the command does.
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		public ImmutableList<string> Aliases { get; }
		/// <summary>
		/// The category the command is in.
		/// </summary>
		public string Category { get; }
		/// <summary>
		/// Whether or not the command is on by default.
		/// </summary>
		public bool DefaultEnabled { get; }
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		public bool AbleToBeToggled { get; }

		internal HelpEntry(string name, string usage, string perms, string desc, string[] aliases, string category, bool defEnabled, bool toggleable)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("cant be null or whitespace", nameof(name));
			}

			Name = name;
			Usage = usage ?? "";
			BasePerm = String.IsNullOrWhiteSpace(perms) ? "N/A" : perms;
			Description = String.IsNullOrWhiteSpace(desc) ? "N/A" : desc;
			Aliases = (aliases ?? new[] { "N/A" }).ToImmutableList();
			Category = category;
			DefaultEnabled = defEnabled;
			AbleToBeToggled = toggleable;
		}

		/// <summary>
		/// Returns a string with all the information about the command.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"**Aliases:** {String.Join(", ", Aliases)}\n" +
				$"**Usage:** {Constants.PLACEHOLDER_PREFIX}{Name} {Usage}\n" +
				$"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}\n\n" +
				$"**Base Permission(s):**\n{BasePerm}\n\n" +
				$"**Description:**\n{Description}";
		}
	}
}