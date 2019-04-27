using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
	public sealed class Channels : CommandResponses
	{
		private Channels() { }

		public static AdvobotResult Created(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_Created, channel));
		public static AdvobotResult SoftDeleted(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_SoftDeleted, channel));
		public static AdvobotResult Deleted(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_Deleted, channel));
		public static AdvobotResult Positions(IEnumerable<IGuildChannel> channels, [CallerMemberName] string caller = "")
		{
			var f = channels.Select(x => Default.FormatInterpolated($"{x.Position.ToString("00")}. {x.Name}"));
			return Success(new EmbedWrapper
			{
				Title = Title.Format(strings.Responses_Channels_Positions_Title, caller),
				Description = f.Join("\n"),
			});
		}
		public static AdvobotResult Moved(IGuildChannel channel, int position)
			=> Success(Default.Format(strings.Responses_Channels_Moved, channel, position));
		public static AdvobotResult AllOverwrites(IGuildChannel channel, IEnumerable<string> roleNames, IEnumerable<string> userNames)
		{
			var embed = new EmbedWrapper { Title = Title.Format(strings.Responses_Channels_AllOverwrites_Title, channel), };
			embed.TryAddField("Roles", Default.FormatInterpolated($"{roleNames}"), false, out _);
			embed.TryAddField("Users", Default.FormatInterpolated($"{userNames}"), false, out _);
			return Success(embed);
		}
		public static AdvobotResult NoOverwriteFound(IGuildChannel channel, ISnowflakeEntity obj)
			=> Success(Default.FormatInterpolated($"No overwrite exists for {obj} on {channel}."));
		public static AdvobotResult Overwrite(IGuildChannel channel, ISnowflakeEntity obj, IEnumerable<(string Name, string Value)> values)
		{
			var padLen = values.Max(x => x.Name.Length);
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Overwrite On {channel}"),
				Description = Default.FormatInterpolated($"{obj}\n") + BigBlock.FormatInterpolated($"{values.Join("\n", x => $"{x.Name.PadRight(padLen)} {x.Value}")}"),
			});
		}
		public static AdvobotResult ModifyPerms(IGuildChannel channel, ISnowflakeEntity obj, ChannelPermission permissions, PermValue action)
			=> Success(Default.FormatInterpolated($"Successfully {GetAction(action)} {EnumUtils.GetFlagNames(permissions)} for {obj} on {channel}."));
	}

	public sealed class ModifyCommands : CommandResponses
	{
		private ModifyCommands() { }

		public static AdvobotResult CannotBeEdited(string command)
			=> Failure($"{command} cannot be edited.");
		public static AdvobotResult Unmodified(string command, bool value)
			=> Failure($"{command} is already {GetEnabled(value)}.");
		public static AdvobotResult InvalidCategory(string category)
			=> Failure($"{category} is not a valid category.");
		public static AdvobotResult Modified(string command, bool value)
			=> Success($"Successfully {GetEnabled(value)} {command}.").WithTime(CreateTime(5));
		public static AdvobotResult ModifiedMultiple(IEnumerable<string> commands, bool value)
			=> Success($"Successfully {GetEnabled(value)} the following: {commands}.").WithTime(CreateTime(5));
	}

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
	}

	/*
	public partial class ResponsesOf
	{
		public sealed class ReadOnlyAdvobotSettingsModuleBase : CommandResponses
		{
			public static ReadOnlyAdvobotSettingsModuleBase For(AdvobotCommandContext context)
				=> new ReadOnlyAdvobotSettingsModuleBase { Context = context, };
			public AdvobotResult Names(string settingName, IEnumerable<string> settings)
			{
				return Success().WithEmbed(new EmbedWrapper
				{
					Title = settingName,
					Description = settings,
				});
			}
		}
	}*/
}
