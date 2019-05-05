using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;

namespace Advobot.Commands.Responses
{
	public sealed class Misc : CommandResponses
	{
		private static readonly string _GeneralHelp =
			$"Type `{Constants.PREFIX}{nameof(Commands.Misc.Misc.Commands)}` for the list of commands.\n" +
			$"Type `{Constants.PREFIX}{nameof(Commands.Misc.Misc.Help)} [Command]` for help with a command.";
		private static readonly string _BasicSyntax =
			"`[]` means required.\n" +
			"`<>` means optional.\n" +
			"`|` means or.";
		private static readonly string _MentionSyntax =
			"`User` means `@User|\"Username\"`.\n" +
			"`Role` means `@Role|\"Role Name\"`.\n" +
			"`Channel` means `#Channel|\"Channel Name\"`.";
		private static readonly string _Links =
			$"[GitHub Repository]({Constants.REPO})\n" +
			$"[Discord Server]({Constants.DISCORD_INV})";

		private Misc() { }

		public static AdvobotResult GeneralHelp(string prefix)
		{
			return Success(new EmbedWrapper
			{
				Title = "General Help",
				Description = _GeneralHelp.Replace(Constants.PREFIX, prefix),
				Footer = new EmbedFooterBuilder { Text = "Help" },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder { Name = "Basic Syntax", Value = _BasicSyntax, IsInline = true, },
					new EmbedFieldBuilder { Name = "Mention Syntax", Value = _MentionSyntax, IsInline = true, },
					new EmbedFieldBuilder { Name = "Links", Value = _Links, IsInline = false, },
				},
			});
		}
		public static AdvobotResult Help(CommandSettings settings, IHelpEntry entry, string prefix)
		{
			return Success(new EmbedWrapper
			{
				Title = entry.Name,
				Description = entry.ToString(settings).Replace(Constants.PREFIX, prefix),
				Footer = new EmbedFooterBuilder { Text = "Help", },
			});
		}
		public static AdvobotResult AllCommands(IReadOnlyCollection<IHelpEntry> entries)
		{
			return Success(new EmbedWrapper
			{
				Title = "All Commands",
				Description = Default.FormatInterpolated($"{entries.Select(x => x.Name)}"),
			});
		}
		public static AdvobotResult CategoryCommands(IReadOnlyCollection<IHelpEntry> entries, string category)
		{
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"{category}"),
				Description = Default.FormatInterpolated($"{entries.Select(x => x.Name)}"),
			});
		}
		public static AdvobotResult GeneralCommandInfo(IReadOnlyCollection<string> categories, string prefix)
		{
			return Success(new EmbedWrapper
			{
				Title = "Categories",
				Description = Default.FormatInterpolated($"Type {prefix}{nameof(Commands)} [Category] for commands from a category.\n\n{categories}"),
			});
		}
		public static AdvobotResult MakeAnEmbed(CustomEmbed embed)
			=> Success(embed.BuildWrapper());
		public static AdvobotResult Remind(TimeSpan time)
			=> Success(Default.FormatInterpolated($"Successfully added a reminder which will trigger in {time:00:00:00}."));
	}
}
