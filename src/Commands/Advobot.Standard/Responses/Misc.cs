using System;
using System.Collections.Generic;
using System.Reflection;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
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
		public static AdvobotResult Help(IHelpEntry entry, IGuildSettings settings)
			=> Help(entry.Name, entry.ToString(settings, Markdown));
		public static AdvobotResult Help(IHelpEntry entry, IGuildSettings settings, int index)
			=> Help(entry.Name, entry.ToString(settings, Markdown, index));
		public static AdvobotResult AllCommands(IReadOnlyList<IHelpEntry> entries)
		{
			var description = entries
				.ToDelimitedString(x => x.Name)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = MiscTitleAllCommands,
				Description = description,
			});
		}
		public static AdvobotResult CategoryCommands(
			IReadOnlyList<IHelpEntry> entries,
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
		private static AdvobotResult Help(string name, string entry)
		{
			return Success(new EmbedWrapper
			{
				Title = name,
				Description = entry,
				Footer = new EmbedFooterBuilder { Text = MiscFooterHelp, },
			});
		}
	}
}
