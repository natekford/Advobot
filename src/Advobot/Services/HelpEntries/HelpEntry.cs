using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.UsageGeneration;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	internal sealed class HelpEntry : IHelpEntry
	{
		/// <inheritdoc />
		public bool AbleToBeToggled { get; }
		/// <inheritdoc />
		public ImmutableArray<string> Aliases { get; }
		/// <inheritdoc />
		public string BasePerms { get; }
		/// <inheritdoc />
		public string Category { get; }
		/// <inheritdoc />
		public bool DefaultEnabled { get; }
		/// <inheritdoc />
		public string Description { get; }
		/// <inheritdoc />
		public string Name { get; }
		/// <inheritdoc />
		public string Usage { get; }

		private string _A;
		private string _U;
		private string _E;
		private string _B;
		private string _D;

		/// <summary>
		/// Creates an instance of <see cref="HelpEntry"/> from a type.
		/// </summary>
		/// <param name="type"></param>
		public HelpEntry(Type type)
		{
			var attrs = type.GetCustomAttributes();
			AbleToBeToggled = attrs.GetAttribute<DefaultEnabledAttribute>().AbleToToggle;
			Aliases = (attrs.GetAttribute<AliasAttribute>()?.Aliases ?? new[] { "N/A" }).ToImmutableArray();
			BasePerms = new[]
			{
				attrs.GetAttribute<PermissionRequirementAttribute>()?.ToString(),
				attrs.GetAttribute<OtherRequirementAttribute>()?.ToString()
			}.JoinNonNullStrings(" | ") is string str && !String.IsNullOrWhiteSpace(str) ? str : "N/A";
			Category = attrs.GetAttribute<CategoryAttribute>().Category ?? throw new ArgumentException(nameof(CategoryAttribute));
			DefaultEnabled = attrs.GetAttribute<DefaultEnabledAttribute>().Enabled;
			Description = attrs.GetAttribute<SummaryAttribute>().Text ?? throw new ArgumentException(nameof(SummaryAttribute));
			Name = attrs.GetAttribute<GroupAttribute>().Prefix ?? throw new ArgumentException(nameof(GroupAttribute));
			Usage = UsageGenerator.GenerateUsage(type) ?? throw new ArgumentException(nameof(Usage));

			SetStrings();
		}
		/// <summary>
		/// Creates an instance of <see cref="HelpEntry"/> from a module.
		/// </summary>
		/// <param name="module"></param>
		public HelpEntry(ModuleInfo module)
		{
			var attrs = module.Attributes;
			AbleToBeToggled = attrs.GetAttribute<DefaultEnabledAttribute>().AbleToToggle;
			Aliases = (module.Aliases.Any() ? module.Aliases : new[] { "N/A" }).ToImmutableArray();
			BasePerms = new[]
			{
				attrs.GetAttribute<PermissionRequirementAttribute>()?.ToString(),
				attrs.GetAttribute<OtherRequirementAttribute>()?.ToString()
			}.JoinNonNullStrings(" | ") is string str && !String.IsNullOrWhiteSpace(str) ? str : "N/A";
			Category = attrs.GetAttribute<CategoryAttribute>().Category ?? throw new ArgumentException(nameof(CategoryAttribute));
			DefaultEnabled = attrs.GetAttribute<DefaultEnabledAttribute>().Enabled;
			Description = module.Summary ?? throw new ArgumentException(nameof(SummaryAttribute));
			Name = module.Name ?? throw new ArgumentException(nameof(GroupAttribute));
			Usage = UsageGenerator.GenerateUsage(module) ?? throw new ArgumentException(nameof(Usage));

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
		/// <inheritdoc />
		public override string ToString()
		{
			return $"{_A}{_U}{_E}\n{_B}\n{_D}";
		}
		/// <inheritdoc />
		public string ToString(CommandSettings settings)
		{
			if (!(settings.IsCommandEnabled(Name) is bool val))
			{
				val = DefaultEnabled;
			}
			return $"{_A}{_U}{_E}**Currently Enabled:** {(val ? "Yes" : "No" )}\n\n{_B}\n{_D}";
		}
	}
}