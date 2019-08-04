using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Attributes;
using Advobot.Formatting;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal readonly struct HelpEntry : IHelpEntry
	{
		public bool AbleToBeToggled { get; }
		public bool DefaultEnabled { get; }
		public string? Category { get; }
		public string Description { get; }
		public string Name { get; }
		public IReadOnlyCollection<string> Aliases { get; }
		public IReadOnlyCollection<PreconditionAttribute> BasePerms { get; }

		private readonly ModuleInfo _Module;

		public HelpEntry(ModuleInfo module)
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
		private static string FormatType(Type type)
		{
			if (!type.IsGenericType)
			{
				return type.Name;
			}

			var name = type.Name.Split('`')[0];
			return $"{name}<{type.GetGenericArguments().Join(", ", x => FormatType(x))}>";
		}
		private static string FormatParameter(ParameterInfo p)
			=> $"{FormatType(p.Type)}: {p.Name}";
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
				$"{"Description".AsTitle()}\n{Description}\n\n",
			};

			var formattedCommands = _Module.Commands.Select((c, i) =>
			{
				var output = $"\t{i + 1}.";
				//If the name of the command is not in its alias, then the name isnt set
				if (c.Aliases.Any(a => a.CaseInsContains(c.Name)))
				{
					output += $" {c.Name}";
				}
				var parameters = c.Parameters
					.Where(p => !p.Attributes.Any(a => a is HiddenAttribute))
					.Select(FormatParameter)
					.Join(", ");
				return output + $" ({parameters})";
			}).Join("\n");
			collection.Add($"{"Commands".AsTitle()} {formattedCommands:```}");
			return collection.ToString(formatProvider);
		}
		public string ToString(IGuildSettings? settings, IFormatProvider? formatProvider, int commandIndex)
		{
			var command = _Module.Commands[commandIndex];

			var collection = new DiscordFormattableStringCollection
			{
				$"{"Aliases".AsTitle()} {command.Aliases.Join(", ")}\n\n",
				$"{"Base Permission(s)".AsTitle()}\nParent preconditions + {FormatPreconditions(command.Preconditions)}\n\n",
				$"{"Description".AsTitle()}\n{command.Summary}\n\n",
			};

			if (!command.Parameters.Any())
			{
				return collection.ToString(formatProvider);
			}

			var visibleParameters = command.Parameters.Where(p => !p.Attributes.Any(a => a is HiddenAttribute));
			var formattedParameters = visibleParameters.Select((p, i) =>
			{
				var output = $"\t{i + 1}. {FormatParameter(p)}";
				if (p.Summary != null)
				{
					output += $"\n{p.Summary}";
				}
				return output;
			}).Join("\n");
			collection.Add($"{"Parameters".AsTitle()} {formattedParameters:```}");
			return collection.ToString(formatProvider);
		}

		string IFormattable.ToString(string format, IFormatProvider formatProvider)
			=> ToString(formatProvider);
	}
}