using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Preconditions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Misc))]
public sealed class Misc : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.Get), nameof(Aliases.Get))]
	[LocalizedSummary(nameof(Summaries.GetInfo))]
	[Id("99dcd5e7-6bb2-49cf-b8b7-66b8e063fd18")]
	[Meta(IsEnabled = true)]
	public sealed class Get : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Ban), nameof(Aliases.Ban))]
		[Priority(1)]
		public Task<AdvobotResult> Ban([Remainder] IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[Command]
		[Hidden]
		public Task<AdvobotResult> BanImplicit([Remainder] IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[LocalizedCommand(nameof(Groups.Bot), nameof(Aliases.Bot))]
		[Priority(1)]
		public Task<AdvobotResult> Bot()
			=> Responses.Misc.InfoBot(Context.Client);

		[LocalizedCommand(nameof(Groups.Channel), nameof(Aliases.ChangePunishment))]
		[Priority(1)]
		public Task<AdvobotResult> Channel([Remainder] IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[Command]
		[Hidden]
		public Task<AdvobotResult> ChannelImplicit([Remainder] IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[LocalizedCommand(nameof(Groups.Emote), nameof(Aliases.Emote))]
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
		public Task<AdvobotResult> FailureImplicit([Remainder] string _)
			=> Responses.Misc.InfoNotFound();

		[LocalizedCommand(nameof(Groups.Guild), nameof(Aliases.Guild))]
		[Priority(1)]
		public Task<AdvobotResult> Guild()
			=> Responses.Misc.InfoGuild(Context.Guild);

		[LocalizedCommand(nameof(Groups.Invite), nameof(Aliases.Invite))]
		[Priority(1)]
		public Task<AdvobotResult> Invite([Remainder] IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[Command]
		[Hidden]
		public Task<AdvobotResult> InviteImplicit([Remainder] IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[LocalizedCommand(nameof(Groups.Role), nameof(Aliases.Role))]
		[Priority(1)]
		public Task<AdvobotResult> Role([Remainder] IRole role)
			=> Responses.Misc.InfoRole(role);

		[Command]
		[Hidden]
		public Task<AdvobotResult> RoleImplicit([Remainder] IRole role)
			=> Responses.Misc.InfoRole(role);

		[LocalizedCommand(nameof(Groups.User), nameof(Aliases.User))]
		[Priority(1)]
		public Task<AdvobotResult> User([Remainder] IGuildUser? user = null)
			=> Responses.Misc.InfoUser(user ?? Context.User);

		[Command]
		[Hidden]
		public Task<AdvobotResult> UserImplicit([Remainder] IGuildUser user)
			=> Responses.Misc.InfoUser(user);

		[LocalizedCommand(nameof(Groups.Webhook), nameof(Aliases.Webhook))]
		[Priority(1)]
		public Task<AdvobotResult> Webhook([Remainder] IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);

		[Command]
		[Hidden]
		public Task<AdvobotResult> WebhookImplicit([Remainder] IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);
	}

	/*
	[LocalizedCommand(nameof(Groups.Help), nameof(Aliases.Help))]
	[LocalizedSummary(nameof(Summaries.Help))]
	[Id("0e89a6fd-5c9c-4008-a912-7c719ea7827d")]
	[Meta(IsEnabled = true, CanToggle = false)]
	public sealed class Help : AdvobotModuleBase
	{
		public required IGuildSettingsService GuildSettings { get; set; }
		public required IHelpService HelpEntries { get; set; }

		[Command]
		[LocalizedSummary(nameof(Summaries.HelpGeneralHelp))]
		public async Task<AdvobotResult> Command()
		{
			var prefix = await GuildSettings.GetPrefixAsync(Context.Guild).ConfigureAwait(false);
			var categories = HelpEntries.GetCategories();
			return Responses.Misc.Help(categories, prefix);
		}

		[Command]
		[Priority(1)]
		[LocalizedSummary(nameof(Summaries.HelpModuleHelp))]
		public Task<AdvobotResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IImmutableCommand module
		) => Responses.Misc.Help(module);

		[Command]
		[Priority(1)]
		public Task<AdvobotResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCategory))]
			[LocalizedName(nameof(Parameters.Category))]
			Category category
		)
		{
			var entries = HelpEntries
				.GetHelpModules(includeSubmodules: false)
				.Where(x => x.Category.CaseInsEquals(category.Name));
			return Responses.Misc.Help(entries, category.Name);
		}

		[Command]
		[Priority(2)]
		[LocalizedSummary(nameof(Summaries.HelpCommandHelp))]
		public Task<AdvobotResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommandPosition))]
			[LocalizedName(nameof(Parameters.Position))]
			[Positive]
			int position,
			[LocalizedSummary(nameof(Summaries.HelpVariableExactCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IImmutableCommand module
		)
		{
			if (module.Commands.Count < position)
			{
				return Responses.Misc.HelpInvalidPosition(module, position);
			}
			return Responses.Misc.Help(module.Commands[position - 1]);
		}

		[Command]
		[Priority(0)]
		[Hidden]
		public async Task<AdvobotResult> Command(
			[CommandsNameTypeReader]
			[Remainder]
			IReadOnlyList<IImmutableCommand> modules
		)
		{
			var entry = await NextItemAtIndexAsync(modules, x => x.Paths[0].Join(" ")).ConfigureAwait(false);
			if (entry.HasValue)
			{
				return Responses.Misc.Help(entry.Value);
			}
			return AdvobotResult.IgnoreFailure;
		}
	}*/

	[LocalizedCommand(nameof(Groups.Test), nameof(Aliases.Test))]
	[LocalizedSummary(nameof(Summaries.Test))]
	[Id("6c0b693e-e3ac-421e-910e-3178110d791d")]
	[Meta(IsEnabled = true)]
	[RequireBotOwner]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public Task Do()
			=> Task.CompletedTask;
	}
}