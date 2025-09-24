using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.TypeReaders;

using Discord;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.MiscCategory))]
public sealed class Misc : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.Get), nameof(Names.GetAlias))]
	[LocalizedSummary(nameof(Summaries.GetInfoSummary))]
	[Meta("99dcd5e7-6bb2-49cf-b8b7-66b8e063fd18", IsEnabled = true)]
	public sealed class Get : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Names.Ban), nameof(Names.BanAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Ban([Remainder] IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[Command]
		[Hidden]
		public Task<AdvobotResult> BanImplicit([Remainder] IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[LocalizedCommand(nameof(Names.Bot), nameof(Names.BotAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Bot()
			=> Responses.Misc.InfoBot(Context.Client);

		[LocalizedCommand(nameof(Names.Channel), nameof(Names.ChangePunishmentAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Channel([Remainder] IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[Command]
		[Hidden]
		public Task<AdvobotResult> ChannelImplicit([Remainder] IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[LocalizedCommand(nameof(Names.Emote), nameof(Names.EmoteAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Emote([Remainder] Emote emote)
			=> Responses.Misc.InfoEmote(emote);

		[Command]
		[Hidden]
		public Task<AdvobotResult> EmoteImplicit([Remainder] Emote emote)
			=> Responses.Misc.InfoEmote(emote);

		[Command]
		[Hidden]
		[Priority(-1)]
		public Task<AdvobotResult> Failure([Remainder] string _)
			=> Responses.Misc.InfoNotFound();

		[LocalizedCommand(nameof(Names.Guild), nameof(Names.GuildAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Guild()
			=> Responses.Misc.InfoGuild(Context.Guild);

		[LocalizedCommand(nameof(Names.Invite), nameof(Names.InviteAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Invite([Remainder] IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[Command]
		[Hidden]
		public Task<AdvobotResult> InviteImplicit([Remainder] IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[LocalizedCommand(nameof(Names.Role), nameof(Names.RoleAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Role([Remainder] IRole role)
			=> Responses.Misc.InfoRole(role);

		[Command]
		[Hidden]
		public Task<AdvobotResult> RoleImplicit([Remainder] IRole role)
			=> Responses.Misc.InfoRole(role);

		[LocalizedCommand(nameof(Names.User), nameof(Names.UserAlias))]
		[Priority(1)]
		public Task<AdvobotResult> User([Remainder] IGuildUser? user = null)
			=> Responses.Misc.InfoUser(user ?? Context.User);

		[Command]
		[Hidden]
		public Task<AdvobotResult> UserImplicit([Remainder] IGuildUser user)
			=> Responses.Misc.InfoUser(user);

		[LocalizedCommand(nameof(Names.Webhook), nameof(Names.WebhookAlias))]
		[Priority(1)]
		public Task<AdvobotResult> Webhook([Remainder] IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);

		[Command]
		[Hidden]
		public Task<AdvobotResult> WebhookImplicit([Remainder] IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);
	}

	[LocalizedCommand(nameof(Names.Help), nameof(Names.HelpAlias))]
	[LocalizedSummary(nameof(Summaries.HelpSummary))]
	[Meta("0e89a6fd-5c9c-4008-a912-7c719ea7827d", IsEnabled = true, CanToggle = false)]
	public sealed class Help : AdvobotModuleBase
	{
		[InjectService]
		public required IGuildSettingsService GuildSettings { get; set; }
		[InjectService]
		public required CommandService HelpEntries { get; set; }

		[Command]
		[Priority(1)]
		public Task<AdvobotResult> Category(
			[LocalizedSummary(nameof(Summaries.HelpVariableCategorySummary))]
			[LocalizedName(nameof(Names.CategoryParameter))]
			[OverrideTypeReader<CommandsCategoryTypeReader>]
			[Remainder]
			IReadOnlyList<IImmutableCommand> commands
		) => Responses.Misc.HelpCategory(commands);

		[Command]
		[LocalizedSummary(nameof(Summaries.HelpGeneralHelpSummary))]
		public async Task<AdvobotResult> General()
		{
			var prefix = await GuildSettings.GetPrefixAsync(Context.Guild).ConfigureAwait(false);
			var categories = HelpEntries.Commands.SelectMany(x => x.Categories).ToHashSet();
			return Responses.Misc.HelpGeneral(categories, prefix);
		}

		[Command]
		[Priority(1)]
		[LocalizedSummary(nameof(Summaries.HelpModuleHelpSummary))]
		public Task<AdvobotResult> Name(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommandSummary))]
			[LocalizedName(nameof(Names.CommandParameter))]
			[OverrideTypeReader<CommandsNameTypeReader>]
			[Remainder]
			IReadOnlyList<IImmutableCommand> commands
		) => Responses.Misc.Help(commands);

		[Command]
		[Priority(2)]
		[LocalizedSummary(nameof(Summaries.HelpCommandHelpSummary))]
		public Task<AdvobotResult> Name(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommandPositionSummary))]
			[LocalizedName(nameof(Names.PositionParameter))]
			[Positive]
			int position,
			[LocalizedSummary(nameof(Summaries.HelpVariableExactCommandSummary))]
			[LocalizedName(nameof(Names.CommandParameter))]
			[OverrideTypeReader<CommandsNameTypeReader>]
			[Remainder]
			IReadOnlyList<IImmutableCommand> commands
		)
		{
			if (commands.Count < position)
			{
				return Responses.Misc.HelpInvalidPosition(commands[0], position);
			}
			return Responses.Misc.Help(commands[position - 1]);
		}

		[Command]
		[Priority(-1)]
		[Hidden]
		public async Task<IResult> Similar(
			[OverrideTypeReader<SimilarCommandsTypeReader>]
			[Remainder]
			IReadOnlyList<SimilarCommands> commands
		)
		{
			var entry = await NextItemAtIndexAsync(commands, x => x.Path).ConfigureAwait(false);
			if (entry.Value?.Commands is IReadOnlyList<IImmutableCommand> list)
			{
				if (list.Count == 1)
				{
					return Responses.Misc.Help(list[0]);
				}
				return Responses.Misc.Help(list);
			}
			return Result.EmptySuccess;
		}
	}

	[LocalizedCommand(nameof(Names.Test), nameof(Names.TestAlias))]
	[LocalizedSummary(nameof(Summaries.TestSummary))]
	[Meta("6c0b693e-e3ac-421e-910e-3178110d791d", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public Task Do()
			=> Task.CompletedTask;
	}
}