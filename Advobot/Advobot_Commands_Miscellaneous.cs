using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Miscellaneous commands are random commands that don't exactly fit the other groups
	[Name("Miscellaneous")]
	public class Advobot_Commands_Miscellaneous : ModuleBase
	{
		[Command("help")]
		[Alias("h", "info")]
		[Usage("<Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		[DefaultEnabled(true)]
		public async Task Help([Optional, Remainder] string input)
		{
			var prefix = Actions.GetPrefix(await Actions.CreateOrGetGuildInfo(Context.Guild));
			if (String.IsNullOrWhiteSpace(input))
			{
			    var emb = Actions.MakeNewEmbed("General Help", String.Format("Type `{0}commands` for the list of commands.\nType `{0}help [Command]` for help with a command.", prefix));
				Actions.AddField(emb, "Basic Syntax", "`[]` means required.\n`<>` means optional.\n`|` means or.");
				Actions.AddField(emb, "Mention Syntax", String.Format("`User` means `{0}`.\n`Role` means `{1}`.\n`Channel` means `{2}`.",
					Constants.USER_INSTRUCTIONS,
					Constants.ROLE_INSTRUCTIONS,
					Constants.CHANNEL_INSTRUCTIONS));
				Actions.AddField(emb, "Links", String.Format("[GitHub Repository]({0})\n[Discord Server]({1})", Constants.REPO, Constants.DISCORD_INV));
				Actions.AddFooter(emb, "Help");
				await Actions.SendEmbedMessage(Context.Channel, emb);
				return;
			}

			//Send the message for that command
			var helpEntry = Variables.HelpList.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (helpEntry == null)
			{
				//Find the command based on its aliases
				Variables.HelpList.ForEach(x =>
				{
					if (x.Aliases.Contains(input))
					{
						helpEntry = x;
						return;
					}
				});
				if (helpEntry == null)
				{
					//Find close help entries
					var closeHelps = Actions.GetCommandsWithSimilarName(input)?.Distinct().ToList();
					if (closeHelps != null && closeHelps.Any())
					{
						var msg = "Did you mean any of the following:\n" + closeHelps.FormatNumberedList("{0}", x => x.Help.Name);

						Variables.ActiveCloseHelp.ThreadSafeRemoveAll(x => x.UserID == Context.User.Id);
						Variables.ActiveCloseHelp.ThreadSafeAdd(new ActiveCloseHelp(Context.User.Id, closeHelps));
						await Actions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.ACTIVE_CLOSE);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent command."));
					}
					return;
				}
			}

			var embed = Actions.MakeNewEmbed(helpEntry.Name, Actions.GetHelpString(helpEntry, prefix));
			Actions.AddFooter(embed, "Help");
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage("<Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[DefaultEnabled(true)]
		public async Task Commands([Optional, Remainder] string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				var desc = String.Format("Type `{0}commands [Category]` for commands from that category.\n\n{1}",
					Actions.GetPrefix(await Actions.CreateOrGetGuildInfo(Context.Guild)),
					String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))));
				var embed = Actions.MakeNewEmbed("Categories", desc);
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}
			else if (Actions.CaseInsEquals(input, "all"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("All Commands", String.Format("`{0}`", String.Join("`, `", Variables.CommandNames))));
				return;
			}

			if (!Enum.TryParse(input, true, out CommandCategory category))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."));
				return;
			}
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(Enum.GetName(typeof(CommandCategory), category), String.Format("`{0}`", String.Join("`, `", Actions.GetCommands(category)))));
		}

		[Command("getid")]
		[Alias("gid")]
		[Usage("[Guild|Channel|Role|User|Emote|Bot] <\"Other Input\">")]
		[Summary("Shows the ID of the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public async Task GetID([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var targetStr = returnedArgs.Arguments[0];
			var otherStr = returnedArgs.Arguments[1];

			if (Actions.CaseInsEquals(targetStr, "guild"))
			{
				await Actions.SendChannelMessage(Context, String.Format("This guild has the ID `{0}`.", Context.Guild.Id));
			}
			else if (Actions.CaseInsEquals(targetStr, "channel"))
			{
				var channel = Actions.GetChannel(Context, new[] { ChannelCheck.None }, true, otherStr).Object ?? Context.Channel as IGuildChannel;

				await Actions.SendChannelMessage(Context, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.GetChannelType(channel), Actions.EscapeMarkdown(channel.Name, true), channel.Id));
			}
			else if (Actions.CaseInsEquals(targetStr, "role"))
			{
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				var role = returnedRole.Object;

				await Actions.SendChannelMessage(Context, String.Format("The role `{0}` has the ID `{1}`.", Actions.EscapeMarkdown(role.Name, true), role.Id));
			}
			else if (Actions.CaseInsEquals(targetStr, "user"))
			{
				var user = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, otherStr).Object ?? Context.User as IGuildUser;

				await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", Actions.EscapeMarkdown(user.Username, true), user.Discriminator, user.Id));
			}
			else if (Actions.CaseInsEquals(targetStr, "emote"))
			{
				var returnedEmote = Actions.GetEmote(Context, true, otherStr);
				if (returnedEmote.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedEmote);
					return;
				}
				var emote = returnedEmote.Object;

				await Actions.SendChannelMessage(Context, String.Format("The emote `{0}` has the ID `{1}`.", Actions.EscapeMarkdown(emote.Name, true), emote.Id));
			}
			else if (Actions.CaseInsEquals(targetStr, "bot"))
			{
				await Actions.SendChannelMessage(Context, String.Format("The bot has the ID `{0}.`", Variables.BotID));
			}
		}

		[Command("getinfo")]
		[Alias("ginf")]
		[Usage("[Guild|Channel|Role|User|Emote|Invite|Bot] <\"Other Input\">")]
		[Summary("Shows information about the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public async Task GetInfo([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var targetStr = returnedArgs.Arguments[0];
			var otherStr = returnedArgs.Arguments[1];

			var guild = Context.Guild as SocketGuild;
			if (Actions.CaseInsEquals(targetStr, "guild"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatGuildInfo(guildInfo, guild));
			}
			else if (Actions.CaseInsEquals(targetStr, "channel"))
			{
				var channel = (SocketChannel)Actions.GetChannel(Context, new[] { ChannelCheck.None }, true, otherStr).Object ?? (SocketChannel)Context.Channel;

				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatChannelInfo(guildInfo, guild, channel));
			}
			else if (Actions.CaseInsEquals(targetStr, "role"))
			{
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				var role = (SocketRole)returnedRole.Object;

				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatRoleInfo(guildInfo, guild, role));
			}
			else if (Actions.CaseInsEquals(targetStr, "user"))
			{
				var user = (SocketGuildUser)Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, otherStr).Object ?? (SocketUser)Actions.GetGlobalUser(otherStr) ?? (SocketGuildUser)Context.User;

				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatUserInfo(guildInfo, guild, (dynamic)user));
			}
			else if (Actions.CaseInsEquals(targetStr, "emote"))
			{
				var returnedEmote = Actions.GetEmote(Context, true, otherStr);
				if (returnedEmote.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedEmote);
					return;
				}
				var emote = returnedEmote.Object;

				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatEmoteInfo(guildInfo, emote));
			}
			else if (Actions.CaseInsEquals(targetStr, "invite"))
			{
				var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == otherStr);
				if (invite == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invite with that code could be gotten."));
					return;
				}

				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatInviteInfo(guildInfo, guild, invite));
			}
			else if (Actions.CaseInsEquals(targetStr, "bot"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatBotInfo(guild));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid target type supplied."));
			}
		}

		[Command("getuserswithreason")]
		[Alias("guwr")]
		[Usage("[Role|Name|Game|Streaming] [\"Other Argument\"] <Exact:True|False> <Count:True|False> <Nickname:True|False>")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task GetUsersWithReason([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 5), new[] { "exact", "count", "nickname" });
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var targetStr = returnedArgs.Arguments[0];
			var otherArgStr = returnedArgs.Arguments[1];
			var exactStr = returnedArgs.GetSpecifiedArg("exact");
			var countStr = returnedArgs.GetSpecifiedArg("count");
			var nicknameStr = returnedArgs.GetSpecifiedArg("nickname");

			var exact = false;
			if (!String.IsNullOrWhiteSpace(exactStr))
			{
				if (!bool.TryParse(exactStr, out exact))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for exact."));
					return;
				}
			}
			var count = false;
			if (!String.IsNullOrWhiteSpace(countStr))
			{
				if (!bool.TryParse(countStr, out count))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for count."));
					return;
				}
			}
			var nickname = false;
			if (!String.IsNullOrWhiteSpace(nicknameStr))
			{
				if (!bool.TryParse(nicknameStr, out nickname))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for nickname."));
					return;
				}
			}

			var title = "";
			var desc = "";
			var users = (await Context.Guild.GetUsersAsync()).ToList();
			if (Actions.CaseInsEquals(targetStr, "role"))
			{
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherArgStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				var role = returnedRole.Object;

				title = String.Format("Users With The Role '{0}'", role.Name.Insert(3, Constants.ZERO_LENGTH_CHAR));
				users = users.Where(x =>
				{
					return x.RoleIds.Contains(role.Id);
				}).ToList();
			}
			else if (Actions.CaseInsEquals(targetStr, "name"))
			{
				title = String.Format("Users With Names Containing '{0}'", otherArgStr);
				users = users.Where(x =>
				{
					if (exact)
					{
						return Actions.CaseInsEquals(x.Username, otherArgStr) || (nickname && Actions.CaseInsEquals(x?.Nickname, otherArgStr));
					}
					else
					{
						return Actions.CaseInsIndexOf(x.Username, otherArgStr) || (nickname && Actions.CaseInsIndexOf(x?.Nickname, otherArgStr));
					}
				}).ToList();
			}
			else if (Actions.CaseInsEquals(targetStr, "game"))
			{
				title = String.Format("Users With Games Containing '{0}'", otherArgStr);
				users = users.Where(x =>
				{
					if (!x.Game.HasValue)
					{
						return false;
					}

					var gameName = x.Game.Value.Name;
					if (exact)
					{
						return Actions.CaseInsEquals(otherArgStr, gameName);
					}
					else
					{
						return Actions.CaseInsIndexOf(gameName, otherArgStr);
					}
				}).ToList();
			}
			else if (Actions.CaseInsEquals(targetStr, "streaming"))
			{
				title = "Users Who Are Streaming";
				users = users.Where(x =>
				{
					if (!x.Game.HasValue)
					{
						return false;
					}

					return x.Game.Value.StreamType != StreamType.NotStreaming;
				}).ToList();
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid target type."));
				return;
			}


			if (count)
			{
				desc = String.Format("**Count:** `{0}`", users.Count);
			}
			else
			{
				desc = users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
		}

		[Command("getuseravatar")]
		[Alias("gua")]
		[Usage("<User> <Type:Gif|Png|Jpg|Webp>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily).")]
		[DefaultEnabled(true)]
		public async Task UserAvatar([Optional, Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 2), new[] { "type" });
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var formatStr = returnedArgs.GetSpecifiedArg("type");

			//Get the type of image
			var format = ImageFormat.Auto;
			if (!String.IsNullOrWhiteSpace(formatStr))
			{
				if (!Enum.TryParse(formatStr, true, out format))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid avatar format supplied."));
					return;
				}
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, userStr);
			var user = returnedUser.Object ?? Context.User as IGuildUser;

			//Send a message with the URL
			await Context.Channel.SendMessageAsync(user.GetAvatarUrl(format));
		}

		[Command("getuserjoinedat")]
		[Alias("gujat")]
		[Usage("[Position]")]
		[Summary("Shows the user which joined the guild in that position.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task UserJoinedAt([Remainder] string input)
		{
			if (int.TryParse(input, out int position))
			{
				var guildUsers = await Context.Guild.GetUsersAsync();
				var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				if (position >= 1 && position < users.Count)
				{
					var user = users[position - 1];
					await Actions.SendChannelMessage(Context, String.Format("`{0}` was #{1} to join the guild on `{2} {3}, {4}` at `{5}`.",
						user.FormatUser(),
						position,
						System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
						user.JoinedAt.Value.UtcDateTime.Day,
						user.JoinedAt.Value.UtcDateTime.Year,
						user.JoinedAt.Value.UtcDateTime.ToLongTimeString()));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Something besides a number was input."));
			}
		}

		[Command("displayguilds")]
		[Alias("dgs")]
		[Usage("")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task ListGuilds()
		{
			var guilds = Variables.Client.GetGuilds().ToList();
			if (guilds.Count <= 10)
			{
				var embed = Actions.MakeNewEmbed("Guilds");
				guilds.ForEach(x =>
				{
					Actions.AddField(embed, x.FormatGuild(), String.Format("**Owner:** `{0}`", x.Owner.FormatUser()));
				});
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				var str = guilds.FormatNumberedList("`{0}` Owner: `{1}`", x => x.FormatGuild(), x => x.Owner.FormatUser());
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guilds", str));
			}
		}

		[Command("displayuserjoinlist")]
		[Alias("dujl")]
		[Usage("")]
		[Summary("Lists most of the users who have joined the guild.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task UserJoins()
		{
			var users = (await Context.Guild.GetUsersAsync()).OrderBy(x => x.JoinedAt);
			var str = users.FormatNumberedList("`{0}` joined on `{1}`", x => x.FormatUser(), x => Actions.FormatDateTime(x.JoinedAt.HasValue ? x.JoinedAt.Value.UtcDateTime : null as DateTime?));
			await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, str, "User_Joins_");
		}

		[Command("displayemotes")]
		[Alias("de")]
		[Usage("[Global|Guild]")]
		[Summary("Lists the emotes in the guild. As of right now, with the current API wrapper version this bot uses, there's no way to upload or remove emotes yet; sorry.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task ListEmojis([Remainder] string input)
		{
			var emotes = new List<GuildEmote>();
			if (Actions.CaseInsEquals(input, "guild"))
			{
				emotes = Context.Guild.Emotes.Where(x => !x.IsManaged).ToList();
			}
			else if (Actions.CaseInsEquals(input, "global"))
			{
				emotes = Context.Guild.Emotes.Where(x => x.IsManaged).ToList();
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option."));
				return;
			}

			var description = emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name) ?? String.Format("This guild has no {0} emotes.", input.ToLower());
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Emotes", description));
		}

		[Command("downloadmessages")]
		[Alias("dlm")]
		[Usage("[Number] <Channel>")]
		[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task DownloadMessages([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var numStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];

			if (!int.TryParse(numStr, out int num))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number supplied."));
				return;
			}
			else if (num > 1000)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please keep the number under 1,000."));
				return;
			}

			var channel = Actions.GetChannel(Context, new[] { ChannelCheck.IsText, ChannelCheck.CanBeRead }, true, chanStr).Object as ITextChannel ?? Context.Channel;

			var limitAmt = ((int)Variables.BotInfo.GetSetting(SettingOnBot.MaxMessageGatherSize));
			Actions.DontWaitForResultOfBigUnimportantFunction(channel, async () =>
			{
				var charCount = 0;
				var formattedMessages = (await Actions.GetMessages(channel, num)).OrderBy(x => x.CreatedAt.Ticks).Select(msg =>
				{
					var temp = Actions.ReplaceMarkdownChars(Actions.FormatMessage(msg), true);
					return (charCount += System.Text.Encoding.ASCII.GetByteCount(temp)) < limitAmt ? temp : null;
				}).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();

				await Actions.WriteAndUploadTextFile(Context.Guild, channel,
					String.Join("\n-----\n", formattedMessages),
					String.Format("{0}_Messages", channel.Name),
					String.Format("Successfully got `{0}` messages", formattedMessages.Length));
			});
		}

		[Command("makeanembed")]
		[Alias("mae")]
		[Usage("<\"Title:input\"> <\"Desc:input\"> <Img:url> <Url:url> <Thumb:url> <Color:int/int/int> <\"Author:input\"> <AuthorIcon:url> <AuthorUrl:url> <\"Foot:input\"> <FootIcon:url> " +
				"<\"Field[1-25]:input\"> <\"FieldText[1-25]:input\"> <FieldInline[1-25]:true|false>")]
		[Summary("Every single piece is optional. The stuff in quotes *must* be in quotes. URLs need the https:// in front. Fields need *both* Field and FieldText to work.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task MakeAnEmbed([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 100), new[] { "title", "desc", "img", "url", "thumb", "author", "authoricon", "authorurl", "foot", "footicon" });
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var title = returnedArgs.GetSpecifiedArg("title");
			var description = returnedArgs.GetSpecifiedArg("desc");
			var imageURL = returnedArgs.GetSpecifiedArg("img");
			var URL = returnedArgs.GetSpecifiedArg("url");
			var thumbnail = returnedArgs.GetSpecifiedArg("thumb");
			var authorName = returnedArgs.GetSpecifiedArg("author");
			var authorIcon = returnedArgs.GetSpecifiedArg("authoricon");
			var authorURL = returnedArgs.GetSpecifiedArg("authorurl");
			var footerText = returnedArgs.GetSpecifiedArg("foot");
			var footerIcon = returnedArgs.GetSpecifiedArg("footicon");

			//Get the color
			var color = Constants.BASE;
			var colorRGB = Actions.GetVariableAndRemove(returnedArgs.Arguments, "color")?.Split('/');
			if (colorRGB != null && colorRGB.Length == 3)
			{
				const byte MAX_VAL = 255;
				if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}

			var embed = Actions.MakeNewEmbed(title, description, color, imageURL, URL, thumbnail);
			Actions.AddAuthor(embed, authorName, authorIcon, authorURL);
			Actions.AddFooter(embed, footerText, footerIcon);

			//Add in the fields and text
			for (int i = 1; i < 25; i++)
			{
				var field = Actions.GetVariableAndRemove(returnedArgs.Arguments, "field" + i);
				var fieldText = Actions.GetVariableAndRemove(returnedArgs.Arguments, "fieldtext" + i);
				//If either is null break out of this loop because they shouldn't be null
				if (field == null || fieldText == null)
					break;

				bool.TryParse(Actions.GetVariableAndRemove(returnedArgs.Arguments, "fieldinline" + i), out bool inlineBool);
				Actions.AddField(embed, field, fieldText, inlineBool);
			}

			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage("[Role] [\"Message\"]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task MentionRole([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var roleStr = returnedArgs.Arguments[0];
			var textStr = returnedArgs.Arguments[1];

			if (textStr.Length > Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Please keep the message to send under `{0}` characters.", Constants.MAX_MESSAGE_LENGTH_LONG)));
				return;
			}

			//Get the role and see if it can be changed
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.CanBeEdited, RoleCheck.IsEveryone }, true, roleStr);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//See if people can already mention the role
			if (role.IsMentionable)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You can already mention this role."));
				return;
			}

			//Make the role mentionable
			await role.ModifyAsync(x => x.Mentionable = true);
			//Send the message
			var user = Context.User;
			await Actions.SendChannelMessage(Context, String.Format("`{0}`, {1}: {2}", user.FormatUser(), role.Mention, textStr));
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("messagebotowner")]
		[Alias("mbo")]
		[Usage("[Message]")]
		[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task MessageBotOwner([Remainder] string input)
		{
			var cutMsg = input.Substring(0, Math.Min(input.Length, 250));
			var fromMsg = String.Format("From `{0}` in `{1}`:", Context.User.FormatUser(), Context.Guild.FormatGuild());
			var newMsg = String.Format("{0}\n```\n{1}```", fromMsg, cutMsg);

			var owner = Variables.Client.GetUser(((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID)));
			if (owner == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The owner is unable to be gotten."));
				return;
			}
			var DMChannel = await owner.GetOrCreateDMChannelAsync();
			await Actions.SendDMMessage(DMChannel, newMsg);
		}

		[Command("getpermnamesfromvalue")]
		[Alias("getperms")]
		[Usage("[Number]")]
		[Summary("Lists all the perms that come from the given value.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public async Task GetPermsFromValue([Remainder] string input)
		{
			if (!uint.TryParse(input, out uint num))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Input is not a valid number."));
				return;
			}

			var perms = Actions.GetPermissionNames(num);
			if (!perms.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given number holds no permissions."));
				return;
			}
			await Actions.SendChannelMessage(Context.Channel, String.Format("The number `{0}` has the following permissions: `{1}`.", num, String.Join("`, `", perms)));
		}

		[Command("getbotdms")]
		[Alias("gbd")]
		[Usage("<User>")]
		[Summary("Lists all the people who have sent the bot DMs or shows the DMs with a person if one is specified.")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task GetBotDMs([Optional, Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 1));
			if (returnedArgs.Reason != ArgFailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];

			if (!String.IsNullOrWhiteSpace(userStr))
			{
				var user = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, userStr).Object ?? Actions.GetGlobalUser(userStr);
				if (user == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the supplied user."));
					return;
				}

				var channel = (await Variables.Client.GetDMChannelsAsync()).FirstOrDefault(x => x.Recipient != null && x.Recipient.Id == user.Id);
				if (channel == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bot does not have a DM open with that user."));
					return;
				}

				var messages = await Actions.GetBotDMs(channel);
				if (messages.Any())
				{
					var fileTitle = String.Format("DMs_From_{0}", user.Id);
					await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, String.Join("\n-----\n", Actions.FormatDMs(messages)), fileTitle, String.Format("{0} Direct Messages", messages.Count));
				}
				else
				{
					await channel.CloseAsync();
					await Actions.MakeAndDeleteSecondaryMessage(Context, "There are no DMs from that user. I don't know why the bot is saying there were some.");
					return;
				}
			}
			else
			{
				var users = (await Variables.Client.GetDMChannelsAsync()).Select(x => x.Recipient).Where(x => x != null);

				var desc = "";
				if (users.Any())
				{
					desc = String.Format("`{0}`", String.Join("`\n`", users.OrderBy(x => x.Id).Select(x => x.FormatUser())));
				}
				else
				{
					desc = "`None`";
				}

				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Users Who Have DMd The Bot", desc));
			}
		}

		[Command("test")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task Test(string input)
		{
			await Actions.SendChannelMessage(Context, "test");
		}
	}
}