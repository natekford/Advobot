using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Standard.Commands;

[Category(nameof(Misc))]
public sealed class Misc : ModuleBase
{
	[LocalizedGroup(nameof(Groups.GetInfo))]
	[LocalizedAlias(nameof(Aliases.GetInfo))]
	[LocalizedSummary(nameof(Summaries.GetInfo))]
	[Meta("99dcd5e7-6bb2-49cf-b8b7-66b8e063fd18", IsEnabled = true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Ban))]
		[LocalizedAlias(nameof(Aliases.Ban))]
		[Priority(1)]
		public Task<RuntimeResult> Ban(IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[Command]
		public Task<RuntimeResult> BanImplicit(IBan ban)
			=> Responses.Misc.InfoBan(ban);

		[LocalizedCommand(nameof(Groups.Bot))]
		[LocalizedAlias(nameof(Aliases.Bot))]
		[Priority(1)]
		public Task<RuntimeResult> Bot()
			=> Responses.Misc.InfoBot(Context.Client);

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		[Priority(1)]
		public Task<RuntimeResult> Channel(IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[Command]
		public Task<RuntimeResult> ChannelImplicit(IGuildChannel channel)
			=> Responses.Misc.InfoChannel(channel);

		[LocalizedCommand(nameof(Groups.Emote))]
		[LocalizedAlias(nameof(Aliases.Emote))]
		[Priority(1)]
		public Task<RuntimeResult> Emote(Emote emote)
			=> Responses.Misc.InfoEmote(emote);

		[Command]
		public Task<RuntimeResult> EmoteImplicit(Emote emote)
			=> Responses.Misc.InfoEmote(emote);

		[LocalizedCommand(nameof(Groups.Guild))]
		[LocalizedAlias(nameof(Aliases.Guild))]
		[Priority(1)]
		public Task<RuntimeResult> Guild()
			=> Responses.Misc.InfoGuild(Context.Guild);

		[LocalizedCommand(nameof(Groups.GuildUsers))]
		[LocalizedAlias(nameof(Aliases.GuildUsers))]
		[Priority(1)]
		public Task<RuntimeResult> GuildUsers()
			=> Responses.Misc.InfoGuildUsers(Context.Guild);

		[LocalizedCommand(nameof(Groups.Invite))]
		[LocalizedAlias(nameof(Aliases.Invite))]
		[Priority(1)]
		public Task<RuntimeResult> Invite(IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[Command]
		public Task<RuntimeResult> InviteImplicit(IInviteMetadata invite)
			=> Responses.Misc.InfoInvite(invite);

		[LocalizedCommand(nameof(Groups.Role))]
		[LocalizedAlias(nameof(Aliases.Role))]
		[Priority(1)]
		public Task<RuntimeResult> Role(IRole role)
			=> Responses.Misc.InfoRole(role);

		[Command]
		public Task<RuntimeResult> RoleImplicit(IRole role)
			=> Responses.Misc.InfoRole(role);

		[LocalizedCommand(nameof(Groups.Shards))]
		[LocalizedAlias(nameof(Aliases.Shards))]
		[Priority(1)]
		public Task<RuntimeResult> Shards()
		{
			if (Context.Client is DiscordShardedClient shardedClient)
			{
				return Responses.Misc.InfoShards(shardedClient);
			}
			return Responses.Misc.InfoBot(Context.Client);
		}

		[LocalizedCommand(nameof(Groups.User))]
		[LocalizedAlias(nameof(Aliases.User))]
		[Priority(1)]
		public Task<RuntimeResult> User(IUser? user = null)
			=> Responses.Misc.InfoUser(user ?? Context.User);

		[Command]
		public Task<RuntimeResult> UserImplicit(IUser user)
			=> Responses.Misc.InfoUser(user);

		[LocalizedCommand(nameof(Groups.Webhook))]
		[LocalizedAlias(nameof(Aliases.Webhook))]
		[Priority(1)]
		public Task<RuntimeResult> Webhook(IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);

		[Command]
		public Task<RuntimeResult> WebhookImplicit(IWebhook webhook)
			=> Responses.Misc.InfoWebhook(webhook);
	}

	[LocalizedGroup(nameof(Groups.GetUserAvatar))]
	[LocalizedAlias(nameof(Aliases.GetUserAvatar))]
	[LocalizedSummary(nameof(Summaries.GetUserAvatar))]
	[Meta("9978b327-b1bb-46af-9e9f-b43ff411902d", IsEnabled = true)]
	public sealed class GetUserAvatar : AdvobotModuleBase
	{
		[Command]
		public Task Command(IUser? user = null)
			=> Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl());
	}

	[LocalizedGroup(nameof(Groups.Help))]
	[LocalizedAlias(nameof(Aliases.Help))]
	[LocalizedSummary(nameof(Summaries.Help))]
	[Meta("0e89a6fd-5c9c-4008-a912-7c719ea7827d", IsEnabled = true, CanToggle = false)]
	public sealed class Help : AdvobotModuleBase
	{
		public required IGuildSettingsService GuildSettings { get; set; }
		public required IHelpService HelpEntries { get; set; }

		[Command]
		[LocalizedSummary(nameof(Summaries.HelpGeneralHelp))]
		public async Task<RuntimeResult> Command()
		{
			var prefix = await GuildSettings.GetPrefixAsync(Context.Guild).ConfigureAwait(false);
			var categories = HelpEntries.GetCategories();
			return Responses.Misc.Help(categories, prefix);
		}

		[Command]
		[Priority(1)]
		[LocalizedSummary(nameof(Summaries.HelpModuleHelp))]
		public Task<RuntimeResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IHelpModule module
		) => Responses.Misc.Help(module);

		[Command]
		[Priority(1)]
		public Task<RuntimeResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCategory))]
			[LocalizedName(nameof(Parameters.Category))]
			Category category
		)
		{
			var entries = HelpEntries.GetHelpModules(category.Name);
			return Responses.Misc.Help(entries, category.Name);
		}

		[Command]
		[Priority(2)]
		[LocalizedSummary(nameof(Summaries.HelpCommandHelp))]
		public Task<RuntimeResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommandPosition))]
			[LocalizedName(nameof(Parameters.Position))]
			[Positive]
			int position,
			[LocalizedSummary(nameof(Summaries.HelpVariableExactCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IHelpModule module
		)
		{
			if (module.Commands.Count < position)
			{
				return Responses.Misc.HelpInvalidPosition(module, position);
			}
			return Responses.Misc.Help(module.Commands[position - 1]);
		}

		[Command(RunMode = RunMode.Async)]
		[Priority(0)]
		[Hidden]
		public async Task<RuntimeResult> Command(
			[Remainder]
			IReadOnlyList<IHelpModule> modules
		)
		{
			var entry = await NextItemAtIndexAsync(modules, x => x.Name).ConfigureAwait(false);
			if (entry.HasValue)
			{
				return Responses.Misc.Help(entry.Value);
			}
			return AdvobotResult.IgnoreFailure;
		}
	}

	[LocalizedGroup(nameof(Groups.Test))]
	[LocalizedAlias(nameof(Aliases.Test))]
	[LocalizedSummary(nameof(Summaries.Test))]
	[Meta("6c0b693e-e3ac-421e-910e-3178110d791d", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public Task Command()
			=> Task.CompletedTask;
	}
}