using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
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
using FormattingUtils = Advobot.Utilities.FormattingUtils;

namespace Advobot.Commands
{
	public sealed class Gets : ModuleBase
	{
		[Group(nameof(GetInfo)), ModuleInitialismAlias(typeof(GetInfo))]
		[Summary("Shows information about the given object. " +
			"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[EnabledByDefault(true)]
		public sealed class GetInfo : AdvobotModuleBase
		{
			public ILogService Logging { get; set; }

			[ImplicitCommand, ImplicitAlias]
			public Task Bot()
				=> ReplyEmbedAsync(FormattingUtils.FormatBotInfo(Context.Client, Logging));
			[ImplicitCommand, ImplicitAlias]
			public Task Shards()
				=> ReplyEmbedAsync(FormattingUtils.FormatShardsInfo(Context.Client));
			[ImplicitCommand, ImplicitAlias]
			public Task Guild()
				=> ReplyEmbedAsync(FormattingUtils.FormatGuildInfo(Context.Guild));
			[ImplicitCommand, ImplicitAlias]
			public Task GuildUsers()
				=> ReplyEmbedAsync(FormattingUtils.FormatAllGuildUsersInfo(Context.Guild));
			[ImplicitCommand, ImplicitAlias]
			public Task Channel(SocketGuildChannel channel)
				=> ReplyEmbedAsync(FormattingUtils.FormatChannelInfo(channel, Context.GuildSettings));
			[ImplicitCommand, ImplicitAlias]
			public Task Role(SocketRole role)
				=> ReplyEmbedAsync(FormattingUtils.FormatRoleInfo(role));
			[ImplicitCommand, ImplicitAlias]
			public Task User(SocketUser user)
				=> ReplyEmbedAsync(user is SocketGuildUser guildUser
					? FormattingUtils.FormatGuildUserInfo(guildUser)
					: FormattingUtils.FormatUserInfo(user));
			[ImplicitCommand, ImplicitAlias]
			public Task Emote(Emote emote)
				=> ReplyEmbedAsync(emote is GuildEmote guildEmote
					? FormattingUtils.FormatGuildEmoteInfo(Context.Guild, guildEmote)
					: FormattingUtils.FormatEmoteInfo(emote));
			[ImplicitCommand, ImplicitAlias]
			public Task Invite(IInvite invite)
				=> ReplyEmbedAsync(FormattingUtils.FormatInviteInfo((IInviteMetadata)invite));
			[ImplicitCommand, ImplicitAlias]
			public Task Webhook(IWebhook webhook)
				=> ReplyEmbedAsync(FormattingUtils.FormatWebhookInfo(Context.Guild, webhook));
		}

		[Group(nameof(GetUsersWithReason)), ModuleInitialismAlias(typeof(GetUsersWithReason))]
		[Summary("Finds users with either a role, a name, a game, or who are streaming.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class GetUsersWithReason : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task Role(SocketRole role)
				=> CommandRunner($"Users With The Role '{role.Name}'", x => x.Roles.Select(r => r.Id).Contains(role.Id));
			[ImplicitCommand, ImplicitAlias]
			public Task Name(string name)
				=> CommandRunner($"Users With Names/Nicknames Containing '{name}'", x => x.Username.CaseInsContains(name) || x.Nickname.CaseInsContains(name));
			[ImplicitCommand, ImplicitAlias]
			public Task Game(string game)
				=> CommandRunner($"Users With Games Containing '{game}'", x => x.Activity is Game g && g.Name.CaseInsContains(game));
			[ImplicitCommand, ImplicitAlias]
			public Task Stream()
				=> CommandRunner("Users Who Are Streaming", x => x.Activity is StreamingGame);

			private Task CommandRunner(string title, Func<SocketGuildUser, bool> predicate)
				=> ReplyIfAny(Context.Guild.Users.Where(predicate).OrderBy(x => x.JoinedAt), title, x => x.Format());
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
			public Task Command([ValidatePositiveNumber] int position)
			{
				var users = Context.Guild.GetUsersByJoinDate().ToArray();
				var newPos = Math.Min(position, users.Length);
				var user = users[newPos - 1];
				return ReplyAsync($"`{user.Format()}` is `#{newPos}` to join the guild on `{user.JoinedAt?.UtcDateTime.ToReadable()}`.");
			}
		}

		[Group(nameof(GetGuilds)), ModuleInitialismAlias(typeof(GetGuilds))]
		[Summary("Lists the name, id, and owner of every guild the bot is on.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class GetGuilds : AdvobotModuleBase
		{
			[Command]
			public Task Command()
			{
				if (Context.Client.Guilds.Count <= EmbedBuilder.MaxFieldCount)
				{
					var embed = new EmbedWrapper { Title = "Guilds", };
					foreach (var guild in Context.Client.Guilds)
					{
						embed.TryAddField(guild.Format(), $"**Owner:** `{guild.Owner.Format()}`", false, out _);
					}
					return ReplyEmbedAsync(embed);
				}
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Guilds",
					Description = Context.Client.Guilds.FormatNumberedList(x => $"`{x.Format()}` Owner: `{x.Owner.Format()}`"),
				});
			}
		}

		[Group(nameof(GetUserJoinList)), ModuleInitialismAlias(typeof(GetUserJoinList))]
		[Summary("Lists most of the users who have joined the guild.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		[DownloadUsers]
		public sealed class GetUserJoinList : AdvobotModuleBase
		{
			[Command]
			public Task Command()
			{
				var users = Context.Guild.GetUsersByJoinDate().ToArray();
				return ReplyFileAsync($"**User Join List:**", new TextFileInfo
				{
					Name = "User_Joins",
					Text = users.FormatNumberedList(x => $"`{x.Format()}` joined on `{x.JoinedAt?.UtcDateTime.ToReadable()}`"),
				});
			}
		}

		[Group(nameof(GetMessages)), ModuleInitialismAlias(typeof(GetMessages))]
		[Summary("Downloads the past x amount of messages. " +
			"Up to 1000 messages or 500KB worth of formatted text.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class GetMessages : AdvobotModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(int number, [Optional, ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
			{
				var messages = await channel.GetMessagesAsync(Math.Min(number, 1000)).FlattenAsync().CAF();
				var m = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();

				var formattedMessagesBuilder = new StringBuilder();
				var count = 0;
				for (count = 0; count < m.Length; ++count)
				{
					var text = m[count].Format(withMentions: false).RemoveAllMarkdown().RemoveDuplicateNewLines();
					if (formattedMessagesBuilder.Length + text.Length < BotSettings.MaxMessageGatherSize)
					{
						formattedMessagesBuilder.AppendLineFeed(text);
						continue;
					}
					break;
				}

				await ReplyFileAsync($"**{count} Messages:**", new TextFileInfo
				{
					Name = $"{channel?.Name}_Messages",
					Text = formattedMessagesBuilder.ToString()
				}).CAF();
			}
		}

		[Group(nameof(GetPermNamesFromValue)), ModuleInitialismAlias(typeof(GetPermNamesFromValue))]
		[Summary("Lists all the perms that come from the given value.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class GetPermNamesFromValue : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task Guild(ulong number)
				=> ReplyIfAny(EnumUtils.GetFlagNames((GuildPermission)number), number.ToString(), "Guild Permissions", x => x);
			[ImplicitCommand, ImplicitAlias]
			public Task Channel(ulong number)
				=> ReplyIfAny(EnumUtils.GetFlagNames((ChannelPermission)number), number.ToString(), "Channel Permissions", x => x);
		}

		[Group(nameof(GetEnumNames)), ModuleInitialismAlias(typeof(GetEnumNames))]
		[Summary("Prints out all the options of an enum.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class GetEnumNames : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task Show()
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Enums",
					Description = $"`{EnumTypeTypeReader.Enums.Join("`, `", x => x.Name)}`",
				});
			}
			[Command]
			public Task Command([OverrideTypeReader(typeof(EnumTypeTypeReader))] Type enumType)
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = enumType.Name,
					Description = $"`{string.Join("`, `", Enum.GetNames(enumType))}`",
				});
			}
		}
	}
}
