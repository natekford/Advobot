using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Miscellaneous
{
	[Group(nameof(Help)), Alias("h", "info")]
	[Usage("<Command>")]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
	[DefaultEnabled(true)]
	public sealed class Help : MyModuleBase
	{
		private static readonly string _GeneralHelp = $"Type `{Constants.BOT_PREFIX}commands` for the list of commands.\nType `{Constants.BOT_PREFIX}help [Command]` for help with a command.";
		private static readonly string _BasicSyntax = "`[]` means required.\n`<>` means optional.\n`|` means or.";
		private static readonly string _MentionSyntax = $"`User` means `{Constants.USER_INSTRUCTIONS}`.\n`Role` means `{Constants.ROLE_INSTRUCTIONS}`.\n`Channel` means `{Constants.CHANNEL_INSTRUCTIONS}`.";
		private static readonly string _Links = $"[GitHub Repository]({Constants.REPO})\n[Discord Server]({Constants.DISCORD_INV})";

		[Command]
		public async Task Command([Optional] string command)
		{
			if (String.IsNullOrWhiteSpace(command))
			{
				var embed = EmbedActions.MakeNewEmbed("General Help", _GeneralHelp, prefix: GetActions.GetPrefix(Context.BotSettings, Context.GuildSettings));
				EmbedActions.AddField(embed, "Basic Syntax", _BasicSyntax);
				EmbedActions.AddField(embed, "Mention Syntax", _MentionSyntax);
				EmbedActions.AddField(embed, "Links", _Links);
				EmbedActions.AddFooter(embed, "Help");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				var helpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.CaseInsEquals(command) || x.Aliases.CaseInsContains(command));
				if (helpEntry != null)
				{
					var embed = EmbedActions.MakeNewEmbed(helpEntry.Name, helpEntry.ToString(), prefix: GetActions.GetPrefix(Context.BotSettings, Context.GuildSettings));
					EmbedActions.AddFooter(embed, "Help");
					await MessageActions.SendEmbedMessage(Context.Channel, embed);
					return;
				}

				var closeHelps = CloseWordActions.GetObjectsWithSimilarNames(Constants.HELP_ENTRIES, command).Distinct();
				if (closeHelps.Any())
				{
					Context.Timers.GetOutActiveCloseHelp(Context.User.Id);
					Context.Timers.AddActiveCloseHelp(new ActiveCloseWord<HelpEntry>(Context.User.Id, closeHelps));

					var msg = "Did you mean any of the following:\n" + closeHelps.FormatNumberedList("{0}", x => x.Word.Name);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Nonexistent command."));
			}
		}
	}

	[Group(nameof(Commands)), Alias("cmds")]
	[Usage("<Category|All>")]
	[Summary("Prints out the commands in that category of the command list.")]
	[DefaultEnabled(true)]
	public sealed class Commands : MyModuleBase
	{
		[Command("all")]
		public async Task CommandAll()
		{
			var desc = $"`{String.Join("`, `", Constants.COMMAND_NAMES)}`";
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
			var desc = $"Type `{Constants.BOT_PREFIX}commands [Category]` for commands from that category.\n\n`{String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Categories", desc));
		}
	}

	[Group(nameof(GetId)), Alias("gid")]
	[Usage("[Bot|Guild|Channel|Role|User|Emote] <\"Other Argument\">")]
	[Summary("Shows the ID of the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetId : MyModuleBase
	{
		[Command(nameof(Target.Bot))]
		public async Task CommandBot()
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The bot has the ID `{Context.Client.CurrentUser.Id}`.");
		}
		[Command(nameof(Target.Guild))]
		public async Task CommandGuild()
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The guild has the ID `{Context.Guild.Id}`.");
		}
		[Command(nameof(Target.Channel))]
		public async Task CommandChannel(IGuildChannel target)
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The channel `{target.Name}` has the ID `{target.Id}`.");
		}
		[Command(nameof(Target.Role))]
		public async Task CommandRole(IRole target)
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The role `{target.Name}` has the ID `{target.Id}`.");
		}
		[Command(nameof(Target.User))]
		public async Task CommandUser(IUser target)
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The user `{target.Username}` has the ID `{target.Id}`.");
		}
		[Command(nameof(Target.Emote))]
		public async Task CommandEmote(Emote target)
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The emote `{target.Name}` has the ID `{target.Id}`.");
		}
	}

	[Group(nameof(GetInfo)), Alias("ginf")]
	[Usage("[Bot|Guild|Channel|Role|User|Emote|Invite] <\"Other Argument\">")]
	[Summary("Shows information about the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : MyModuleBase
	{
		[Command(nameof(Target.Bot))]
		public async Task CommandBot()
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatBotInfo(Context.BotSettings, Context.Client, Context.Logging, Context.Guild));
		}
		[Command(nameof(Target.Guild))]
		public async Task CommandGuild()
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatGuildInfo(Context.GuildSettings, Context.Guild as SocketGuild));
		}
		[Command(nameof(Target.Channel))]
		public async Task CommandChannel(IGuildChannel target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatChannelInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketChannel));
		}
		[Command(nameof(Target.Role))]
		public async Task CommandRole(IRole target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatRoleInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketRole));
		}
		[Command(nameof(Target.User))]
		public async Task CommandUser(IUser target)
		{
			if (target is IGuildUser)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatUserInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketGuildUser));
			}
			else
			{
				await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatUserInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketUser));
			}
		}
		[Command(nameof(Target.Emote))]
		public async Task CommandEmote(Emote target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatEmoteInfo(Context.GuildSettings, await Context.Client.GetGuildsAsync(), target as Emote));
		}
		[Command(nameof(Target.Invite))]
		public async Task CommandInvite(IInvite target)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, FormattingActions.FormatInviteInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as IInviteMetadata));
		}
	}

	[Group(nameof(GetUsersWithReason)), Alias("guwr")]
	[Usage("[Role|Name|Game|Stream] <\"Other Argument\"> <Count> <Nickname> <Exact>")]
	[Summary("Gets users with a variable reason. Count specifies if to say the count. Nickname specifies if to include nickanmes. Exact specifies if only exact matches count.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : MyModuleBase
	{
		[Command(nameof(Target.Role))]
		public async Task CommandRole([VerifyRole(false, RoleVerification.CanBeEdited)] IRole role, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Role, role, additionalSearchOptions);
		}
		[Command(nameof(Target.Name))]
		public async Task CommandName(string name, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Name, name, additionalSearchOptions);
		}
		[Command(nameof(Target.Game))]
		public async Task CommandGame(string game, params string[] additionalSearchOptions)
		{
			await CommandRunner(Target.Game, game, additionalSearchOptions);
		}
		[Command(nameof(Target.Stream))]
		public async Task CommandStream(params string[] additionalSearchOptions)
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

	[Group(nameof(GetUserAvatar)), Alias("gua")]
	[Usage("[Gif|Png|Jpg|Webp] <Size> <User>")]
	[Summary("Shows the URL of the given user's avatar. Must supply a format, can supply a size, and can specify which user.")]
	[DefaultEnabled(true)]
	public sealed class GetUserAvatar : MyModuleBase
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

	[Group(nameof(GetUserJoinedAt)), Alias("gujat")]
	[Usage("[Number]")]
	[Summary("Shows the user which joined the guild in that position.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetUserJoinedAt : MyModuleBase
	{
		[Command]
		public async Task Command(uint position)
		{
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToArray();

			var newPos = Math.Max(1, Math.Min(position, users.Length));
			var user = users[newPos - 1];
			await MessageActions.SendChannelMessage(Context, $"`{user.FormatUser()}` is `#{newPos}` to join the guild on `{FormattingActions.FormatDateTime(user.JoinedAt)}`.");
		}
	}

	[Group(nameof(DisplayGuilds)), Alias("dgs")]
	[Usage("")]
	[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisplayGuilds : MyModuleBase
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
					EmbedActions.AddField(embed, guild.FormatGuild(), $"**Owner:** `{(await guild.GetOwnerAsync()).FormatUser()}`");
				}

				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				//This may be one of the most retarded work arounds I have ever done.
				var tempTupleList = new List<Tuple<IGuild, IGuildUser>>();
				foreach (var guild in guilds)
				{
					tempTupleList.Add(Tuple.Create(guild, await guild.GetOwnerAsync()));
				}

				var desc = tempTupleList.FormatNumberedList("`{0}` Owner: `{1}`", x => x.Item1.FormatGuild(), x => x.Item2.FormatUser());
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Guilds", desc));
			}
		}
	}

	[Group(nameof(DisplayUserJoinList)), Alias("dujl")]
	[Usage("")]
	[Summary("Lists most of the users who have joined the guild.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayUserJoinList : MyModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToArray();

			var text = users.FormatNumberedList("`{0}` joined on `{1}`", x => x.FormatUser(), x => FormattingActions.FormatDateTime(x.JoinedAt));
			await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "User_Joins_");
		}
	}

	[Group(nameof(DisplayEmotes)), Alias("de")]
	[Usage("[Global|Guild]")]
	[Summary("Lists the emotes in the guild. As of right now, there's no way to upload or remove emotes through Discord's API.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : MyModuleBase
	{
		[Command]
		public async Task Command(EmoteType target)
		{
			IEnumerable<GuildEmote> emotes;
			switch (target)
			{
				case EmoteType.Global:
				{
					emotes = Context.Guild.Emotes.Where(x => x.IsManaged);
					break;
				}
				case EmoteType.Guild:
				{
					emotes = Context.Guild.Emotes.Where(x => !x.IsManaged);
					break;
				}
				default:
				{
					return;
				}
			}

			var desc = emotes.Any()
				? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name)
				: $"This guild has no `{target.EnumName()}` emotes.";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Emotes", desc));
		}
	}

	[Group(nameof(DownloadMessages)), Alias("dlm")]
	[Usage("[Number] <Channel>")]
	[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DownloadMessages : MyModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int num, [Optional, VerifyChannel(true, ChannelVerification.CanBeRead)] ITextChannel channel)
		{
			channel = channel ?? Context.Channel as ITextChannel;
			var messages = (await MessageActions.GetMessages(channel, Math.Min(num, 1000))).OrderBy(x => x.CreatedAt.Ticks).ToArray();

			var formattedMessagesBuilder = new System.Text.StringBuilder();
			var count = 0;
			for (count = 0; count < messages.Length; ++count)
			{
				var text = FormattingActions.FormatMessage(messages[count]).RemoveAllMarkdown().RemoveDuplicateNewLines() + "\n-----\n";
				if (formattedMessagesBuilder.Length + text.Length >= Context.BotSettings.MaxMessageGatherSize)
				{
					break;
				}
				else
				{
					formattedMessagesBuilder.Append(text);
				}
			}

			await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel,
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
	public sealed class MakeAnEmbed : MyModuleBase
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

	[Group(nameof(MentionRole)), Alias("mnr")]
	[Usage("[Role] [Message]")]
	[Summary("Mention an unmentionable role with the given message.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MentionRole : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role, [Remainder] string text)
		{
			if (role.IsMentionable)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("You can already mention this role."));
			}
			else
			{
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, new RequestOptions { AuditLogReason = FormattingActions.FormatUserReason(Context.User) });
				await MessageActions.SendChannelMessage(Context, $"From `{Context.User.FormatUser()}`, {role.Mention}: {text.Substring(0, Math.Min(text.Length, 250))}");
				await role.ModifyAsync(x => x.Mentionable = false, new RequestOptions { AuditLogReason = FormattingActions.FormatUserReason(Context.User) });
			}
		}
	}

	[Group(nameof(MessageBotOwner)), Alias("mbo")]
	[Usage("[Message]")]
	[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : MyModuleBase
	{
		[Command]
		public async Task Command([Remainder] string input)
		{
			var newMsg = $"From `{Context.User.FormatUser()}` in `{Context.Guild.FormatGuild()}`:\n```\n{input.Substring(0, Math.Min(input.Length, 250))}```";

			var owner = await UserActions.GetBotOwner(Context.Client);
			if (owner != null)
			{
				var DMChannel = await owner.GetOrCreateDMChannelAsync();
				await MessageActions.SendDMMessage(DMChannel, newMsg);
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The owner is unable to be gotten."));
			}
		}
	}

	[Group(nameof(GetPermNamesFromValue)), Alias("getperms")]
	[Usage("[Guild|Channel] [Number]")]
	[Summary("Lists all the perms that come from the given value.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class GetPermNamesFromValue : MyModuleBase
	{
		[Command(nameof(Target.Guild))]
		public async Task CommandGuild(ulong permNum)
		{
			var perms = GetActions.GetGuildPermissionNames(permNum);
			if (!perms.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendChannelMessage(Context.Channel, $"The number `{permNum}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
		[Command(nameof(Target.Channel))]
		public async Task CommandChannel(ulong permNum)
		{
			var perms = GetActions.GetChannelPermissionNames(permNum);
			if (!perms.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The given number holds no permissions."));
			}
			else
			{
				await MessageActions.SendChannelMessage(Context.Channel, $"The number `{permNum}` has the following permissions: `{String.Join("`, `", perms)}`.");
			}
		}
	}

	[Group(nameof(Test)), Alias("t")]
	[Usage("")]
	[Summary("Mostly just makes the bot say test.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class Test : MyModuleBase
	{
		[Command]
		public async Task TestCommand()
		{
			await MessageActions.SendChannelMessage(Context, "test");
		}
	}
}