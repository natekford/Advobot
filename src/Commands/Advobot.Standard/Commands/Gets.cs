using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.LogCounters;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using System.Collections.Immutable;

namespace Advobot.Standard.Commands;

[Category(nameof(Gets))]
public sealed class Gets : ModuleBase
{
	[LocalizedGroup(nameof(Groups.GetInfo))]
	[LocalizedAlias(nameof(Aliases.GetInfo))]
	[LocalizedSummary(nameof(Summaries.GetInfo))]
	[Meta("99dcd5e7-6bb2-49cf-b8b7-66b8e063fd18", IsEnabled = true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public ILogCounterService Counters { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		[LocalizedCommand(nameof(Groups.Bot))]
		[LocalizedAlias(nameof(Aliases.Bot))]
		[Priority(1)]
		public Task<RuntimeResult> Bot()
			=> Responses.Gets.Bot(Context.Client, Counters);

		[LocalizedCommand(nameof(Groups.Shards))]
		[LocalizedAlias(nameof(Aliases.Shards))]
		[Priority(1)]
		public Task<RuntimeResult> Shards()
			=> Responses.Gets.Shards(Context.Client);

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

		[LocalizedCommand(nameof(Groups.Me))]
		[LocalizedAlias(nameof(Aliases.Me))]
		[Priority(1)]
		public Task<RuntimeResult> Me()
			=> Responses.Gets.User(Context.User);

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		[Priority(1)]
		public Task<RuntimeResult> Channel(IGuildChannel channel)
			=> Responses.Gets.Channel(channel);

		[LocalizedCommand(nameof(Groups.Role))]
		[LocalizedAlias(nameof(Aliases.Role))]
		[Priority(1)]
		public Task<RuntimeResult> Role(IRole role)
			=> Responses.Gets.Role(role);

		[LocalizedCommand(nameof(Groups.User))]
		[LocalizedAlias(nameof(Aliases.User))]
		[Priority(1)]
		public Task<RuntimeResult> User(IUser user)
			=> Responses.Gets.User(user);

		[LocalizedCommand(nameof(Groups.Emote))]
		[LocalizedAlias(nameof(Aliases.Emote))]
		[Priority(1)]
		public Task<RuntimeResult> Emote(Emote emote)
			=> Responses.Gets.Emote(emote);

		[LocalizedCommand(nameof(Groups.Invite))]
		[LocalizedAlias(nameof(Aliases.Invite))]
		[Priority(1)]
		public Task<RuntimeResult> Invite(IInviteMetadata invite)
			=> Responses.Gets.Invite(invite);

		[LocalizedCommand(nameof(Groups.Webhook))]
		[LocalizedAlias(nameof(Aliases.Webhook))]
		[Priority(1)]
		public Task<RuntimeResult> Webhook(IWebhook webhook)
			=> Responses.Gets.Webhook(webhook);

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
	}

	[LocalizedGroup(nameof(Groups.GetUsersWithReason))]
	[LocalizedAlias(nameof(Aliases.GetUsersWithReason))]
	[LocalizedSummary(nameof(Summaries.GetUsersWithReason))]
	[Meta("2442d1e5-ac94-4524-a108-37e2f9c23a47", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class GetUsersWithReason : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command([Remainder] UserFilterer filterer)
		{
			var matches = filterer.Filter(Context.Guild.Users);
			return Responses.Gets.UsersWithReason(matches);
		}

		[NamedArgumentType]
		public sealed class UserFilterer : Filterer<IGuildUser>
		{
			public IRole? Role { get; set; }
			public string? Name { get; set; }
			public string? Game { get; set; }
			public bool? IsStreaming { get; set; }

			public override IReadOnlyList<IGuildUser> Filter(
				IEnumerable<IGuildUser> source)
			{
				if (Role != null)
				{
					source = source.Where(x => x.RoleIds.Contains(Role.Id));
				}
				if (Name != null)
				{
					source = source.Where(x => x.Username.CaseInsContains(Name) || x.Nickname.CaseInsContains(Name));
				}
				if (Game != null)
				{
					source = source.Where(x => x.Activities.Any(a => a is Game g && g.Name.CaseInsContains(Game)));
				}
				if (IsStreaming != null)
				{
					source = source.Where(x => x.Activities.OfType<StreamingGame>().Any() == IsStreaming);
				}
				return source?.ToArray() ?? Array.Empty<IGuildUser>();
			}
		}
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
		public Task<RuntimeResult> Command([Positive] int position)
		{
			var users = Context.Guild.Users.OrderByJoinDate();
			var newPos = Math.Min(position, users.Count);
			return Responses.Gets.UserJoinPosition(users[newPos - 1], newPos);
		}
	}

	[LocalizedGroup(nameof(Groups.GetGuilds))]
	[LocalizedAlias(nameof(Aliases.GetGuilds))]
	[LocalizedSummary(nameof(Summaries.GetGuilds))]
	[Meta("7ef0f627-453d-45c1-8bd3-ad5b623cb34f", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class GetGuilds : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command()
			=> Responses.Gets.Guilds(Context.Client.Guilds);
	}

	[LocalizedGroup(nameof(Groups.GetUserJoinList))]
	[LocalizedAlias(nameof(Aliases.GetUserJoinList))]
	[LocalizedSummary(nameof(Summaries.GetUserJoinList))]
	[Meta("7ceac749-15ea-4f7f-9c7a-f531b1288d98", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class GetUserJoinList : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command()
			=> Responses.Gets.UserJoin(Context.Guild.Users.OrderByJoinDate());
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
			var messages = await channel.GetMessagesAsync(num).FlattenAsync().CAF();
			var orderedMessages = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();
			return Responses.Gets.Messages(channel, orderedMessages, BotSettings.MaxMessageGatherSize);
		}
	}

	[LocalizedGroup(nameof(Groups.GetPermNamesFromValue))]
	[LocalizedAlias(nameof(Aliases.GetPermNamesFromValue))]
	[LocalizedSummary(nameof(Summaries.GetPermNamesFromValue))]
	[Meta("85c6c3a8-f718-4cae-8331-531a149dee60", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class GetPermNamesFromValue : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Guild))]
		[LocalizedAlias(nameof(Aliases.Guild))]
		public Task<RuntimeResult> Guild(ulong number)
			=> Responses.Gets.ShowEnumNames<GuildPermission>(number);

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		public Task<RuntimeResult> Channel(ulong number)
			=> Responses.Gets.ShowEnumNames<ChannelPermission>(number);
	}
}