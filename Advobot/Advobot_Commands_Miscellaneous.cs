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
	namespace Miscellaneous
	{
		[Usage("<Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		[DefaultEnabled(true)]
		public class Help : ModuleBase<MyCommandContext>
		{
			[Command("help")]
			[Alias("h", "info")]
			public async Task Command(string command)
			{
				await CommandRunner(command);
			}

			private async Task CommandRunner(string command)
			{
				var prefix = Actions.GetPrefix(Context.GuildInfo);
				if (String.IsNullOrWhiteSpace(command))
				{
					var embed = Actions.MakeNewEmbed("General Help", String.Format("Type `{0}commands` for the list of commands.\nType `{0}help [Command]` for help with a command.", prefix));
					Actions.AddField(embed, "Basic Syntax", "`[]` means required.\n`<>` means optional.\n`|` means or.");
					Actions.AddField(embed, "Mention Syntax", String.Format("`User` means `{0}`.\n`Role` means `{1}`.\n`Channel` means `{2}`.",
						Constants.USER_INSTRUCTIONS,
						Constants.ROLE_INSTRUCTIONS,
						Constants.CHANNEL_INSTRUCTIONS));
					Actions.AddField(embed, "Links", String.Format("[GitHub Repository]({0})\n[Discord Server]({1})", Constants.REPO, Constants.DISCORD_INV));
					Actions.AddFooter(embed, "Help");
					await Actions.SendEmbedMessage(Context.Channel, embed);
				}
				else
				{
					var helpEntry = Variables.HelpList.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, command) || x.Aliases.CaseInsContains(command));
					if (helpEntry == null)
					{
						var closeHelps = Actions.GetCommandsWithSimilarName(command)?.Distinct();
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
					}
					else
					{
						var embed = Actions.MakeNewEmbed(helpEntry.Name, Actions.GetHelpString(helpEntry, prefix));
						Actions.AddFooter(embed, "Help");
						await Actions.SendEmbedMessage(Context.Channel, embed);
					}
				}
			}
		}

		[Usage("<Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[DefaultEnabled(true)]
		public class Commands : ModuleBase<MyCommandContext>
		{
			[Command("commands")]
			[Alias("cmds")]
			public async Task Command([Optional] string targetStr)
			{
				await CommandRunner(targetStr);
			}

			private async Task CommandRunner(string targetStr)
			{
				if (String.IsNullOrWhiteSpace(targetStr))
				{
					var desc = String.Format("Type `{0}commands [Category]` for commands from that category.\n\n{1}",
						Actions.GetPrefix(await Actions.CreateOrGetGuildInfo(Context.Guild)),
						String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))));
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Categories", desc));
				}
				else if (Actions.CaseInsEquals(targetStr, "all"))
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Variables.CommandNames));
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("All Commands", desc));
				}
				else if (!Enum.TryParse(targetStr, true, out CommandCategory category))
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Actions.GetCommands(category)));
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(Enum.GetName(typeof(CommandCategory), category), desc));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."));
				}
			}
		}

		[Usage("[Guild|Channel|Role|User|Emote|Invite|Bot] <\"Other Argument\">")]
		[Summary("Shows the ID of the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public class GetID : ModuleBase<MyCommandContext>
		{
			[Command("getid")]
			[Alias("gid")]
			public async Task Command(string targetStr, string otherArg)
			{
				await CommandRunner(targetStr, otherArg);
			}

			private async Task CommandRunner(string targetStr, string otherArg)
			{
				if (!Enum.TryParse(targetStr, true, out GetIDInfoType target))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid target type."));
					return;
				}

				switch (target)
				{
					case GetIDInfoType.Guild:
					{
						await Actions.SendChannelMessage(Context, String.Format("This guild has the ID `{0}`.", Context.Guild.Id));
						return;
					}
					case GetIDInfoType.Channel:
					{
						var channel = Actions.GetChannel(Context, new[] { ChannelCheck.None }, true, otherArg).Object ?? Context.Channel as IGuildChannel;
						await Actions.SendChannelMessage(Context, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.GetChannelType(channel), Actions.EscapeMarkdown(channel.Name, true), channel.Id));
						return;
					}
					case GetIDInfoType.Role:
					{
						var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherArg);
						if (returnedRole.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedRole);
							return;
						}
						var role = returnedRole.Object;
						await Actions.SendChannelMessage(Context, String.Format("The role `{0}` has the ID `{1}`.", Actions.EscapeMarkdown(role.Name, true), role.Id));
						return;
					}
					case GetIDInfoType.User:
					{
						var user = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, otherArg).Object ?? Context.User as IGuildUser;
						await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", Actions.EscapeMarkdown(user.Username, true), user.Discriminator, user.Id));
						return;
					}
					case GetIDInfoType.Emote:
					{
						var returnedEmote = Actions.GetEmote(Context, true, otherArg);
						if (returnedEmote.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedEmote);
							return;
						}
						var emote = returnedEmote.Object;
						await Actions.SendChannelMessage(Context, String.Format("The emote `{0}` has the ID `{1}`.", Actions.EscapeMarkdown(emote.Name, true), emote.Id));
						return;
					}
					case GetIDInfoType.Invite:
					{
						var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == otherArg);
						if (invite == null)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invite with that code could be gotten."));
							return;
						}
						await Actions.SendChannelMessage(Context.Channel, String.Format("The invite `{0}` has the ID `{1}`.", invite.Code, invite.Id));
						return;
					}
					case GetIDInfoType.Bot:
					{
						await Actions.SendChannelMessage(Context, String.Format("The bot has the ID `{0}.`", Variables.BotID));
						return;
					}
				}
			}
		}

		[Usage("[Guild|Channel|Role|User|Emote|Invite|Bot] <\"Other Argument\">")]
		[Summary("Shows information about the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public class GetInfo : ModuleBase<MyCommandContext>
		{
			[Command("getinfo")]
			[Alias("ginf")]
			public async Task Command(string targetStr, [Optional] string otherArg)
			{
				await CommandRunner(targetStr, otherArg);
			}
			
			private async Task CommandRunner(string targetStr, string otherArg)
			{
				if (!Enum.TryParse(targetStr, true, out GetIDInfoType target))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid target type."));
					return;
				}

				var guild = Context.Guild as SocketGuild;
				switch (target)
				{
					case GetIDInfoType.Guild:
					{
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatGuildInfo(Context.GuildInfo, guild));
						return;
					}
					case GetIDInfoType.Channel:
					{
						var channel = (SocketChannel)Actions.GetChannel(Context, new[] { ChannelCheck.None }, true, otherArg).Object ?? (SocketChannel)Context.Channel;
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatChannelInfo(Context.GuildInfo, guild, channel));
						return;
					}
					case GetIDInfoType.Role:
					{
						var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherArg);
						if (returnedRole.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedRole);
							return;
						}
						var role = (SocketRole)returnedRole.Object;
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatRoleInfo(Context.GuildInfo, guild, role));
						return;
					}
					case GetIDInfoType.User:
					{
						var user = (SocketGuildUser)Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, otherArg).Object ?? (SocketUser)Actions.GetGlobalUser(otherArg) ?? (SocketGuildUser)Context.User;
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatUserInfo(Context.GuildInfo, guild, (dynamic)user));
						return;
					}
					case GetIDInfoType.Emote:
					{
						var returnedEmote = Actions.GetEmote(Context, true, otherArg);
						if (returnedEmote.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedEmote);
							return;
						}
						var emote = returnedEmote.Object;
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatEmoteInfo(Context.GuildInfo, emote));
						return;
					}
					case GetIDInfoType.Invite:
					{
						var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == otherArg);
						if (invite == null)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invite with that code could be gotten."));
							return;
						}
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatInviteInfo(Context.GuildInfo, guild, invite));
						return;
					}
					case GetIDInfoType.Bot:
					{
						await Actions.SendEmbedMessage(Context.Channel, Actions.FormatBotInfo(guild));
						return;
					}
				}
			}
		}

		[Usage("[Role|Name|Game|Stream] <\"Other Argument\"> <True|False> <True|False> <True|False>")]
		[Summary("Gets users with a variable reason. First bool specifies if to only give a count. Second specifies if to search for the other argument exactly. Third specifies if to include nicknames.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class GetUsersWithReason : ModuleBase<MyCommandContext>
		{
			[Command("getuserswithreason")]
			[Alias("guwr")]
			public async Task Command(string targetStr, [Optional] string otherArg, [Optional] bool count, [Optional] bool exact, [Optional] bool nickname)
			{
				await CommandRunner(targetStr, otherArg, count, exact, nickname);
			}

			private async Task CommandRunner(string targetStr, string otherArg, bool count = false, bool exact = false, bool nickname = false)
			{
				if (!Enum.TryParse(targetStr, true, out GetUsersWithReasonTargets target))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid target type."));
					return;
				}

				var title = "";
				var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
				switch (target)
				{
					case GetUsersWithReasonTargets.Role:
					{
						var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, otherArg);
						if (returnedRole.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedRole);
							return;
						}
						var role = returnedRole.Object;

						title = String.Format("Users With The Role '{0}'", role.Name);
						users = users.Where(x => x.RoleIds.Contains(role.Id));
						break;
					}
					case GetUsersWithReasonTargets.Name:
					{
						title = String.Format("Users With Names Containing '{0}'", otherArg);
						users = users.Where(x => exact ? Actions.CaseInsEquals(x.Username, otherArg) || (nickname && Actions.CaseInsEquals(x?.Nickname, otherArg))
													   : Actions.CaseInsIndexOf(x.Username, otherArg) || (nickname && Actions.CaseInsIndexOf(x?.Nickname, otherArg)));
						break;
					}
					case GetUsersWithReasonTargets.Game:
					{
						title = String.Format("Users With Games Containing '{0}'", otherArg);
						users = users.Where(x => exact ? x.Game.HasValue && Actions.CaseInsEquals(x.Game.Value.Name, otherArg)
													   : x.Game.HasValue && Actions.CaseInsIndexOf(x.Game.Value.Name, otherArg));
						break;
					}
					case GetUsersWithReasonTargets.Stream:
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

				var desc = count ? String.Format("**Count:** `{0}`", users.Count()) : users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
			}
		}

		[Usage("<User> <Number> <Gif|Png|Jpg|Webp>")]
		[Summary("Shows the URL of the given user's avatar. Can supply a format and size.")]
		[DefaultEnabled(true)]
		public class GetUserAvatar : ModuleBase<MyCommandContext>
		{
			[Command("getuseravatar")]
			[Alias("gua")]
			public async Task Command([Optional] IUser user, [Optional] ushort size, [Optional] string formatStr)
			{
				await CommandRunner(user, size, formatStr);
			}

			private async Task CommandRunner(IUser user, ushort size = 128, string formatStr = null)
			{
				var format = ImageFormat.Auto;
				if (!String.IsNullOrWhiteSpace(formatStr) && !Enum.TryParse(formatStr, true, out format))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid avatar format supplied."));
					return;
				}

				await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size));
			}
		}

		[Usage("[Number]")]
		[Summary("Shows the user which joined the guild in that position.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class GetUserJoinedAt : ModuleBase<MyCommandContext>
		{
			[Command("getuserjoinedat")]
			[Alias("gujat")]
			public async Task Command(uint position)
			{
				await CommandRunner(position);
			}

			private async Task CommandRunner(uint position)
			{
				var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToArray();

				var newPos = Math.Max(1, Math.Min(position, users.Length));
				var user = users[newPos - 1];
				await Actions.SendChannelMessage(Context, String.Format("`{0}` was `#{1}` to join the guild on `{2}`.", user.FormatUser(), newPos, Actions.FormatDateTime(user.JoinedAt)));
			}
		}

		[Usage("")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class DisplayGuilds : ModuleBase<MyCommandContext>
		{
			[Command("displayguilds")]
			[Alias("dgs")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
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
		}

		[Usage("")]
		[Summary("Lists most of the users who have joined the guild.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class DisplayUserJoinList : ModuleBase<MyCommandContext>
		{
			[Command("displayuserjoinlist")]
			[Alias("dujl")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var str = (await Context.Guild.GetUsersAsync()).OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}` joined on `{1}`", x => x.FormatUser(), x => Actions.FormatDateTime(x.JoinedAt));
				await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, str, "User_Joins_");
			}
		}

		[Usage("[Global|Guild]")]
		[Summary("Lists the emotes in the guild. As of right now, there's no way to upload or remove emotes through Discord's API.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class DisplayEmotes : ModuleBase<MyCommandContext>
		{
			[Command("displayemotes")]
			[Alias("de")]
			public async Task Command(string input)
			{
				await CommandRunner(input);
			}

			private async Task CommandRunner(string input)
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

				var description = emotes.Any() ? emotes.FormatNumberedList("<:{0}:{1}> `{2}`", x => x.Name, x => x.Id, x => x.Name) : String.Format("This guild has no {0} emotes.", input.ToLower());
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Emotes", description));
			}
		}

		[Usage("[Number] <Channel>")]
		[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public class DownloadMessages : ModuleBase<MyCommandContext>
		{
			[Command("downloadmessages")]
			[Alias("dlm")]
			public async Task Command(int num, [Optional] ITextChannel channel)
			{
				await CommandRunner(num, channel);
			}

			private async Task CommandRunner(int num, ITextChannel channel)
			{
				num = Math.Min(num, 1000);
				channel = Actions.GetChannel(Context, new[] { ChannelCheck.IsText, ChannelCheck.CanBeRead }, channel).Object as ITextChannel ?? Context.Channel as ITextChannel;

				var limitAmt = ((int)Variables.BotInfo.GetSetting(SettingOnBot.MaxMessageGatherSize));

				var charCount = 0;
				var formattedMessages = new List<string>();
				foreach (var msg in (await Actions.GetMessages(channel, num)).OrderBy(x => x.CreatedAt.Ticks))
				{
					var temp = Actions.ReplaceMarkdownChars(Actions.FormatMessage(msg), true);
					if ((charCount += temp.Length) < limitAmt)
					{
						formattedMessages.Add(temp);
					}
					else
					{
						break;
					}
				}

				await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel,
					String.Join("\n-----\n", formattedMessages),
					String.Format("{0}_Messages", channel.Name),
					String.Format("Successfully got `{0}` messages", formattedMessages.Count));
			}
		}

		[Usage("<\"Title:input\"> <\"Desc:input\"> <Img:url> <Url:url> <Thumb:url> <Color:int/int/int> <\"Author:input\"> <AuthorIcon:url> <AuthorUrl:url> <\"Foot:input\"> <FootIcon:url> " +
			"<\"Field[1-25]:input\"> <\"FieldText[1-25]:input\"> <FieldInline[1-25]:true|false>")]
		[Summary("Every single piece is optional. The stuff in quotes *must* be in quotes. URLs need the https:// in front. Fields need *both* Field and FieldText to work.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class MakeAnEmbed : ModuleBase<MyCommandContext>
		{
			[Command("makeanembed")]
			[Alias("mae")]
			public async Task Command([Remainder] string input)
			{
				await CommandRunner(input);
			}

			private async Task CommandRunner(string input)
			{
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
		}

		[Usage("[Role] [Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class MentionRole : ModuleBase<MyCommandContext>
		{
			[Command("mentionrole")]
			[Alias("mnr")]
			public async Task Command(IRole role, [Remainder] string text)
			{
				await CommandRunner(role, text);
			}

			private async Task CommandRunner(IRole role, string text)
			{
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.CanBeEdited, RoleCheck.IsEveryone }, role);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}

				if (role.IsMentionable)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You can already mention this role."));
				}
				else
				{
					await role.ModifyAsync(x => x.Mentionable = true);
					await Actions.SendChannelMessage(Context, String.Format("From `{0}`, {1}: {2}", Context.User.FormatUser(), role.Mention, text.Substring(0, Math.Min(text.Length, 250))));
					await role.ModifyAsync(x => x.Mentionable = false);
				}
			}
		}

		[Usage("[Message]")]
		[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class MessageBotOwner : ModuleBase<MyCommandContext>
		{
			[Command("messagebotowner")]
			[Alias("mbo")]
			public async Task Command([Remainder] string input)
			{
				await CommandRunner(input);
			}

			private async Task CommandRunner(string input)
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
		}

		[Usage("[Number]")]
		[Summary("Lists all the perms that come from the given value.")]
		[OtherRequirement(1U << (int)Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class GetPermNamesFromValue : ModuleBase<MyCommandContext>
		{
			[Command("getpermnamesfromvalue")]
			[Alias("getperms")]
			public async Task Command(uint permNum)
			{
				await CommandRunner(permNum);
			}

			private async Task CommandRunner(uint permNum)
			{
				var perms = Actions.GetPermissionNames(permNum);
				if (!perms.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given number holds no permissions."));
				}
				else
				{
					await Actions.SendChannelMessage(Context.Channel, String.Format("The number `{0}` has the following permissions: `{1}`.", permNum, String.Join("`, `", perms)));
				}
			}
		}

		[Usage("<User>")]
		[Summary("Lists all the people who have sent the bot DMs or shows the DMs with a person if one is specified.")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class GetBotDMs : ModuleBase<MyCommandContext>
		{
			[Command("getbotdms")]
			[Alias("gbd")]
			public async Task Command([Optional] IUser user)
			{
				await CommandRunner(user);
			}

			private async Task CommandRunner(IUser user = null)
			{
				if (user != null)
				{
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
		}

		[Usage("")]
		[Summary("Mostly just makes the bot say test.")]
		[OtherRequirement(1U << (int)Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class Test : ModuleBase<MyCommandContext>
		{
			[Command("test")]
			[Alias("t")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var test = Context.GuildInfo;
				await Actions.SendChannelMessage(Context, "test");
			}
		}
	}}