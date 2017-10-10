using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Permissions;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Miscellaneous
{
	[Group(nameof(Help)), TopLevelShortAlias(typeof(Help))]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will provide general help.")]
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
				var embed = EmbedActions.MakeNewEmbed("General Help", _GeneralHelp)
					.MyAddField("Basic Syntax", _BasicSyntax)
					.MyAddField("Mention Syntax", _MentionSyntax)
					.MyAddField("Links", _Links)
					.MyAddFooter("Help");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			var helpEntry = Constants.HELP_ENTRIES[commandName];
			if (helpEntry != null)
			{
				var embed = EmbedActions.MakeNewEmbed(helpEntry.Name, helpEntry.ToString())
					.MyAddFooter("Help");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			var closeHelps = new CloseWords<HelpEntry>(Context.User as IGuildUser, Constants.HELP_ENTRIES.GetHelpEntries(), commandName);
			if (closeHelps.List.Any())
			{
				Context.Timers.AddActiveCloseHelp(closeHelps);

				var msg = "Did you mean any of the following:\n" + closeHelps.List.FormatNumberedList("{0}", x => x.Word.Name);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
				return;
			}

			await MessageActions.SendErrorMessage(Context, new ErrorReason("Nonexistent command."));
		}
	}

	[Group(nameof(Commands)), TopLevelShortAlias(typeof(Commands))]
	[Summary("Prints out the commands in that category of the command list. Inputting nothing will list the command categories.")]
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
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("All Commands", desc));
		}
		[Command]
		public async Task Command(CommandCategory category)
		{
			var desc = $"`{String.Join("`, `", GetActions.GetCommandNames(category))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(category.EnumName(), desc));
		}
		[Command]
		public async Task Command()
		{
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Categories", _CommandCategories));
		}
	}

	[Group(nameof(GetId)), TopLevelShortAlias(typeof(GetId))]
	[Summary("Shows the ID of the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetId : AdvobotModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			await MessageActions.SendMessage(Context.Channel, $"The bot has the ID `{Context.Client.CurrentUser.Id}`.");
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			await MessageActions.SendMessage(Context.Channel, $"The guild has the ID `{Context.Guild.Id}`.");
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			await MessageActions.SendMessage(Context.Channel, $"The channel `{channel.Name}` has the ID `{channel.Id}`.");
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			await MessageActions.SendMessage(Context.Channel, $"The role `{role.Name}` has the ID `{role.Id}`.");
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			await MessageActions.SendMessage(Context.Channel, $"The user `{user.Username}` has the ID `{user.Id}`.");
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			await MessageActions.SendMessage(Context.Channel, $"The emote `{emote.Name}` has the ID `{emote.Id}`.");
		}
	}

	[Group(nameof(GetInfo)), TopLevelShortAlias(typeof(GetInfo))]
	[Summary("Shows information about the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : AdvobotModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatBotInfo(Context.BotSettings, Context.Client, Context.Logging, Context.Guild));
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatGuildInfo(Context.Guild as SocketGuild));
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatChannelInfo(Context.GuildSettings, Context.Guild as SocketGuild, channel as SocketChannel));
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatRoleInfo(Context.Guild as SocketGuild, role as SocketRole));
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			if (user is SocketGuildUser socketGuildUser)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketGuildUser));
			}
			else if (user is SocketUser socketUser)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketUser));
			}
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatEmoteInfo(emote));
		}
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite(IInvite invite)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatInviteInfo(invite as IInviteMetadata));
		}
	}

	[Group(nameof(GetUsersWithReason)), TopLevelShortAlias(typeof(GetUsersWithReason))]
	[Summary("Gets users with a variable reason. `Count` specifies if to say the count. `Nickname` specifies if to include nickanmes. `Exact` specifies if only exact matches apply.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : AdvobotModuleBase
	{
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Role, role, additionalSearchOptions);
		}
		[Command(nameof(Name)), ShortAlias(nameof(Name))]
		public async Task Name(string name, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Name, name, additionalSearchOptions);
		}
		[Command(nameof(Game)), ShortAlias(nameof(Game))]
		public async Task Game(string game, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Game, game, additionalSearchOptions);
		}
		[Command(nameof(Stream)), ShortAlias(nameof(Stream))]
		public async Task Stream(params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Stream, null as string, additionalSearchOptions);
		}

		private async Task CommandRunner(Target targetType, object obj, SearchOptions[] additionalSearchOptions)
		{
			var count = additionalSearchOptions.Contains(SearchOptions.Count);
			var nickname = additionalSearchOptions.Contains(SearchOptions.Nickname);
			var exact = additionalSearchOptions.Contains(SearchOptions.Exact);

			var title = "";
			var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
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

			var desc = count ? $"**Count:** `{users.Count()}`" : users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(title, desc));
		}

		public enum SearchOptions : uint
		{
			Count    = (1U << 0),
			Nickname = (1U << 1),
			Exact    = (1U << 2),
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
			await CommandRunner(user, size, format);
		}
		[Command, Priority(1)]
		public async Task Command(ImageFormat format, [Optional] ushort size, [Optional] IUser user)
		{
			await CommandRunner(user, size, format);
		}

		private async Task CommandRunner(IUser user, ushort size = 128, ImageFormat format = ImageFormat.Auto)
		{
			await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size));
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
			var users = await GuildActions.GetUsersAndOrderByJoin(Context.Guild);
			var newPos = Math.Max(1, Math.Min(position, users.Length));
			var user = users[newPos - 1];
			var time = TimeFormatting.FormatReadableDateTime(user.JoinedAt.Value.UtcDateTime);
			var text = $"`{user.FormatUser()}` is `#{newPos}` to join the guild on `{time}`.";
			await MessageActions.SendMessage(Context.Channel, text);
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
			var guilds = await Context.Client.GetGuildsAsync();
			if (guilds.Count() <= 10)
			{
				var embed = EmbedActions.MakeNewEmbed("Guilds");
				foreach (var guild in guilds)
				{
					embed.MyAddField(guild.FormatGuild(), $"**Owner:** `{(await guild.GetOwnerAsync()).FormatUser()}`");
				}

				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				var guildsAndOwners = await Task.WhenAll(guilds.Select(async x => (Guild: x, Owner: await x.GetOwnerAsync())));
				var desc = guildsAndOwners.FormatNumberedList("`{0}` Owner: `{1}`", x => x.Guild.FormatGuild(), x => x.Owner.FormatUser());
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Guilds", desc));
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
			var users = await GuildActions.GetUsersAndOrderByJoin(Context.Guild);
			var text = users.FormatNumberedList("`{0}` joined on `{1}`", x => x.FormatUser(), x => TimeFormatting.FormatReadableDateTime(x.JoinedAt.Value.UtcDateTime));
			await MessageActions.SendTextFile(Context.Channel, text, "User_Joins_");
		}
	}

	[Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild. As of right now, there's no way to upload or remove emotes through Discord's API.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : AdvobotModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
		{
			var emotes = Context.Guild.Emotes.Where(x => x.IsManaged);
			var desc = emotes.Any() ? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name) : $"This guild has no global emotes.";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Emotes", desc));
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var emotes = Context.Guild.Emotes.Where(x => !x.IsManaged);
			var desc = emotes.Any() ? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name) : $"This guild has no guild emotes.";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Emotes", desc));
		}
	}

	[Group(nameof(DownloadMessages)), TopLevelShortAlias(typeof(DownloadMessages))]
	[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DownloadMessages : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int number, [Optional, VerifyObject(true, ObjectVerification.CanBeRead)] ITextChannel channel)
		{
			channel = channel ?? Context.Channel as ITextChannel;
			var messages = (await MessageActions.GetMessages(channel, Math.Min(number, 1000))).OrderBy(x => x.CreatedAt.Ticks).ToArray();

			var formattedMessagesBuilder = new System.Text.StringBuilder();
			var count = 0;
			for (count = 0; count < messages.Length; ++count)
			{
				var text = messages[count].FormatMessage().RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (formattedMessagesBuilder.Length + text.Length >= Context.BotSettings.MaxMessageGatherSize)
				{
					break;
				}
				else
				{
					formattedMessagesBuilder.AppendLineFeed(text);
				}
			}

			await MessageActions.SendTextFile(Context.Channel,
				formattedMessagesBuilder.ToString(),
				$"{channel.Name}_Messages",
				$"Successfully got `{count}` messages");
		}
	}

	/*
	[Group(nameof(MakeAnEmbed)), Alias("mae")]
	[Usage("<\"Title:input\"> <\"Desc:input\"> <Img:url> <Url:url> <Thumb:url> <Color:int/int/int> <\"Author:input\"> <AuthorIcon:url> <AuthorUrl:url> <\"Foot:input\"> <FootIcon:url> " +
		"<\"Field[1-25]:input\"> <\"FieldText[1-25]:input\"> <FieldInline[1-25]:true|false>")]
	[Summary("Every single piece is optional. The stuff in quotes *must* be in quotes. URLs need the https:// in front. Fields need *both* Field and FieldText to work.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MakeAnEmbed : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Remainder] string input)
		{
			var returnedArgs = GetActions.GetArgs(Context, input, 0, 100, new[] { "title", "desc", "img", "url", "thumb", "author", "authoricon", "authorurl", "foot", "footicon" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await MessageActions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var title = returnedArgs.GetSpecifiedArg("title");
			var description = returnedArgs.GetSpecifiedArg("desc");
			var imageUrl = returnedArgs.GetSpecifiedArg("img");
			var Url = returnedArgs.GetSpecifiedArg("url");
			var thumbnail = returnedArgs.GetSpecifiedArg("thumb");
			var authorName = returnedArgs.GetSpecifiedArg("author");
			var authorIcon = returnedArgs.GetSpecifiedArg("authoricon");
			var authorUrl = returnedArgs.GetSpecifiedArg("authorurl");
			var footerText = returnedArgs.GetSpecifiedArg("foot");
			var footerIcon = returnedArgs.GetSpecifiedArg("footicon");

			//Get the color
			var color = Constants.BASE;
			var colorRGB = GetActions.GetVariableAndRemove(returnedArgs.Arguments, "color")?.Split('/');
			if (colorRGB != null && colorRGB.Length == 3)
			{
				const byte MAX_VAL = 255;
				if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}

			var embed = EmbedActions.MakeNewEmbed(title, description, color, imageUrl, Url, thumbnail);
			EmbedActions.AddAuthor(embed, authorName, authorIcon, authorUrl);
			EmbedActions.AddFooter(embed, footerText, footerIcon);

			//Add in the fields and text
			for (int i = 1; i < 25; ++i)
			{
				var field = GetActions.GetVariableAndRemove(returnedArgs.Arguments, "field" + i);
				var fieldText = GetActions.GetVariableAndRemove(returnedArgs.Arguments, "fieldtext" + i);
				//If either is null break out of this loop because they shouldn't be null
				if (field == null || fieldText == null)
					break;

				bool.TryParse(GetActions.GetVariableAndRemove(returnedArgs.Arguments, "fieldinline" + i), out bool inlineBool);
				EmbedActions.AddField(embed, field, fieldText, inlineBool);
			}

			await MessageActions.SendEmbedMessage(Context.Channel, embed);
		}
	}*/

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
				await MessageActions.SendErrorMessage(Context, new ErrorReason("You can already mention this role."));
			}
			else
			{
				var cutText = $"From `{Context.User.FormatUser()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, new ModerationReason(Context.User, null).CreateRequestOptions());
				await MessageActions.SendMessage(Context.Channel, cutText);
				await role.ModifyAsync(x => x.Mentionable = false, new ModerationReason(Context.User, null).CreateRequestOptions());
			}
		}
	}

	[Group(nameof(MessageBotOwner)), TopLevelShortAlias(typeof(MessageBotOwner))]
	[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string message)
		{
			var newMsg = $"From `{Context.User.FormatUser()}` in `{Context.Guild.FormatGuild()}`:\n```\n{message.Substring(0, Math.Min(message.Length, 250))}```";

			var owner = await UserActions.GetBotOwner(Context.Client);
			if (owner != null)
			{
				await owner.SendMessageAsync(newMsg);
			}
			else
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The owner is unable to be gotten."));
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
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendMessage(Context.Channel, $"The number `{number}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong number)
		{
			var perms = ChannelPerms.ConvertValueToNames(number);
			if (!perms.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendMessage(Context.Channel, $"The number `{number}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
	}

	[Group(nameof(Test)), TopLevelShortAlias(typeof(Test))]
	[Summary("Mostly just makes the bot say test.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class Test : AdvobotModuleBase
	{
		[Group("a")]
		public sealed class A : AdvobotModuleBase
		{
			[Command]
			public async Task CommandA()
			{
				await MessageActions.SendMessage(Context.Channel, "test");
			}
		}
		[Command]
		public async Task TestCommand(int cat)
		{
			await MessageActions.SendMessage(Context.Channel, "test");
		}
		[Command("B")]
		public async Task B(int dog)
		{
			await MessageActions.SendMessage(Context.Channel, "test");
		}
	}
}