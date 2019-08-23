using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Modules;
using Advobot.Services.Logging;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
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
			public ILogService Logging { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Bot()
				=> Responses.Gets.Bot(Context.Client, Logging);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Shards()
				=> Responses.Gets.Shards(Context.Client);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Guild()
				=> Responses.Gets.Guild(Context.Guild);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> GuildUsers()
				=> Responses.Gets.AllGuildUsers(Context.Guild);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Channel(IGuildChannel channel)
				=> Responses.Gets.Channel(channel, Context.Settings);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Role(IRole role)
				=> Responses.Gets.Role(role);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> User(IUser user)
				=> Responses.Gets.User(user);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Emote(Emote emote)
				=> Responses.Gets.Emote(emote);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Invite(IInviteMetadata invite)
				=> Responses.Gets.Invite(invite);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Webhook(IWebhook webhook)
				=> Responses.Gets.Webhook(webhook);
		}

		[LocalizedGroup(nameof(Groups.GetUsersWithReason))]
		[LocalizedAlias(nameof(Aliases.GetUsersWithReason))]
		[LocalizedSummary(nameof(Summaries.GetUsersWithReason))]
		[Meta("2442d1e5-ac94-4524-a108-37e2f9c23a47", IsEnabled = true)]
		[RequireGenericGuildPermissions]
		public sealed class GetUsersWithReason : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Role(IRole role)
				=> Responses.Gets.UsersWithReason($"Users With The Role '{role.Name}'",
					Context.Guild.Users.Where(x => x.Roles.Select(r => r.Id).Contains(role.Id)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Name(string name)
				=> Responses.Gets.UsersWithReason($"Users With Names/Nicknames Containing '{name}'",
					Context.Guild.Users.Where(x => x.Username.CaseInsContains(name) || x.Nickname.CaseInsContains(name)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Game(string game)
				=> Responses.Gets.UsersWithReason($"Users With Games Containing '{game}'",
					Context.Guild.Users.Where(x => x.Activity is Game g && g.Name.CaseInsContains(game)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Stream()
				=> Responses.Gets.UsersWithReason("Users Who Are Streaming",
					Context.Guild.Users.Where(x => x.Activity is StreamingGame));
		}

		[LocalizedGroup(nameof(Groups.GetUserAvatar))]
		[LocalizedAlias(nameof(Aliases.GetUserAvatar))]
		[LocalizedSummary(nameof(Summaries.GetUserAvatar))]
		[Meta("9978b327-b1bb-46af-9e9f-b43ff411902d", IsEnabled = true)]
		public sealed class GetUserAvatar : AdvobotModuleBase
		{
			[Command]
			public Task Command([Optional] IUser user)
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
			public async Task<RuntimeResult> Command([Channel] ITextChannel channel, int number)
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
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Guild(ulong number)
				=> Responses.Gets.ShowEnumNames<GuildPermission>(number);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Channel(ulong number)
				=> Responses.Gets.ShowEnumNames<ChannelPermission>(number);
		}

		[LocalizedGroup(nameof(Groups.GetEnumNames))]
		[LocalizedAlias(nameof(Aliases.GetEnumNames))]
		[LocalizedSummary(nameof(Summaries.GetEnumNames))]
		[Meta("ed4e05f9-d380-4708-95af-49f0232e15f7", IsEnabled = true)]
		[RequireGenericGuildPermissions]
		public sealed class GetEnumNames : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.ShowAllEnums(EnumTypeTypeReader.Enums);
			[Command]
			public Task<RuntimeResult> Command(
				[OverrideTypeReader(typeof(EnumTypeTypeReader))] Type enumType)
				=> Responses.Gets.ShowEnumValues(enumType);
		}
	}
}
