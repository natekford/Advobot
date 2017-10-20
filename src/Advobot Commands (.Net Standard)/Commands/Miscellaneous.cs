using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Commands.Miscellaneous
{
	[Group(nameof(Help)), TopLevelShortAlias(typeof(Help))]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
		"If left blank will provide general help.")]
	[DefaultEnabled(true)]
	public sealed class Help : AdvobotModuleBase
	{
		private static readonly string _GeneralHelp =
			$"Type `{Constants.PLACEHOLDER_PREFIX}{nameof(Commands)}` for the list of commands.\n" +
			$"Type `{Constants.PLACEHOLDER_PREFIX}{nameof(Help)} [Command]` for help with a command.";
		private static readonly string _BasicSyntax =
			"`[]` means required.\n" +
			"`<>` means optional.\n" +
			"`|` means or.";
		private static readonly string _MentionSyntax =
			"`User` means `@User|\"Username\"`.\n" +
			"`Role` means `@Role|\"Role Name\"`.\n" +
			"`Channel` means `#Channel|\"Channel Name\"`.";
		private static readonly string _Links =
			$"[GitHub Repository]({Constants.REPO})\n" +
			$"[Discord Server]({Constants.DISCORD_INV})";

		[Command]
		public async Task Command([Optional] string commandName)
		{
			if (String.IsNullOrWhiteSpace(commandName))
			{
				var embed = new AdvobotEmbed("General Help", _GeneralHelp)
					.AddField("Basic Syntax", _BasicSyntax)
					.AddField("Mention Syntax", _MentionSyntax)
					.AddField("Links", _Links)
					.AddFooter("Help");
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
				return;
			}

			var helpEntry = Constants.HELP_ENTRIES[commandName];
			if (helpEntry != null)
			{
				var embed = new AdvobotEmbed(helpEntry.Name, helpEntry.ToString())
					.AddFooter("Help");
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
				return;
			}

			var closeHelps = new CloseWords<HelpEntry>(Context.User as IGuildUser, Constants.HELP_ENTRIES.GetHelpEntries(), commandName);
			if (closeHelps.List.Any())
			{
				Context.Timers.AddActiveCloseHelp(closeHelps);

				var msg = "Did you mean any of the following:\n" + closeHelps.List.FormatNumberedList("{0}", x => x.Word.Name);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, msg, Constants.SECONDS_ACTIVE_CLOSE).CAF();
				return;
			}

			await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Nonexistent command.")).CAF();
		}
	}

	[Group(nameof(Commands)), TopLevelShortAlias(typeof(Commands))]
	[Summary("Prints out the commands in that category of the command list. " +
		"Inputting nothing will list the command categories.")]
	[DefaultEnabled(true)]
	public sealed class Commands : AdvobotModuleBase
	{
		private static readonly string _Command = $"Type `{Constants.PLACEHOLDER_PREFIX}commands [Category]` for commands from that category.\n\n";
		private static readonly string _Categories = $"`{String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))}`";
		private static readonly string _CommandCategories = _Command + _Categories;

		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			var desc = $"`{String.Join("`, `", Constants.HELP_ENTRIES.GetCommandNames())}`";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("All Commands", desc)).CAF();
		}
		[Command]
		public async Task Command(CommandCategory category)
		{
			var desc = $"`{String.Join("`, `", GetActions.GetCommandNames(category))}`";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed(category.EnumName(), desc)).CAF();
		}
		[Command]
		public async Task Command()
		{
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Categories", _CommandCategories)).CAF();
		}
	}

	[Group(nameof(GetId)), TopLevelShortAlias(typeof(GetId))]
	[Summary("Shows the ID of the given object. " +
		"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetId : AdvobotModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The bot has the ID `{Context.Client.CurrentUser.Id}`.").CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The guild has the ID `{Context.Guild.Id}`.").CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The channel `{channel.Name}` has the ID `{channel.Id}`.").CAF();
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The role `{role.Name}` has the ID `{role.Id}`.").CAF();
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The user `{user.Username}` has the ID `{user.Id}`.").CAF();
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			await MessageActions.SendMessageAsync(Context.Channel, $"The emote `{emote.Name}` has the ID `{emote.Id}`.").CAF();
		}
	}

	[Group(nameof(GetInfo)), TopLevelShortAlias(typeof(GetInfo))]
	[Summary("Shows information about the given object. " +
		"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			var embed = InfoFormatting.FormatBotInfo(Context.BotSettings, Context.Client, Context.Logging, Context.Guild);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var embed = InfoFormatting.FormatGuildInfo(Context.Guild as SocketGuild);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			var embed = InfoFormatting.FormatChannelInfo(Context.GuildSettings, Context.Guild as SocketGuild, channel as SocketChannel);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			var embed = InfoFormatting.FormatRoleInfo(Context.Guild as SocketGuild, role as SocketRole);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			if (user is SocketGuildUser socketGuildUser)
			{
				var embed = InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketGuildUser);
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else if (user is SocketUser socketUser)
			{
				var embed = InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketUser);
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			var embed = InfoFormatting.FormatEmoteInfo(emote);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite(IInvite invite)
		{
			var embed = InfoFormatting.FormatInviteInfo(invite as IInviteMetadata);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(GetUsersWithReason)), TopLevelShortAlias(typeof(GetUsersWithReason))]
	[Summary("Gets users with a variable reason. " +
		"`Count` specifies if to say the count. " +
		"`Nickname` specifies if to include nickanmes. " +
		"`Exact` specifies if only exact matches apply.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : AdvobotModuleBase
	{
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Role, role, additionalSearchOptions).CAF();
		}
		[Command(nameof(Name)), ShortAlias(nameof(Name))]
		public async Task Name(string name, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Name, name, additionalSearchOptions).CAF();
		}
		[Command(nameof(Game)), ShortAlias(nameof(Game))]
		public async Task Game(string game, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Game, game, additionalSearchOptions).CAF();
		}
		[Command(nameof(Stream)), ShortAlias(nameof(Stream))]
		public async Task Stream(params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Stream, null as string, additionalSearchOptions).CAF();
		}

		private async Task CommandRunner(Target targetType, object obj, SearchOptions[] additionalSearchOptions)
		{
			var count = additionalSearchOptions.Contains(SearchOptions.Count);
			var nickname = additionalSearchOptions.Contains(SearchOptions.Nickname);
			var exact = additionalSearchOptions.Contains(SearchOptions.Exact);

			var title = "";
			var users = (await Context.Guild.GetUsersAsync().CAF()).AsEnumerable();
			switch (targetType)
			{
				case Target.Role:
				{
					var role = obj as IRole;
					title = $"Users With The Role '{role.Name}'";
					users = users.Where(x => x.RoleIds.Contains(role.Id));
					break;
				}
				case Target.Name:
				{
					var str = obj.ToString();
					title = $"Users With Names Containing '{obj}'";
					users = users.Where(x => exact ? x.Username.CaseInsEquals(str) || (nickname && x.Nickname.CaseInsEquals(str)) 
												   : x.Username.CaseInsContains(str) || (nickname && x.Nickname.CaseInsContains(str)));
					break;
				}
				case Target.Game:
				{
					var str = obj.ToString();
					title = $"Users With Games Containing '{obj}'";
					users = users.Where(x => exact ? x.Game.HasValue && x.Game.Value.Name.CaseInsEquals(str) 
												   : x.Game.HasValue && x.Game.Value.Name.CaseInsContains(str));
					break;
				}
				case Target.Stream:
				{
					title = "Users Who Are Streaming";
					users = users.Where(x => x.Game.HasValue && x.Game.Value.StreamType != StreamType.NotStreaming);
					break;
				}
				default:
				{
					return;
				}
			}

			var desc = count
				? $"**Count:** `{users.Count()}`"
				: users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed(title, desc)).CAF();
		}
	}

	[Group(nameof(GetUserAvatar)), TopLevelShortAlias(typeof(GetUserAvatar))]
	[Summary("Shows the URL of the given user's avatar. Must supply a format, can supply a size, and can specify which user.")]
	[DefaultEnabled(true)]
	public sealed class GetUserAvatar : AdvobotModuleBase
	{
		[Command, Priority(0)]
		public async Task Command(ImageFormat format, [Optional] IUser user, [Optional] ushort size)
		{
			await CommandRunner(user, size, format).CAF();
		}
		[Command, Priority(1)]
		public async Task Command(ImageFormat format, [Optional] ushort size, [Optional] IUser user)
		{
			await CommandRunner(user, size, format).CAF();
		}

		private async Task CommandRunner(IUser user, ushort size = 128, ImageFormat format = ImageFormat.Auto)
		{
			await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size)).CAF();
		}
	}

	[Group(nameof(GetUserJoinedAt)), TopLevelShortAlias(typeof(GetUserJoinedAt))]
	[Summary("Shows the user which joined the guild in that position.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUserJoinedAt : AdvobotModuleBase
	{
		[Command]
		public async Task Command(uint position)
		{
			var users = (await Context.Guild.GetUsersAndOrderByJoinAsync().CAF()).ToArray();
			var newPos = Math.Max(1, Math.Min(position, users.Length));
			var user = users[newPos - 1];
			var time = TimeFormatting.FormatReadableDateTime(user.JoinedAt.Value.UtcDateTime);
			var text = $"`{user.FormatUser()}` is `#{newPos}` to join the guild on `{time}`.";
			await MessageActions.SendMessageAsync(Context.Channel, text).CAF();
		}
	}

	[Group(nameof(DisplayGuilds)), TopLevelShortAlias(typeof(DisplayGuilds))]
	[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisplayGuilds : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var guilds = await Context.Client.GetGuildsAsync().CAF();
			if (guilds.Count() <= 10)
			{
				var embed = new AdvobotEmbed("Guilds");
				foreach (var guild in guilds)
				{
					embed.AddField(guild.FormatGuild(), $"**Owner:** `{(await guild.GetOwnerAsync().CAF()).FormatUser()}`");
				}

				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else
			{
				var guildsAndOwners = await Task.WhenAll(guilds.Select(async x => (Guild: x, Owner: await x.GetOwnerAsync().CAF())));
				var desc = guildsAndOwners.FormatNumberedList("`{0}` Owner: `{1}`", x => x.Guild.FormatGuild(), x => x.Owner.FormatUser());
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Guilds", desc)).CAF();
			}
		}
	}

	[Group(nameof(DisplayUserJoinList)), TopLevelShortAlias(typeof(DisplayUserJoinList))]
	[Summary("Lists most of the users who have joined the guild.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayUserJoinList : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var users = await Context.Guild.GetUsersAndOrderByJoinAsync().CAF();
			var text = users.FormatNumberedList("`{0}` joined on `{1}`",
				x => x.FormatUser(),
				x => TimeFormatting.FormatReadableDateTime(x.JoinedAt.Value.UtcDateTime));
			await MessageActions.SendTextFileAsync(Context.Channel, text, "User_Joins_").CAF();
		}
	}

	[Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : AdvobotModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
		{
			var emotes = Context.Guild.Emotes.Where(x => x.IsManaged);
			var desc = emotes.Any()
				? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name)
				: $"This guild has no global emotes.";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Emotes", desc)).CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var emotes = Context.Guild.Emotes.Where(x => !x.IsManaged);
			var desc = emotes.Any()
				? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name)
				: $"This guild has no guild emotes.";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Emotes", desc)).CAF();
		}
	}

	[Group(nameof(DownloadMessages)), TopLevelShortAlias(typeof(DownloadMessages))]
	[Summary("Downloads the past x amount of messages. " +
		"Up to 1000 messages or 500KB worth of formatted text.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DownloadMessages : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int number, [Optional, VerifyObject(true, ObjectVerification.CanBeRead)] ITextChannel channel)
		{
			channel = channel ?? Context.Channel as ITextChannel;
			var messages = await MessageActions.GetMessagesAsync(channel, Math.Min(number, 1000)).CAF();
			var m = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();

			var formattedMessagesBuilder = new System.Text.StringBuilder();
			var count = 0;
			for (count = 0; count < m.Length; ++count)
			{
				var text = m[count].FormatMessage().RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (formattedMessagesBuilder.Length + text.Length < Context.BotSettings.MaxMessageGatherSize)
				{
					formattedMessagesBuilder.AppendLineFeed(text);
					continue;
				}
				break;
			}

			var fileName = $"{channel.Name}_Messages";
			var content = $"Successfully got `{count}` messages";
			await MessageActions.SendTextFileAsync(Context.Channel, formattedMessagesBuilder.ToString(), fileName, content).CAF();
		}
	}

	[Group(nameof(MakeAnEmbed)), TopLevelShortAlias(typeof(MakeAnEmbed))]
	[Summary("Makes an embed with the given arguments. Urls need http:// in front of them. " +
		"FieldInfo can have up to 25 arguments supplied. " +
		"Each must be formatted like the following: `" + CustomEmbed.FORMAT + "`.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MakeAnEmbed : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] CustomArguments<CustomEmbed> arguments)
		{
			var embed = arguments.CreateObject().Embed;
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(MentionRole)), TopLevelShortAlias(typeof(MentionRole))]
	[Summary("Mention an unmentionable role with the given message.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MentionRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder] string message)
		{
			if (role.IsMentionable)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("You can already mention this role.")).CAF();
			}
			else
			{
				var cutText = $"From `{Context.User.FormatUser()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
				await MessageActions.SendMessageAsync(Context.Channel, cutText).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
			}
		}
	}

	[Group(nameof(MessageBotOwner)), TopLevelShortAlias(typeof(MessageBotOwner))]
	[Summary("Sends a message to the bot owner with the given text. " +
		"Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string message)
		{
			var cut = message.Substring(0, Math.Min(message.Length, 250));
			var newMsg = $"From `{Context.User.FormatUser()}` in `{Context.Guild.FormatGuild()}`:\n```\n{cut}```";

			var owner = await ClientActions.GetBotOwnerAsync(Context.Client).CAF();
			if (owner != null)
			{
				await owner.SendMessageAsync(newMsg).CAF();
			}
			else
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("The owner is unable to be gotten.")).CAF();
			}
		}
	}

	[Group(nameof(GetPermNamesFromValue)), TopLevelShortAlias(typeof(GetPermNamesFromValue))]
	[Summary("Lists all the perms that come from the given value.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetPermNamesFromValue : AdvobotModuleBase
	{
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild(ulong number)
		{
			var perms = GuildPerms.ConvertValueToNames(number);
			if (!perms.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("The given number holds no permissions.")).CAF();
			}
			else
			{
				var resp = $"The number `{number}` has the following guild permissions: `{String.Join("`, `", perms)}`.";
				await MessageActions.SendMessageAsync(Context.Channel, resp).CAF();
			}
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong number)
		{
			var perms = ChannelPerms.ConvertValueToNames(number);
			if (!perms.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("The given number holds no permissions.")).CAF();
			}
			else
			{
				var resp = $"The number `{number}` has the following channel permissions: `{String.Join("`, `", perms)}`.";
				await MessageActions.SendMessageAsync(Context.Channel, resp).CAF();
			}
		}
	}

	[Group(nameof(GetEnumNames)), TopLevelShortAlias(typeof(GetEnumNames))]
	[Summary("Prints out all the options of an enum.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetEnumNames : AdvobotModuleBase
	{
		private static IEnumerable<Type> _Enums;

		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", _Enums.Select(x => x.Name))}`";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Enums", desc));
		}
		[Command]
		public async Task Command(string enumName)
		{
			var matchingNames = _Enums.Where(x => x.Name.CaseInsEquals(enumName));
			if (!matchingNames.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason($"No enum has the name `{enumName}`."));
			}
			else if (matchingNames.Count() > 1)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason($"Too many enums have the name `{enumName}`."));
			}

			var e = matchingNames.Single();
			var desc = $"`{String.Join("`, `", Enum.GetNames(e))}`";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed(e.Name, desc));
		}

		public static void SetEnums(CommandService cs)
		{
			_Enums = cs.Commands.SelectMany(x => x.Parameters).Select(x => x.Type).Where(x => x.IsEnum).Distinct();
		}
	}

	[Group(nameof(Test)), TopLevelShortAlias(typeof(Test))]
	[Summary("Mostly just makes the bot say test.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageActions.SendMessageAsync(Context.Channel, "test").CAF();
		}
	}
}