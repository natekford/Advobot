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
	[Group(nameof(Help)), TopLevelShortAlias(nameof(Help))]
	[Usage("<Command>")]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
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
		public async Task Command([Optional] string command)
		{
			var temp = DateTime.UtcNow.Subtract(DateTime.UtcNow);
			if (String.IsNullOrWhiteSpace(command))
			{
				var embed = EmbedActions.MakeNewEmbed("General Help", _GeneralHelp)
					.MyAddField("Basic Syntax", _BasicSyntax)
					.MyAddField("Mention Syntax", _MentionSyntax)
					.MyAddField("Links", _Links)
					.MyAddFooter("Help");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				var helpEntry = Constants.HELP_ENTRIES[command];
				if (helpEntry != null)
				{
					var embed = EmbedActions.MakeNewEmbed(helpEntry.Name, helpEntry.ToString())
						.MyAddFooter("Help");
					await MessageActions.SendEmbedMessage(Context.Channel, embed);
					return;
				}

				var closeHelps = new CloseWords<HelpEntry>(Context.User as IGuildUser, Constants.HELP_ENTRIES.GetHelpEntries(), command);
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
	}

	[Group(nameof(Commands)), TopLevelShortAlias(nameof(Commands))]
	[Usage("<Category|All>")]
	[Summary("Prints out the commands in that category of the command list.")]
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

	[Group(nameof(GetId)), TopLevelShortAlias(nameof(GetId))]
	[Usage("[Bot|Guild|Channel|Role|User|Emote] <\"Other Argument\">")]
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
		public async Task Channel(IGuildChannel target)
		{
			await MessageActions.SendMessage(Context.Channel, $"The channel `{target.Name}` has the ID `{target.Id}`.");
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole target)
		{
			await MessageActions.SendMessage(Context.Channel, $"The role `{target.Name}` has the ID `{target.Id}`.");
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser target)
		{
			await MessageActions.SendMessage(Context.Channel, $"The user `{target.Username}` has the ID `{target.Id}`.");
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote target)
		{
			await MessageActions.SendMessage(Context.Channel, $"The emote `{target.Name}` has the ID `{target.Id}`.");
		}
	}

	[Group(nameof(GetInfo)), TopLevelShortAlias(nameof(GetInfo))]
	[Usage("[Bot|Guild|Channel|Role|User|Emote|Invite] <\"Other Argument\">")]
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
		public async Task Channel(IGuildChannel target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatChannelInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketChannel));
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatRoleInfo(Context.Guild as SocketGuild, target as SocketRole));
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser target)
		{
			if (target is SocketGuildUser socketGuildUser)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketGuildUser));
			}
			else if (target is SocketUser socketUser)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketUser));
			}
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatEmoteInfo(target));
		}
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite(IInvite target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, InfoFormatting.FormatInviteInfo(target as IInviteMetadata));
		}
	}

	[Group(nameof(GetUsersWithReason)), TopLevelShortAlias(nameof(GetUsersWithReason))]
	[Usage("[Role|Name|Game|Stream] <\"Other Argument\"> <Count> <Nickname> <Exact>")]
	[Summary("Gets users with a variable reason. Count specifies if to say the count. Nickname specifies if to include nickanmes. Exact specifies if only exact matches count.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : AdvobotModuleBase
	{
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Role, role, additionalSearchOptions);
		}
		[Command(nameof(Name)), ShortAlias(nameof(Name))]
		public async Task Name(string name, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Name, name, additionalSearchOptions);
		}
		[Command(nameof(Game)), ShortAlias(nameof(Game))]
		public async Task Game(string game, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Game, game, additionalSearchOptions);
		}
		[Command(nameof(Stream)), ShortAlias(nameof(Stream))]
		public async Task Stream(params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Stream, null as string, additionalSearchOptions);
		}

		private async Task CommandRunner(Target targetType, string otherArg, string[] additionalSearchOptions)
		{
			var exact = additionalSearchOptions.Any(x => "exact".CaseInsEquals(x));
			var nickname = additionalSearchOptions.Any(x => "nickname".CaseInsEquals(x));
			var count = additionalSearchOptions.Any(x => "count".CaseInsEquals(x));

			var title = "";
			var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
			switch (targetType)
			{
				case Target.Name:
				{
					title = $"Users With Names Containing '{otherArg}'";
					users = users.Where(x => exact ? x.Username.CaseInsEquals(otherArg) || (nickname && x.Nickname.CaseInsEquals(otherArg)) 
												   : x.Username.CaseInsContains(otherArg) || (nickname && x.Nickname.CaseInsContains(otherArg)));
					break;
				}
				case Target.Game:
				{
					title = $"Users With Games Containing '{otherArg}'";
					users = users.Where(x => exact ? x.Game.HasValue && x.Game.Value.Name.CaseInsEquals(otherArg) 
												   : x.Game.HasValue && x.Game.Value.Name.CaseInsContains(otherArg));
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
		private async Task CommandRunner(Target targetType, IRole role, string[] additionalSearchOptions)
		{
			var count = additionalSearchOptions.Any(x => "count".CaseInsEquals(x));

			var title = "";
			var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
			switch (targetType)
			{
				case Target.Role:
				{
					title = $"Users With The Role '{role.Name}'";
					users = users.Where(x => x.RoleIds.Contains(role.Id));
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
	}

	[Group(nameof(GetUserAvatar)), TopLevelShortAlias(nameof(GetUserAvatar))]
	[Usage("[Gif|Png|Jpg|Webp] <Size> <User>")]
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

	[Group(nameof(GetUserJoinedAt)), TopLevelShortAlias(nameof(GetUserJoinedAt))]
	[Usage("[Number]")]
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

	[Group(nameof(DisplayGuilds)), TopLevelShortAlias(nameof(DisplayGuilds))]
	[Usage("")]
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

	[Group(nameof(DisplayUserJoinList)), TopLevelShortAlias(nameof(DisplayUserJoinList))]
	[Usage("")]
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

	[Group(nameof(DisplayEmotes)), TopLevelShortAlias(nameof(DisplayEmotes))]
	[Usage("[Managed|Guild]")]
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

	[Group(nameof(DownloadMessages)), TopLevelShortAlias(nameof(DownloadMessages))]
	[Usage("[Number] <Channel>")]
	[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DownloadMessages : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int num, [Optional, VerifyObject(true, ObjectVerification.CanBeRead)] ITextChannel channel)
		{
			channel = channel ?? Context.Channel as ITextChannel;
			var messages = (await MessageActions.GetMessages(channel, Math.Min(num, 1000))).OrderBy(x => x.CreatedAt.Ticks).ToArray();

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

	[Group(nameof(MentionRole)), TopLevelShortAlias(nameof(MentionRole))]
	[Usage("[Role] [Message]")]
	[Summary("Mention an unmentionable role with the given message.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MentionRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder] string text)
		{
			if (role.IsMentionable)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("You can already mention this role."));
			}
			else
			{
				var cutText = $"From `{Context.User.FormatUser()}`, {role.Mention}: {text.Substring(0, Math.Min(text.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, new ModerationReason(Context.User, null).CreateRequestOptions());
				await MessageActions.SendMessage(Context.Channel, cutText);
				await role.ModifyAsync(x => x.Mentionable = false, new ModerationReason(Context.User, null).CreateRequestOptions());
			}
		}
	}

	[Group(nameof(MessageBotOwner)), TopLevelShortAlias(nameof(MessageBotOwner))]
	[Usage("[Message]")]
	[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string input)
		{
			var newMsg = $"From `{Context.User.FormatUser()}` in `{Context.Guild.FormatGuild()}`:\n```\n{input.Substring(0, Math.Min(input.Length, 250))}```";

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

	[Group(nameof(GetPermNamesFromValue)), TopLevelShortAlias(nameof(GetPermNamesFromValue))]
	[Usage("[Guild|Channel] [Number]")]
	[Summary("Lists all the perms that come from the given value.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetPermNamesFromValue : AdvobotModuleBase
	{
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild(ulong permNum)
		{
			var perms = GuildPerms.ConvertValueToNames(permNum);
			if (!perms.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendMessage(Context.Channel, $"The number `{permNum}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong permNum)
		{
			var perms = ChannelPerms.ConvertValueToNames(permNum);
			if (!perms.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendMessage(Context.Channel, $"The number `{permNum}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
	}

	[Group(nameof(Test)), TopLevelShortAlias(nameof(Test))]
	[Usage("")]
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