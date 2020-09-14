using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses
{
	public sealed class Misc : AdvobotResult
	{
		private static readonly Type _Commands = typeof(Commands.Misc.Commands);
		private static readonly Type _Help = typeof(Commands.Misc.Help);

		private Misc() : base(null, "")
		{
		}

		public static AdvobotResult CategoryCommands(
			IReadOnlyList<IModuleHelpEntry> entries,
			string category)
		{
			var title = MiscTitleCategoryCommands.Format(
				category.WithTitleCase()
			);
			var description = entries
				.Join(x => x.Name)
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
				categories.Join().WithBigBlock()
			);
			return Success(new EmbedWrapper
			{
				Title = MiscTitleCategories,
				Description = description,
			});
		}

		public static AdvobotResult GeneralHelp(string prefix)
		{
			var description = MiscGeneralHelp.Format(
				GetPrefixedCommand(prefix, _Commands),
				GetPrefixedCommand(prefix, _Help, MiscVariableCategoryParameter)
			);
			var syntaxFieldValue = MiscBasicSyntax.Format(
				(MiscVariableRequiredLeft + MiscVariableRequiredRight).WithBlock(),
				(MiscVariableOptionalLeft + MiscVariableOptionalRight).WithBlock(),
				MiscVariableOr.WithBlock()
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
						Value = syntaxFieldValue,
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
			var top = info.CreateCollection();
			top.Add(MiscTitleAliases, module.Aliases.Join());
			top.Add(MiscTitleBasePermissions, FormatPreconditions(module.Preconditions));
			var description = info.CreateCollection();
			description.Add(MiscTitleDescription, module.Summary);
			var meta = info.CreateCollection();
			meta.Add(MiscTitleCurrentlyEnabled, GetEnabledStatus(module, settings));
			meta.Add(MiscTitleEnabledByDefault, module.EnabledByDefault);
			meta.Add(MiscTitleAbleToBeToggled, module.AbleToBeToggled);

			if (module.Commands.Count == 0)
			{
				return Success(CreateHelpEmbed(module.Name, info.ToString()));
			}

			var commands = "\n" + module.Commands.Select((x, i) =>
			{
				//If the name of the command is not in its alias, then the name isnt set
				var name = x.Aliases.Any(a => a.CaseInsContains(x.Name)) ? $" {x.Name}" : "";
				var parameters = x.Parameters.Join(FormatParameter);
				return $"{i + 1}.{name} ({parameters})";
			}).Join("\n").WithBigBlock().Value;
			info.CreateCollection().Add(MiscTitleCommands, commands);

			return Success(CreateHelpEmbed(module.Aliases[0], info.ToString()));
		}

		public static AdvobotResult Help(
			IModuleHelpEntry module,
			int index)
		{
			if (module.Commands.Count <= index)
			{
				return Failure(MiscInvalidHelpEntryNumber.Format(
					index.ToString().WithBlock(),
					module.Name.WithBlock()
				));
			}
			var command = module.Commands[index];

			var info = new InformationMatrix();
			var top = info.CreateCollection();
			top.Add(MiscTitleAliases, command.Aliases.Join());
			top.Add(MiscTitleBasePermissions, FormatPreconditions(command.Preconditions));
			var description = info.CreateCollection();
			description.Add(MiscTitleDescription, command.Summary);

			var embed = CreateHelpEmbed(command.Aliases[0], info.ToString());
			foreach (var parameter in command.Parameters)
			{
				var paramInfo = new InformationMatrix();
				var paramTop = paramInfo.CreateCollection();
				paramTop.Add(MiscTitleBasePermissions, FormatPreconditions(parameter.Preconditions));
				paramTop.Add(MiscTitleDescription, parameter.Summary);
				paramTop.Add(MiscTitleNamedArguments, parameter.NamedArguments.Join());

				embed.TryAddField(FormatParameter(parameter), paramInfo.ToString(), true, out _);
			}
			return Success(embed);
		}

		public static AdvobotResult MakeAnEmbed(CustomEmbed embed)
			=> Success(embed.BuildWrapper());

		public static AdvobotResult Remind(TimeSpan time)
		{
			return Success(MiscRemind.Format(
				time.ToString("00:00:00").WithBlock()
			));
		}

		private static EmbedWrapper CreateHelpEmbed(string name, string entry)
		{
			return new EmbedWrapper
			{
				Title = name,
				Description = entry,
				Footer = new EmbedFooterBuilder { Text = MiscFooterHelp, },
			};
		}

		private static string FormatParameter(IParameterHelpEntry p)
		{
			var left = p.IsOptional ? MiscVariableOptionalLeft : MiscVariableRequiredLeft;
			var right = p.IsOptional ? MiscVariableOptionalRight : MiscVariableRequiredRight;
			return left + p.Name + right;
		}

		private static string FormatPreconditions(IEnumerable<IPrecondition> preconditions)
		{
			if (!preconditions.Any())
			{
				return MiscVariableNotApplicable;
			}
			if (preconditions.Any(x => x.Group == null))
			{
				return preconditions.Join(x => x.Summary, MiscVariableAnd);
			}

			var groups = preconditions
				.GroupBy(x => x.Group)
				.Select(g => g.Join(x => x.Summary, MiscVariableOr))
				.ToArray();
			if (groups.Length == 1)
			{
				return groups[0];
			}
			return groups.Join(g => $"({g})", MiscVariableAnd);
		}

		private static string FormatPreconditions(IEnumerable<IParameterPrecondition> preconditions)
		{
			if (!preconditions.Any())
			{
				return MiscVariableNotApplicable;
			}
			return preconditions.Join(x => x.Summary, MiscVariableAnd);
		}

		private static bool GetEnabledStatus(IModuleHelpEntry entry, IGuildSettings settings)
			=> settings?.CommandSettings?.IsCommandEnabled(entry.Id) ?? entry.EnabledByDefault;

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
	}
}