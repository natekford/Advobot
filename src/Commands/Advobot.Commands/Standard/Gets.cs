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
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.Logging;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Standard
{
	public sealed class Gets : ModuleBase
	{
		[Group(nameof(GetInfo)), ModuleInitialismAlias(typeof(GetInfo))]
		[LocalizedSummary(nameof(Summaries.GetInfo))]
		[EnabledByDefault(true)]
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

		[Group(nameof(GetUsersWithReason)), ModuleInitialismAlias(typeof(GetUsersWithReason))]
		[LocalizedSummary(nameof(Summaries.GetUsersWithReason))]
		[GenericGuildPermissionRequirement]
		[EnabledByDefault(true)]
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

		[Group(nameof(GetUserAvatar)), ModuleInitialismAlias(typeof(GetUserAvatar))]
		[LocalizedSummary(nameof(Summaries.GetUserAvatar))]
		[EnabledByDefault(true)]
		public sealed class GetUserAvatar : AdvobotModuleBase
		{
			[Command]
			public Task Command([Optional] IUser user)
				=> Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl());
		}

		[Group(nameof(GetUserJoinedAt)), ModuleInitialismAlias(typeof(GetUserJoinedAt))]
		[LocalizedSummary(nameof(Summaries.GetUserJoinedAt))]
		[GenericGuildPermissionRequirement]
		[EnabledByDefault(true)]
		public sealed class GetUserJoinedAt : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([Positive] int position)
			{
				var users = Context.Guild.Users.OrderByJoinDate();
				var newPos = Math.Min(position, users.Count);
				return Responses.Gets.UserJoinPosition(users.ElementAt(newPos - 1), newPos);
			}
		}

		[Group(nameof(GetGuilds)), ModuleInitialismAlias(typeof(GetGuilds))]
		[LocalizedSummary(nameof(Summaries.GetGuilds))]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class GetGuilds : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.Guilds(Context.Client.Guilds);
		}

		[Group(nameof(GetUserJoinList)), ModuleInitialismAlias(typeof(GetUserJoinList))]
		[LocalizedSummary(nameof(Summaries.GetUserJoinList))]
		[GenericGuildPermissionRequirement]
		[EnabledByDefault(true)]
		public sealed class GetUserJoinList : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.UserJoin(Context.Guild.Users.OrderByJoinDate());
		}

		[Group(nameof(GetMessages)), ModuleInitialismAlias(typeof(GetMessages))]
		[LocalizedSummary(nameof(Summaries.GetMessages))]
		[GuildPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
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

		[Group(nameof(GetPermNamesFromValue)), ModuleInitialismAlias(typeof(GetPermNamesFromValue))]
		[LocalizedSummary(nameof(Summaries.GetPermNamesFromValue))]
		[GenericGuildPermissionRequirement]
		[EnabledByDefault(true)]
		public sealed class GetPermNamesFromValue : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Guild(ulong number)
				=> Responses.Gets.ShowEnumNames<GuildPermission>(number);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Channel(ulong number)
				=> Responses.Gets.ShowEnumNames<ChannelPermission>(number);
		}

		[Group(nameof(GetEnumNames)), ModuleInitialismAlias(typeof(GetEnumNames))]
		[LocalizedSummary(nameof(Summaries.GetEnumNames))]
		[GenericGuildPermissionRequirement]
		[EnabledByDefault(true)]
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
