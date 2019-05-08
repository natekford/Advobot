using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class Gets : ModuleBase
	{
		[Group(nameof(GetInfo)), ModuleInitialismAlias(typeof(GetInfo))]
		[Summary("Shows information about the given object. " +
			"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[EnabledByDefault(true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public sealed class GetInfo : AdvobotModuleBase
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		{
			public ILogService Logging { get; set; }

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
			public Task<RuntimeResult> Channel(SocketGuildChannel channel)
				=> Responses.Gets.Channel(channel, Context.GuildSettings);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Role(SocketRole role)
				=> Responses.Gets.Role(role);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> User(SocketUser user)
				=> Responses.Gets.User(user);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Emote(Emote emote)
				=> Responses.Gets.Emote(emote);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Invite(IInviteMetadata invite)
				=> Responses.Gets.Invite(invite);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Webhook(IWebhook webhook)
				=> Responses.Gets.Webhook(webhook, Context.Guild);
		}

		[Group(nameof(GetUsersWithReason)), ModuleInitialismAlias(typeof(GetUsersWithReason))]
		[Summary("Finds users with either a role, a name, a game, or who are streaming.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class GetUsersWithReason : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Role(SocketRole role)
				=> Responses.Gets.UsersWithReason($"Users With The Role '{role.Name}'", Context.Guild.Users.Where(x => x.Roles.Select(r => r.Id).Contains(role.Id)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Name(string name)
				=> Responses.Gets.UsersWithReason($"Users With Names/Nicknames Containing '{name}'", Context.Guild.Users.Where(x => x.Username.CaseInsContains(name) || x.Nickname.CaseInsContains(name)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Game(string game)
				=> Responses.Gets.UsersWithReason($"Users With Games Containing '{game}'", Context.Guild.Users.Where(x => x.Activity is Game g && g.Name.CaseInsContains(game)));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Stream()
				=> Responses.Gets.UsersWithReason("Users Who Are Streaming", Context.Guild.Users.Where(x => x.Activity is StreamingGame));
		}

		[Group(nameof(GetUserAvatar)), ModuleInitialismAlias(typeof(GetUserAvatar))]
		[Summary("Shows the URL of the given user's avatar.")]
		[EnabledByDefault(true)]
		public sealed class GetUserAvatar : AdvobotModuleBase
		{
			[Command]
			public Task Command([Optional] IUser user)
				=> Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl());
		}

		[Group(nameof(GetUserJoinedAt)), ModuleInitialismAlias(typeof(GetUserJoinedAt))]
		[Summary("Shows the user which joined the guild in that position.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		[DownloadUsers]
		public sealed class GetUserJoinedAt : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidatePositiveNumber] int position)
			{
				var users = Context.Guild.GetUsersByJoinDate().ToArray();
				var newPos = Math.Min(position, users.Length);
				return Responses.Gets.UserJoinPosition(users[newPos - 1], newPos);
			}
		}

		[Group(nameof(GetGuilds)), ModuleInitialismAlias(typeof(GetGuilds))]
		[Summary("Lists the name, id, and owner of every guild the bot is on.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class GetGuilds : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.Guilds(Context.Client.Guilds);
		}

		[Group(nameof(GetUserJoinList)), ModuleInitialismAlias(typeof(GetUserJoinList))]
		[Summary("Lists most of the users who have joined the guild.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		[DownloadUsers]
		public sealed class GetUserJoinList : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.UserJoin(Context.Guild.GetUsersByJoinDate().ToArray());
		}

		[Group(nameof(GetMessages)), ModuleInitialismAlias(typeof(GetMessages))]
		[Summary("Downloads the past x amount of messages. " +
			"Up to 1000 messages or 500KB worth of formatted text.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class GetMessages : AdvobotModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(int number, [Optional, ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				var messages = await channel.GetMessagesAsync(Math.Min(number, 1000)).FlattenAsync().CAF();
				var orderedMessages = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();
				return Responses.Gets.Messages(channel, orderedMessages, BotSettings.MaxMessageGatherSize);
			}
		}

		[Group(nameof(GetPermNamesFromValue)), ModuleInitialismAlias(typeof(GetPermNamesFromValue))]
		[Summary("Lists all the perms that come from the given value.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
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
		[Summary("Prints out all the options of an enum.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class GetEnumNames : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.ShowAllEnums(EnumTypeTypeReader.Enums);
			[Command]
			public Task<RuntimeResult> Command([OverrideTypeReader(typeof(EnumTypeTypeReader))] Type enumType)
				=> Responses.Gets.ShowEnumValues(enumType);
		}
	}
}
