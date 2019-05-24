using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Formatting;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal readonly struct HelpEntryV2 : IHelpEntry
	{
		public bool AbleToBeToggled { get; }
		public bool DefaultEnabled { get; }
		public string? Category { get; }
		public string Description { get; }
		public string Name { get; }
		public IReadOnlyCollection<string> Aliases { get; }
		public IReadOnlyCollection<PreconditionAttribute> BasePerms { get; }

		private readonly ModuleInfo _Module;

		public HelpEntryV2(ModuleInfo module)
		{
			var enabledByDefaultAttr = module.Attributes.GetAttribute<EnabledByDefaultAttribute>();
			AbleToBeToggled = enabledByDefaultAttr.AbleToToggle;
			DefaultEnabled = enabledByDefaultAttr.Enabled;

			static ModuleInfo GetParentModule(ModuleInfo m)
				=> m.Parent == null ? m : GetParentModule(m.Parent);

			var parent = GetParentModule(module);
			Category = parent == module ? null : parent.Name;
			Description = module.Summary ?? throw new ArgumentException(nameof(Description));
			Name = module.Name ?? throw new ArgumentException(nameof(Name));

			Aliases = module.Aliases.Any() ? module.Aliases : new[] { "N/A" }.ToImmutableArray();
			BasePerms = module.Preconditions;

			_Module = module;
		}

		private static string FormatPreconditions(IEnumerable<PreconditionAttribute> preconditions)
		{
			//Don't let users see preconditions which are designated as not visible (e.g the basic command requirement one)
			preconditions = preconditions.Where(x => x is AdvobotPreconditionAttribute temp && temp.Visible);
			if (!preconditions.Any())
			{
				return "N/A";
			}
			if (preconditions.Any(x => x.Group == null))
			{
				return preconditions.JoinNonNullValues(" & ", x => x.ToString());
			}

			var groups = preconditions
				.GroupBy(x => x.Group)
				.Select(g => g.JoinNonNullValues(" | ", x => x.ToString()))
				.ToArray();
			if (groups.Length == 1)
			{
				return groups[0];
			}

			return groups.JoinNonNullValues(" & ", g => $"({g})");
		}
		private string GetEnabledStatus(IGuildSettings? settings)
			=> settings?.CommandSettings?.IsCommandEnabled(Name) ?? DefaultEnabled ? "Yes" : "No";

		public string ToString(IFormatProvider? formatProvider)
			=> ToString(null, formatProvider);
		public string ToString(IGuildSettings? settings, IFormatProvider? formatProvider)
		{
			var collection = new DiscordFormattableStringCollection
			{
				$"{"Aliases".AsTitle()} {Aliases.Join(", ")}\n",
				$"{"Currently Enabled".AsTitle()} {GetEnabledStatus(settings)}\n",
				$"{"Enabled By Default".AsTitle()} {(DefaultEnabled ? "Yes" : "No")}{(AbleToBeToggled ? "" : " (Not toggleable)")}\n\n",
				$"{"Base Permission(s)".AsTitle()}\n{FormatPreconditions(BasePerms)}\n\n",
				$"{"Description".AsTitle()}\n{Description}\n",
			};

			var formattedCommands = _Module.Commands.Select((x, i) =>
			{
				var parameters = x.Parameters.Select(x => $"{x.Type.Name}: {x.Name}").Join(", ");
				return $"\t{i + 1}. {x.Name} ({parameters})";
			}).Join("\n");
			collection.Add($"{"Commands".AsTitle()} {formattedCommands:```}");
			return collection.ToString(formatProvider);
		}
		public string ToString(int commandIndex, IGuildSettings? settings, IFormatProvider? formatProvider)
		{
			var command = _Module.Commands[commandIndex];
			throw new NotImplementedException();
		}

		string IFormattable.ToString(string format, IFormatProvider formatProvider) => ToString(formatProvider);
	}

	/*
	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	internal sealed class HelpEntry : IHelpEntry
	{
		/// <inheritdoc />
		public bool AbleToBeToggled { get; }
		/// <inheritdoc />
		public IReadOnlyCollection<string> Aliases { get; }
		/// <inheritdoc />
		public string BasePerms { get; }
		/// <inheritdoc />
		public string? Category { get; }
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
			static Type GetParentType(Type t)
				=> t.DeclaringType == null ? t : GetParentType(t.DeclaringType);

			var attrs = type.GetCustomAttributes();
			var parent = GetParentType(type);
			AbleToBeToggled = attrs.GetAttribute<EnabledByDefaultAttribute>().AbleToToggle;
			Aliases = (attrs.GetAttribute<AliasAttribute>()?.Aliases ?? new[] { "N/A" }).ToImmutableArray();
			BasePerms = FormatPreconditions(attrs.OfType<PreconditionAttribute>());
			Category = parent == type ? "" : parent.Name;
			DefaultEnabled = attrs.GetAttribute<EnabledByDefaultAttribute>().Enabled;
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
			static ModuleInfo GetParentModule(ModuleInfo m)
				=> m.Parent == null ? m : GetParentModule(m.Parent);

			var attrs = module.Attributes;
			var parent = GetParentModule(module);
			AbleToBeToggled = attrs.GetAttribute<EnabledByDefaultAttribute>().AbleToToggle;
			Aliases = (module.Aliases.Any() ? module.Aliases : new[] { "N/A" }).ToImmutableArray();
			BasePerms = FormatPreconditions(module.Preconditions);
			Category = parent == module ? "" : parent.Name;
			DefaultEnabled = attrs.GetAttribute<EnabledByDefaultAttribute>().Enabled;
			Description = module.Summary ?? throw new ArgumentException(nameof(Description));
			Name = module.Name ?? throw new ArgumentException(nameof(Name));
			Usage = UsageGenerator.GenerateUsage(module) ?? throw new ArgumentException(nameof(Usage));
		}

		private string FormatPreconditions(IEnumerable<PreconditionAttribute> preconditions)
		{
			//Don't let users see preconditions which are designated as not visible (e.g the basic command requirement one)
			preconditions = preconditions.Where(x => x is AdvobotPreconditionAttribute temp && temp.Visible);
			if (!preconditions.Any())
			{
				return "N/A";
			}
			if (preconditions.Any(x => x.Group == null))
			{
				return preconditions.JoinNonNullValues(" & ", x => x.ToString());
			}

			var groups = preconditions
				.GroupBy(x => x.Group)
				.Select(g => g.JoinNonNullValues(" | ", x => x.ToString()))
				.ToArray();
			if (groups.Length == 1)
			{
				return groups[0];
			}

			return groups.JoinNonNullValues(" & ", g => $"({g})");
		}
		/// <inheritdoc />
		public override string ToString()
			=> ToString(null);
		/// <inheritdoc />
		public string ToString(CommandSettings? settings)
		{
			var str = "";
			str += $"**Aliases:** {Aliases.Join(", ")}\n";
			if (settings != null)
			{
				var val = settings.IsCommandEnabled(Name) ?? DefaultEnabled;
				str += $"**Currently Enabled:** {(val ? "Yes" : "No")}\n";
			}
			str += $"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}{(AbleToBeToggled ? "" : " (Not toggleable)")}\n\n";
			str += $"**Base Permission(s):**\n{BasePerms}\n\n";
			str += $"**Description:**\n{Description}\n";
			str += $"**Usage:** {Constants.PREFIX}{Name} {Usage}\n";
			return str;
		}
	}*/
}