using System;
using System.Collections.Generic;
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
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Gets
{
	[Category(typeof(GetInfo)), Group(nameof(GetInfo)), TopLevelShortAlias(typeof(GetInfo))]
	[Summary("Shows information about the given object. " +
		"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
		public ILogService Logging { get; set; }

		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
			=> await ReplyEmbedAsync(DiscordFormatting.FormatBotInfo(Context.Client, Logging)).CAF();
		[Command(nameof(Shards)), ShortAlias(nameof(Shards))]
		public async Task Shards()
			=> await ReplyEmbedAsync(DiscordFormatting.FormatShardsInfo(Context.Client)).CAF();
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
			=> await ReplyEmbedAsync(DiscordFormatting.FormatGuildInfo(Context.Guild)).CAF();
		[Command(nameof(GuildUsers)), ShortAlias(nameof(GuildUsers))]
		public async Task GuildUsers()
			=> await ReplyEmbedAsync(DiscordFormatting.FormatAllGuildUsersInfo(Context.Guild)).CAF();
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(SocketGuildChannel channel)
			=> await ReplyEmbedAsync(DiscordFormatting.FormatChannelInfo(channel, Context.GuildSettings)).CAF();
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(SocketRole role)
			=> await ReplyEmbedAsync(DiscordFormatting.FormatRoleInfo(role)).CAF();
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(SocketUser user)
			=> await ReplyEmbedAsync(user is SocketGuildUser guildUser
				? DiscordFormatting.FormatGuildUserInfo(guildUser)
				: DiscordFormatting.FormatUserInfo(user)).CAF();
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
			=> await ReplyEmbedAsync(emote is GuildEmote guildEmote
				? DiscordFormatting.FormatGuildEmoteInfo(Context.Guild, guildEmote)
				: DiscordFormatting.FormatEmoteInfo(emote)).CAF();
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite(IInvite invite)
			=> await ReplyEmbedAsync(DiscordFormatting.FormatInviteInfo(invite as IInviteMetadata)).CAF();
		[Command(nameof(Webhook)), ShortAlias(nameof(Webhook))]
		public async Task Webhook(IWebhook webhook)
			=> await ReplyEmbedAsync(DiscordFormatting.FormatWebhookInfo(Context.Guild, webhook)).CAF();
	}

	[Category(typeof(GetUsersWithReason)), Group(nameof(GetUsersWithReason)), TopLevelShortAlias(typeof(GetUsersWithReason))]
	[Summary("Finds users with either a role, a name, a game, or who are streaming.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : AdvobotModuleBase
	{
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(SocketRole role)
			=> await CommandRunner($"Users With The Role '{role.Name}'", x => x.Roles.Select(r => r.Id).Contains(role.Id)).CAF();
		[Command(nameof(Name)), ShortAlias(nameof(Name))]
		public async Task Name(string name)
			=> await CommandRunner($"Users With Names/Nicknames Containing '{name}'", x => x.Username.CaseInsContains(name) || x.Nickname.CaseInsContains(name)).CAF();
		[Command(nameof(Game)), ShortAlias(nameof(Game))]
		public async Task Game(string game)
			=> await CommandRunner($"Users With Games Containing '{game}'", x => x.Activity is Game g && g.Name.CaseInsContains(game)).CAF();
		[Command(nameof(Stream)), ShortAlias(nameof(Stream))]
		public async Task Stream()
			=> await CommandRunner("Users Who Are Streaming", x => x.Activity is StreamingGame).CAF();

		private async Task CommandRunner(string title, Func<SocketGuildUser, bool> predicate)
			=> await ReplyIfAny(Context.Guild.Users.Where(predicate).OrderBy(x => x.JoinedAt), title, x => x.Format()).CAF();
	}

	[Category(typeof(GetUserAvatar)), Group(nameof(GetUserAvatar)), TopLevelShortAlias(typeof(GetUserAvatar))]
	[Summary("Shows the URL of the given user's avatar.")]
	[DefaultEnabled(true)]
	public sealed class GetUserAvatar : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional] IUser user)
			=> await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl()).CAF();
	}

	[Category(typeof(GetUserJoinedAt)), Group(nameof(GetUserJoinedAt)), TopLevelShortAlias(typeof(GetUserJoinedAt))]
	[Summary("Shows the user which joined the guild in that position.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	[DownloadUsers]
	public sealed class GetUserJoinedAt : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidatePositiveNumber] int position)
		{
			var users = Context.Guild.GetUsersByJoinDate().ToArray();
			var newPos = Math.Min(position, users.Length);
			var user = users[newPos - 1];
			await ReplyAsync($"`{user.Format()}` is `#{newPos}` to join the guild on `{user.JoinedAt?.UtcDateTime.ToReadable()}`.").CAF();
		}
	}

	[Category(typeof(GetGuilds)), Group(nameof(GetGuilds)), TopLevelShortAlias(typeof(GetGuilds))]
	[Summary("Lists the name, id, and owner of every guild the bot is on.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class GetGuilds : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.Client.Guilds.Count <= EmbedBuilder.MaxFieldCount)
			{
				var embed = new EmbedWrapper { Title = "Guilds", };
				foreach (var guild in Context.Client.Guilds)
				{
					embed.TryAddField(guild.Format(), $"**Owner:** `{guild.Owner.Format()}`", false, out _);
				}
				await ReplyEmbedAsync(embed).CAF();
				return;
			}
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Guilds",
				Description = Context.Client.Guilds.FormatNumberedList(x => $"`{x.Format()}` Owner: `{x.Owner.Format()}`"),
			}).CAF();
		}
	}

	[Category(typeof(GetUserJoinList)), Group(nameof(GetUserJoinList)), TopLevelShortAlias(typeof(GetUserJoinList))]
	[Summary("Lists most of the users who have joined the guild.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	[DownloadUsers]
	public sealed class GetUserJoinList : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var users = Context.Guild.GetUsersByJoinDate().ToArray();
			await ReplyFileAsync($"**User Join List:**", new TextFileInfo
			{
				Name = "User_Joins",
				Text = users.FormatNumberedList(x => $"`{x.Format()}` joined on `{x.JoinedAt?.UtcDateTime.ToReadable()}`"),
			}).CAF();
		}
	}

	[Category(typeof(GetMessages)), Group(nameof(GetMessages)), TopLevelShortAlias(typeof(GetMessages))]
	[Summary("Downloads the past x amount of messages. " +
		"Up to 1000 messages or 500KB worth of formatted text.")]
	[UserPermissionRequirement(GuildPermission.Administrator)]
	[DefaultEnabled(true)]
	public sealed class GetMessages : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int number, [Optional, ValidateTextChannel(FromContext = true)] SocketTextChannel channel)
		{
			var messages = await MessageUtils.GetMessagesAsync(channel, Math.Min(number, 1000)).CAF();
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

	[Category(typeof(GetPermNamesFromValue)), Group(nameof(GetPermNamesFromValue)), TopLevelShortAlias(typeof(GetPermNamesFromValue))]
	[Summary("Lists all the perms that come from the given value.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetPermNamesFromValue : AdvobotModuleBase
	{
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild(ulong number)
			=> await CommandRunner(number, EnumUtils.GetFlagNames((GuildPermission)number)).CAF();
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong number)
			=> await CommandRunner(number, EnumUtils.GetFlagNames((ChannelPermission)number)).CAF();

		private async Task CommandRunner(ulong value, IEnumerable<string> perms)
			=> await ReplyIfAny(perms, value, "Permissions", x => x).CAF();
	}

	[Category(typeof(GetEnumNames)), Group(nameof(GetEnumNames)), TopLevelShortAlias(typeof(GetEnumNames))]
	[Summary("Prints out all the options of an enum.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetEnumNames : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Enums",
				Description = $"`{EnumTypeTypeReader.Enums.Join("`, `", x => x.Name)}`",
			}).CAF();
		}
		[Command]
		public async Task Command([OverrideTypeReader(typeof(EnumTypeTypeReader))] Type enumType)
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = enumType.Name,
				Description = $"`{string.Join("`, `", Enum.GetNames(enumType))}`",
			}).CAF();
		}
	}
}
