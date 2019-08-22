using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Advobot.Standard.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses
{
	public sealed class Misc : CommandResponses
	{
		private static readonly Type _Commands = typeof(Commands.Misc.Commands);
		private static readonly Type _Help = typeof(Commands.Misc.Help);

		private Misc() { }

		public static AdvobotResult GeneralHelp(string prefix)
		{
			var description = MiscGeneralHelp.Format(
				GetPrefixedCommand(prefix, _Commands),
				GetPrefixedCommand(prefix, _Help, MiscVariableCategoryParameter)
			);
			return Success(new EmbedWrapper
			{
				Title = MiscTitleGeneralHelp,
				Description = description,
				Footer = new EmbedFooterBuilder { Text = MiscFooterHelp },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder
					{
						Name = MiscTitleBasicSyntax,
						Value = MiscBasicSyntax,
						IsInline = true,
					},
					new EmbedFieldBuilder
					{
						Name = MiscTitleMentionSyntax,
						Value = MiscMentionSyntax,
						IsInline = true,
					},
					new EmbedFieldBuilder
					{
						Name = MiscTitleLinks,
						Value = MiscLinks.Format(
							Constants.REPO.WithNoMarkdown(),
							Constants.DISCORD_INV.WithNoMarkdown()
						),
						IsInline = false,
					},
				},
			});
		}
		public static AdvobotResult Help(
			IModuleHelpEntry module,
			IGuildSettings settings)
		{
			var info = new InformationMatrix();
			var aliases = info.CreateCollection();
			aliases.Add("Aliases", module.Aliases.ToDelimitedString());
			aliases.Add("Base Permissions", FormatPreconditions(module.Preconditions));
			var description = info.CreateCollection();
			description.Add("Description", module.Summary);
			var meta = info.CreateCollection();
			meta.Add("Currently Enabled", GetEnabledStatus(module, settings));
			meta.Add("Enabled By Default", module.EnabledByDefault);
			meta.Add("Able To Be Toggled", module.AbleToBeToggled);

			if (!module.Commands.Any())
			{
				return Success(CreateHelpEmbed(module.Name, info.ToString()));
			}

			var commands = "\n" + module.Commands.Select((x, i) =>
			{
				//If the name of the command is not in its alias, then the name isnt set
				var name = x.Aliases.Any(a => a.CaseInsContains(x.Name)) ? $" {x.Name}" : "";
				var parameters = x.Parameters.ToDelimitedString(FormatParameter);
				return $"\t{i + 1}.{name} ({parameters})";
			}).ToDelimitedString("\n").WithBigBlock().Value;
			info.CreateCollection().Add("Commands", commands);

			return Success(CreateHelpEmbed(module.Name, info.ToString()));
		}
		public static AdvobotResult Help(
			IModuleHelpEntry module,
			int index)
		{
			var command = module.Commands[index];

			var info = new InformationMatrix();
			var aliases = info.CreateCollection();
			aliases.Add("Aliases", command.Aliases.ToDelimitedString());
			aliases.Add("Base Permissions", FormatPreconditions(command.Preconditions));

			if (!command.Parameters.Any())
			{
				return Success(CreateHelpEmbed(module.Name, info.ToString()));
			}

			var parameters = "\n" + command.Parameters.Select((x, i) =>
			{
				var name = FormatParameter(x);
				var summary = x.Summary != null ? $"\n{x.Summary}" : "";
				return $"\t{i + 1}. {name}{summary}";
			}).ToDelimitedString("\n").WithBigBlock().Value;
			info.CreateCollection().Add("Parameters", parameters);

			return Success(CreateHelpEmbed(module.Name, info.ToString()));
		}
		public static AdvobotResult CategoryCommands(
			IReadOnlyList<IModuleHelpEntry> entries,
			string category)
		{
			var title = MiscTitleCategoryCommands.Format(
				category.WithTitleCase()
			);
			var description = entries
				.ToDelimitedString(x => x.Name)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = description,
			});
		}
		public static AdvobotResult GeneralCommandInfo(
			IReadOnlyList<string> categories,
			string prefix)
		{
			var description = MiscGeneralCommandInfo.Format(
				GetPrefixedCommand(prefix, _Commands, MiscVariableCategoryParameter),
				categories.ToDelimitedString().WithBigBlock()
			);
			return Success(new EmbedWrapper
			{
				Title = MiscTitleCategories,
				Description = description,
			});
		}
		public static AdvobotResult MakeAnEmbed(CustomEmbed embed)
			=> Success(embed.BuildWrapper());
		public static AdvobotResult Remind(TimeSpan time)
		{
			return Success(MiscRemind.Format(
				time.ToString("00:00:00").WithBlock()
			));
		}

		private static MarkdownFormattedArg GetPrefixedCommand(
			string prefix,
			Type command,
			string args = "")
		{
			var attr = command.GetCustomAttribute<GroupAttribute>();
			if (attr == null)
			{
				throw new ArgumentException(nameof(command));
			}

			return $"{prefix}{attr.Prefix} {args}".WithBlock();
		}
		private static string FormatPreconditions(IEnumerable<IPrecondition> preconditions)
		{
			if (!preconditions.Any())
			{
				return "N/A";
			}
			if (preconditions.Any(x => x.Group == null))
			{
				return preconditions.ToDelimitedString(x => x.ToString(), " & ");
			}

			var groups = preconditions
				.GroupBy(x => x.Group)
				.Select(g => g.ToDelimitedString(x => x.ToString(), " | "))
				.ToArray();
			if (groups.Length == 1)
			{
				return groups[0];
			}

			return groups.ToDelimitedString(g => $"({g})", " & ");
		}
		private static string FormatParameter(IParameterHelpEntry p)
			=> $"{p.TypeName}: {p.Name}";
		private static bool GetEnabledStatus(IModuleHelpEntry entry, IGuildSettings settings)
			=> settings?.CommandSettings?.IsCommandEnabled(entry.Id) ?? entry.EnabledByDefault;
		private static EmbedWrapper CreateHelpEmbed(string name, string entry)
		{
			return new EmbedWrapper
			{
				Title = name,
				Description = entry,
				Footer = new EmbedFooterBuilder { Text = MiscFooterHelp, },
			};
		}
	}
}
