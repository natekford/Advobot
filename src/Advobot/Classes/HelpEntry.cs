using System;
using System.Collections.Immutable;
using System.Reflection;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.UsageGeneration;
using AdvorangesUtils;
using Discord.Commands;

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
		public string BasePerms { get; }
		/// <summary>
		/// Describes what the command does.
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		public ImmutableArray<string> Aliases { get; }
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

		private string _A;
		private string _U;
		private string _E;
		private string _B;
		private string _D;

		/// <summary>
		/// Creates an instance of <see cref="HelpEntry"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="category"></param>
		public HelpEntry(Type type, string category)
		{
			Name = type.GetCustomAttribute<GroupAttribute>()?.Prefix;
			Usage = UsageGenerator.GenerateUsage(type);
			BasePerms = new[]
			{
				type.GetCustomAttribute<PermissionRequirementAttribute>()?.ToString(),
				type.GetCustomAttribute<OtherRequirementAttribute>()?.ToString()
			}.JoinNonNullStrings(" | ");
			Description = type.GetCustomAttribute<SummaryAttribute>()?.Text;
			Aliases = (type.GetCustomAttribute<AliasAttribute>()?.Aliases ?? new string[0]).ToImmutableArray();
			Category = category;
			DefaultEnabled = type.GetCustomAttribute<DefaultEnabledAttribute>()?.Enabled ?? false;
			AbleToBeToggled = type.GetCustomAttribute<DefaultEnabledAttribute>()?.AbleToToggle ?? true;

			SetStrings();
		}
		internal HelpEntry(string name, string usage, string perms, string desc, string[] aliases, string category, bool defEnabled, bool toggleable)
		{
			Name = name ?? throw new ArgumentException("Name cannott be null or whitespace.", nameof(name));
			Usage = usage ?? "";
			BasePerms = perms ?? "N/A";
			Description = desc ?? "N/A";
			Aliases = (aliases ?? new string[0]).ToImmutableArray();
			Category = category;
			DefaultEnabled = defEnabled;
			AbleToBeToggled = toggleable;

			SetStrings();
		}

		private void SetStrings()
		{
			_A = $"**Aliases:** {String.Join(", ", Aliases)}\n";
			_U = $"**Usage:** {Constants.PREFIX}{Name} {Usage}\n";
			_E = $"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}\n";
			_B = $"**Base Permission(s):**\n{BasePerms}\n";
			_D = $"**Description:**\n{Description}";
		}
		/// <summary>
		/// Returns a string with all the information about the command.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{_A}{_U}{_E}\n{_B}\n{_D}";
		}
		/// <summary>
		/// Returns a string with all the information about the command and whether it's currently enabled.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public string ToString(CommandSettings settings)
		{
			return $"{_A}{_U}{_E}**Currently Enabled:** {(settings.IsCommandEnabled(this) ? "Yes" : "No" )}\n\n{_B}\n{_D}";
		}
	}
}