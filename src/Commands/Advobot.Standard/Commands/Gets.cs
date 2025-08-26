using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Standard.Commands;

[Category(nameof(Gets))]
public sealed class Gets : ModuleBase
{
	[LocalizedGroup(nameof(Groups.GetGuilds))]
	[LocalizedAlias(nameof(Aliases.GetGuilds))]
	[LocalizedSummary(nameof(Summaries.GetGuilds))]
	[Meta("7ef0f627-453d-45c1-8bd3-ad5b623cb34f", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class GetGuilds : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command()
		{
			var guilds = await Context.Client.GetGuildsAsync().ConfigureAwait(false);
			return Responses.Gets.Guilds(guilds);
		}
	}

	[LocalizedGroup(nameof(Groups.GetInfo))]
	[LocalizedAlias(nameof(Aliases.GetInfo))]
	[LocalizedSummary(nameof(Summaries.GetInfo))]
	[Meta("99dcd5e7-6bb2-49cf-b8b7-66b8e063fd18", IsEnabled = true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Bot))]
		[LocalizedAlias(nameof(Aliases.Bot))]
		[Priority(1)]
		public Task<RuntimeResult> Bot()
			=> Responses.Gets.Bot(Context.Client);

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		[Priority(1)]
		public Task<RuntimeResult> Channel(IGuildChannel channel)
			=> Responses.Gets.Channel(channel);

		[LocalizedCommand(nameof(Groups.Emote))]
		[LocalizedAlias(nameof(Aliases.Emote))]
		[Priority(1)]
		public Task<RuntimeResult> Emote(Emote emote)
			=> Responses.Gets.Emote(emote);

		[LocalizedCommand(nameof(Groups.Guild))]
		[LocalizedAlias(nameof(Aliases.Guild))]
		[Priority(1)]
		public Task<RuntimeResult> Guild()
			=> Responses.Gets.Guild(Context.Guild);

		[LocalizedCommand(nameof(Groups.GuildUsers))]
		[LocalizedAlias(nameof(Aliases.GuildUsers))]
		[Priority(1)]
		public Task<RuntimeResult> GuildUsers()
			=> Responses.Gets.AllGuildUsers(Context.Guild);

		[Command]
		public Task<RuntimeResult> Implicit(IBan ban)
			=> Responses.Gets.Ban(ban);

		[Command]
		public Task<RuntimeResult> Implicit(IGuildChannel channel)
			=> Responses.Gets.Channel(channel);

		[Command]
		public Task<RuntimeResult> Implicit(IRole role)
			=> Responses.Gets.Role(role);

		[Command]
		public Task<RuntimeResult> Implicit(IUser user)
			=> Responses.Gets.User(user);

		[Command]
		public Task<RuntimeResult> Implicit(Emote emote)
			=> Responses.Gets.Emote(emote);

		[Command]
		public Task<RuntimeResult> Implicit(IInviteMetadata invite)
			=> Responses.Gets.Invite(invite);

		[Command]
		public Task<RuntimeResult> Implicit(IWebhook webhook)
			=> Responses.Gets.Webhook(webhook);

		[LocalizedCommand(nameof(Groups.Invite))]
		[LocalizedAlias(nameof(Aliases.Invite))]
		[Priority(1)]
		public Task<RuntimeResult> Invite(IInviteMetadata invite)
			=> Responses.Gets.Invite(invite);

		[LocalizedCommand(nameof(Groups.Me))]
		[LocalizedAlias(nameof(Aliases.Me))]
		[Priority(1)]
		public Task<RuntimeResult> Me()
			=> Responses.Gets.User(Context.User);

		[LocalizedCommand(nameof(Groups.Role))]
		[LocalizedAlias(nameof(Aliases.Role))]
		[Priority(1)]
		public Task<RuntimeResult> Role(IRole role)
			=> Responses.Gets.Role(role);

		[LocalizedCommand(nameof(Groups.Shards))]
		[LocalizedAlias(nameof(Aliases.Shards))]
		[Priority(1)]
		public Task<RuntimeResult> Shards()
		{
			if (Context.Client is DiscordShardedClient shardedClient)
			{
				return Responses.Gets.Shards(shardedClient);
			}
			return Responses.Gets.Bot(Context.Client);
		}

		[LocalizedCommand(nameof(Groups.User))]
		[LocalizedAlias(nameof(Aliases.User))]
		[Priority(1)]
		public Task<RuntimeResult> User(IUser user)
			=> Responses.Gets.User(user);

		[LocalizedCommand(nameof(Groups.Webhook))]
		[LocalizedAlias(nameof(Aliases.Webhook))]
		[Priority(1)]
		public Task<RuntimeResult> Webhook(IWebhook webhook)
			=> Responses.Gets.Webhook(webhook);
	}

	[LocalizedGroup(nameof(Groups.GetMessages))]
	[LocalizedAlias(nameof(Aliases.GetMessages))]
	[LocalizedSummary(nameof(Summaries.GetMessages))]
	[Meta("0004a4f6-23ab-4e5e-bb4a-0b16e5b403a3", IsEnabled = true)]
	[RequireGuildPermissions]
	public sealed class GetMessages : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command([CanModifyChannel] ITextChannel channel, int number)
		{
			var num = Math.Min(number, 1000);
			var messages = await channel.GetMessagesAsync(num).FlattenAsync().ConfigureAwait(false);
			var orderedMessages = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();
			return Responses.Gets.Messages(channel, orderedMessages, BotConfig.MaxMessageGatherSize);
		}
	}

	[LocalizedGroup(nameof(Groups.GetPermNamesFromValue))]
	[LocalizedAlias(nameof(Aliases.GetPermNamesFromValue))]
	[LocalizedSummary(nameof(Summaries.GetPermNamesFromValue))]
	[Meta("85c6c3a8-f718-4cae-8331-531a149dee60", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class GetPermNamesFromValue : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		public Task<RuntimeResult> Channel(ulong number)
			=> Responses.Gets.ShowEnumNames<ChannelPermission>(number);

		[LocalizedCommand(nameof(Groups.Guild))]
		[LocalizedAlias(nameof(Aliases.Guild))]
		public Task<RuntimeResult> Guild(ulong number)
			=> Responses.Gets.ShowEnumNames<GuildPermission>(number);
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

	[LocalizedGroup(nameof(Groups.GetUserJoinedAt))]
	[LocalizedAlias(nameof(Aliases.GetUserJoinedAt))]
	[LocalizedSummary(nameof(Summaries.GetUserJoinedAt))]
	[Meta("32c92c8f-537f-4551-96a6-709c89c5dfac", IsEnabled = true)]
	public sealed class GetUserJoinedAt : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command([Positive] int position)
		{
			var users = (await Context.Guild.GetUsersAsync().ConfigureAwait(false))
				.Where(x => x.JoinedAt.HasValue)
				.OrderBy(x => x.JoinedAt.GetValueOrDefault().Ticks);

			var i = 0;
			var requestedUser = users.First();
			foreach (var user in users)
			{
				requestedUser = user;
				if (++i == position)
				{
					break;
				}
			}

			return Responses.Gets.UserJoinPosition(requestedUser, i + 1);
		}
	}
}