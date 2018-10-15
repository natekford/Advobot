using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Settings;
using Advobot.Classes.UsageGeneration;
using Advobot.Interfaces;
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

		/// <summary>
		/// Creates an instance of <see cref="HelpEntry"/> from a type.
		/// </summary>
		/// <param name="type"></param>
		public HelpEntry(Type type)
		{
			var attrs = type.GetCustomAttributes();
			AbleToBeToggled = attrs.GetAttribute<DefaultEnabledAttribute>().AbleToToggle;
			Aliases = (attrs.GetAttribute<AliasAttribute>()?.Aliases ?? new[] { "N/A" }).ToImmutableArray();
			BasePerms = FormatPreconditions(attrs.OfType<PreconditionAttribute>());
			Category = attrs.GetAttribute<CategoryAttribute>().Category ?? throw new ArgumentException(nameof(CategoryAttribute));
			DefaultEnabled = attrs.GetAttribute<DefaultEnabledAttribute>().Enabled;
			Description = attrs.GetAttribute<SummaryAttribute>().Text ?? throw new ArgumentException(nameof(Description));
			Name = attrs.GetAttribute<GroupAttribute>().Prefix ?? throw new ArgumentException(nameof(Name));
			Usage = UsageGenerator.GenerateUsage(type) ?? throw new ArgumentException(nameof(Usage));
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
			BasePerms = FormatPreconditions(module.Preconditions);
			Category = attrs.GetAttribute<CategoryAttribute>().Category ?? throw new ArgumentException(nameof(CategoryAttribute));
			DefaultEnabled = attrs.GetAttribute<DefaultEnabledAttribute>().Enabled;
			Description = module.Summary ?? throw new ArgumentException(nameof(Description));
			Name = module.Name ?? throw new ArgumentException(nameof(Name));
			Usage = UsageGenerator.GenerateUsage(module) ?? throw new ArgumentException(nameof(Usage));
		}

		private string FormatPreconditions(IEnumerable<PreconditionAttribute> preconditions)
		{
			//Don't let users see preconditions which are designated as not visible (e.g the basic command requirement one)
			preconditions = preconditions.Where(x => !(x is SelfGroupPreconditionAttribute self) || !self.Visible);
			if (!preconditions.Any())
			{
				return "N/A";
			}
			if (preconditions.Any(x => x.Group == null))
			{
				return preconditions.Select(x => x.ToString()).JoinNonNullStrings(" & ");
			}

			var groups = preconditions.GroupBy(x => x.Group).ToArray();
			if (groups.Length == 1)
			{
				return groups[0].Select(x => x.ToString()).JoinNonNullStrings(" | ");
			}

			return groups.Select(g => $"({g.Select(c => c.ToString()).JoinNonNullStrings(" | ")})").JoinNonNullStrings(" & ");
		}
		/// <inheritdoc />
		public override string ToString()
			=> ToString(null);
		/// <inheritdoc />
		public string ToString(CommandSettings settings)
		{
			var str = "";
			str += $"**Aliases:** {string.Join(", ", Aliases)}\n";
			str += $"**Usage:** {Constants.PREFIX}{Name} {Usage}\n";
			str += $"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}{(AbleToBeToggled ? "" : " (Not toggleable)")}\n";
			if (settings != null)
			{
				if (!(settings.IsCommandEnabled(Name) is bool val))
				{
					val = DefaultEnabled;
				}
				str += $"**Currently Enabled:** {(val ? "Yes" : "No")}\n";
			}
			str += $"**Base Permission(s):**\n{BasePerms}\n";
			str += $"**Description:**\n{Description}";
			return str;
		}
	}
}