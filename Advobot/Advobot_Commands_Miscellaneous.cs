using Advobot.Actions;
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
	namespace Miscellaneous
	{
		[Group("help"), Alias("h", "info")]
		[Usage("<Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		[DefaultEnabled(true)]
		public sealed class Help : MyModuleBase
		{
			[Command]
			public async Task Command([Optional] string command)
			{
				await CommandRunner(command);
			}

			private async Task CommandRunner(string command)
			{
				if (String.IsNullOrWhiteSpace(command))
				{
					var embed = Embeds.MakeNewEmbed("General Help", String.Format("Type `{0}commands` for the list of commands.\nType `{0}help [Command]` for help with a command.", Constants.BOT_PREFIX));
					Embeds.AddField(embed, "Basic Syntax", "`[]` means required.\n`<>` means optional.\n`|` means or.");
					Embeds.AddField(embed, "Mention Syntax", String.Format("`User` means `{0}`.\n`Role` means `{1}`.\n`Channel` means `{2}`.",
						Constants.USER_INSTRUCTIONS,
						Constants.ROLE_INSTRUCTIONS,
						Constants.CHANNEL_INSTRUCTIONS));
					Embeds.AddField(embed, "Links", String.Format("[GitHub Repository]({0})\n[Discord Server]({1})", Constants.REPO, Constants.DISCORD_INV));
					Embeds.AddFooter(embed, "Help");
					await Messages.SendEmbedMessage(Context.Channel, embed);
				}
				else
				{
					var helpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.CaseInsEquals(command) || x.Aliases.CaseInsContains(command));
					if (helpEntry != null)
					{
						var embed = Embeds.MakeNewEmbed(helpEntry.Name, helpEntry.ToString());
						Embeds.AddFooter(embed, "Help");
						await Messages.SendEmbedMessage(Context.Channel, embed);
						return;
					}

					var closeHelps = CloseWords.GetObjectsWithSimilarNames(Constants.HELP_ENTRIES, command).Distinct();
					if (closeHelps.Any())
					{
						Context.Timers.ActiveCloseHelp.ThreadSafeRemoveAll(x => x.UserID == Context.User.Id);
						Context.Timers.ActiveCloseHelp.ThreadSafeAdd(new ActiveCloseWord<HelpEntry>(Context.User.Id, closeHelps));

						var msg = "Did you mean any of the following:\n" + closeHelps.FormatNumberedList("{0}", x => x.Word.Name);
						await Messages.MakeAndDeleteSecondaryMessage(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
						return;
					}

					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Nonexistent command."));
				}
			}
		}

		[Group("commands"), Alias("cmds")]
		[Usage("<Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[DefaultEnabled(true)]
		public sealed class Commands : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner(false);
			}
			[Command]
			public async Task Command(CommandCategory category)
			{
				await CommandRunner(category);
			}
			[Command("all")]
			public async Task CommandAll()
			{
				await CommandRunner(true);
			}

			private async Task CommandRunner(bool all)
			{
				if (all)
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Constants.COMMAND_NAMES));
					await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed("All Commands", desc));
				}
				else
				{
					var desc = String.Format("Type `{0}commands [Category]` for commands from that category.\n\n{1}",
						Constants.BOT_PREFIX,
						String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))));
					await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed("Categories", desc));
				}
			}
			private async Task CommandRunner(CommandCategory category)
			{
				var desc = String.Format("`{0}`", String.Join("`, `", Gets.GetCommands(category)));
				await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed(category.EnumName(), desc));
			}
		}

		[Group("getid"), Alias("gid")]
		[Usage("[Bot|Guild|Channel|Role|User|Emote] <\"Other Argument\">")]
		[Summary("Shows the ID of the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public sealed class GetID : MyModuleBase
		{
			[Command("bot")]
			public async Task CommandBot()
			{
				await CommandRunner(Target.Bot, null);
			}
			[Command("guild")]
			public async Task CommandGuild()
			{
				await CommandRunner(Target.Guild, null);
			}
			[Command("channel")]
			public async Task CommandChannel(IGuildChannel target)
			{
				await CommandRunner(Target.Channel, target);
			}
			[Command("role")]
			public async Task CommandRole(IRole target)
			{
				await CommandRunner(Target.Role, target);
			}
			[Command("user")]
			public async Task CommandUser(IUser target)
			{
				await CommandRunner(Target.User, target);
			}
			[Command("emote")]
			public async Task CommandEmote(Emote target)
			{
				await CommandRunner(Target.Emote, target);
			}

			private async Task CommandRunner(Target targetType, object target)
			{
				switch (targetType)
				{
					case Target.Guild:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The guild has the ID `{0}`.", Context.Guild.Id));
						return;
					}
					case Target.Bot:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The bot has the ID `{0}`.", Context.Client.CurrentUser.Id));
						return;
					}
					case Target.Channel:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The channel `{0}` has the ID `{1}`.", (target as IChannel).Name, (target as IChannel).Id));
						return;
					}
					case Target.Role:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The role `{0}` has the ID `{1}`.", (target as IRole).Name, (target as IRole).Id));
						return;
					}
					case Target.User:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The user `{0}` has the ID `{1}`.", (target as IUser).Username, (target as IUser).Id));
						return;
					}
					case Target.Emote:
					{
						await Messages.SendChannelMessage(Context.Channel, String.Format("The emote `{0}` has the ID `{1}`.", (target as Emote).Name, (target as Emote).Id));
						return;
					}
				}
			}
		}

		[Group("getinfo"), Alias("ginf")]
		[Usage("[Bot|Guild|Channel|Role|User|Emote|Invite] <\"Other Argument\">")]
		[Summary("Shows information about the given object. Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
		[DefaultEnabled(true)]
		public sealed class GetInfo : MyModuleBase
		{
			[Command("bot")]
			public async Task CommandBot()
			{
				await CommandRunner(Target.Bot, null);
			}
			[Command("guild")]
			public async Task CommandGuild()
			{
				await CommandRunner(Target.Guild, null);
			}
			[Command("channel")]
			public async Task CommandChannel(IGuildChannel target)
			{
				await CommandRunner(Target.Channel, target);
			}
			[Command("role")]
			public async Task CommandRole(IRole target)
			{
				await CommandRunner(Target.Role, target);
			}
			[Command("user")]
			public async Task CommandUser(IUser target)
			{
				await CommandRunner(Target.User, target);
			}
			[Command("emote")]
			public async Task CommandEmote(Emote target)
			{
				await CommandRunner(Target.Emote, target);
			}
			[Command("invite")]
			public async Task CommandInvite(IInvite target)
			{
				await CommandRunner(Target.Invite, target);
			}

			private async Task CommandRunner(Target targetType, object target)
			{
				switch (targetType)
				{
					case Target.Guild:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatGuildInfo(Context.GuildSettings, Context.Guild as SocketGuild));
						return;
					}
					case Target.Bot:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatBotInfo(Context.BotSettings, Context.Client, Context.Logging, Context.Guild));
						return;
					}
					case Target.Channel:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatChannelInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketChannel));
						return;
					}
					case Target.Role:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatRoleInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketRole));
						return;
					}
					case Target.User:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatUserInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as SocketGuildUser ?? target as SocketUser));
						return;
					}
					case Target.Emote:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatEmoteInfo(Context.GuildSettings, await Context.Client.GetGuildsAsync(), target as Emote));
						return;
					}
					case Target.Invite:
					{
						await Messages.SendEmbedMessage(Context.Channel, Formatting.FormatInviteInfo(Context.GuildSettings, Context.Guild as SocketGuild, target as IInviteMetadata));
						return;
					}
				}
			}
		}

		[Group("getuserswithreason"), Alias("guwr")]
		[Usage("[Role|Name|Game|Stream] [True|False] <\"Other Argument\"> <True|False> <True|False>")]
		[Summary("Gets users with a variable reason. 1st bool specifies if to only give a count. 2nd specifies if to search for the other argument exactly. 3rd specifies if to include nicknames. 2nd/3rd don't apply to roles or streams. 3rd only applies to names.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class GetUsersWithReason : MyModuleBase
		{
			[Command("role")]
			public async Task CommandRole(bool count, [VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				await CommandRunner(Target.Role, count, role, false, false);
			}
			[Command("name")]
			public async Task CommandName(bool count, string name, [Optional] bool exact, [Optional] bool nickname)
			{
				await CommandRunner(Target.Name, count, name, exact, nickname);
			}
			[Command("game")]
			public async Task CommandGame(bool count, string game, [Optional] bool exact)
			{
				await CommandRunner(Target.Game, count, game, exact, false);
			}
			[Command("stream")]
			public async Task CommandString(bool count)
			{
				await CommandRunner(Target.Stream, count, null as string, false, false);
			}

			private async Task CommandRunner(Target targetType, bool count, string otherArg, bool exact, bool nickname)
			{
				var title = "";
				var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
				switch (targetType)
				{
					case Target.Name:
					{
						title = String.Format("Users With Names Containing '{0}'", otherArg);
						users = users.Where(x => exact ? x.Username.CaseInsEquals(otherArg) || (nickname && x.Nickname.CaseInsEquals(otherArg))
													   : x.Username.CaseInsContains(otherArg) || (nickname && x.Nickname.CaseInsContains(otherArg)));
						break;
					}
					case Target.Game:
					{
						title = String.Format("Users With Games Containing '{0}'", otherArg);
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

				var desc = count ? String.Format("**Count:** `{0}`", users.Count()) : users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
				await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed(title, desc));
			}
			private async Task CommandRunner(Target targetType, bool count, IRole role, bool exact, bool nickname)
			{
				var title = "";
				var users = (await Context.Guild.GetUsersAsync()).AsEnumerable();
				switch (targetType)
				{
					case Target.Role:
					{
						title = String.Format("Users With The Role '{0}'", role.Name);
						users = users.Where(x => x.RoleIds.Contains(role.Id));
						break;
					}
					default:
					{
						return;
					}
				}

				var desc = count ? String.Format("**Count:** `{0}`", users.Count()) : users.OrderBy(x => x.JoinedAt).FormatNumberedList("`{0}`", x => x.FormatUser());
				await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed(title, desc));
			}
		}

		[Group("getuseravatar"), Alias("gua")]
		[Usage("<User> <Number> <Gif|Png|Jpg|Webp>")]
		[Summary("Shows the URL of the given user's avatar. Can supply a format and size.")]
		[DefaultEnabled(true)]
		public sealed class GetUserAvatar : MyModuleBase
		{
			//TODO: Figure out how to make this not need 6 explicitly typed overloads
			[Command]
			public async Task Command([Optional] IUser user, [Optional] ushort size, [Optional] ImageFormat format)
			{
				await CommandRunner(user, size, format);
			}
			[Command]
			public async Task Command([Optional] IUser user, [Optional] ImageFormat format, [Optional] ushort size)
			{
				await CommandRunner(user, size, format);
			}
			[Command]
			public async Task Command([Optional] ushort size, [Optional] IUser user, [Optional] ImageFormat format)
			{
				await CommandRunner(user, size, format);
			}
			[Command]
			public async Task Command([Optional] ushort size, [Optional] ImageFormat format, [Optional] IUser user)
			{
				await CommandRunner(user, size, format);
			}
			[Command]
			public async Task Command([Optional] ImageFormat format, [Optional] IUser user, [Optional] ushort size)
			{
				await CommandRunner(user, size, format);
			}
			[Command]
			public async Task Command([Optional] ImageFormat format, [Optional] ushort size, [Optional] IUser user)
			{
				await CommandRunner(user, size, format);
			}

			private async Task CommandRunner(IUser user, ushort size = 128, ImageFormat format = ImageFormat.Auto)
			{
				await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size));
			}
		}

		[Group("getuserjoinedat"), Alias("gujat")]
		[Usage("[Number]")]
		[Summary("Shows the user which joined the guild in that position.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class GetUserJoinedAt : MyModuleBase
		{
			[Command]
			public async Task Command(uint position)
			{
				await CommandRunner(position);
			}

			private async Task CommandRunner(uint position)
			{
				var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToArray();

				var newPos = Math.Max(1, Math.Min(position, users.Length));
				var user = users[newPos - 1];
				await Messages.SendChannelMessage(Context, String.Format("`{0}` was `#{1}` to join the guild on `{2}`.", user.FormatUser(), newPos, Formatting.FormatDateTime(user.JoinedAt)));
			}
		}

		[Group("displayguilds"), Alias("dgs")]
		[Usage("")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class DisplayGuilds : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var guilds = await Context.Client.GetGuildsAsync();
				if (guilds.Count() <= 10)
				{
					var embed = Embeds.MakeNewEmbed("Guilds");
					foreach (var guild in guilds)
					{
						Embeds.AddField(embed, guild.FormatGuild(), String.Format("**Owner:** `{0}`", (await guild.GetOwnerAsync()).FormatUser()));
					}
					await Messages.SendEmbedMessage(Context.Channel, embed);
				}
				else
				{
					//This may be one of the most retarded work arounds I have ever done.
					var tempTupleList = new List<Tuple<IGuild, IGuildUser>>();
					foreach (var guild in guilds)
					{
						tempTupleList.Add(new Tuple<IGuild, IGuildUser>(guild, await guild.GetOwnerAsync()));
					}
					var desc = tempTupleList.FormatNumberedList("`{0}` Owner: `{1}`", x => x.Item1.FormatGuild(), x => x.Item2.FormatUser());
					await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed("Guilds", desc));
				}
			}
		}

		[Group("displayuserjoinlist"), Alias("dujl")]
		[Usage("")]
		[Summary("Lists most of the users who have joined the guild.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class DisplayUserJoinList : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToArray();

				var text = users.FormatNumberedList("`{0}` joined on `{1}`", x => x.FormatUser(), x => Formatting.FormatDateTime(x.JoinedAt));
				await Uploads.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "User_Joins_");
			}
		}

		[Group("displayemotes"), Alias("de")]
		[Usage("[Global|Guild]")]
		[Summary("Lists the emotes in the guild. As of right now, there's no way to upload or remove emotes through Discord's API.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class DisplayEmotes : MyModuleBase
		{
			[Command]
			public async Task Command(EmoteType target)
			{
				await CommandRunner(target);
			}

			private async Task CommandRunner(EmoteType target)
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
					: String.Format("This guild has no `{0}` emotes.", target.EnumName());
				await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed("Emotes", desc));
			}
		}

		[Group("downloadmessages"), Alias("dlm")]
		[Usage("[Number] <Channel>")]
		[Summary("Downloads the past x amount of messages. Up to 1000 messages or 500KB worth of formatted text.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class DownloadMessages : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(int num, [Optional, VerifyObject(true, ObjectVerification.CanBeRead)] ITextChannel channel)
			{
				await CommandRunner(num, channel);
			}

			private async Task CommandRunner(int num, ITextChannel channel)
			{
				var charCount = 0;
				var formattedMessages = new List<string>();
				foreach (var msg in (await Messages.GetMessages(channel, Math.Min(num, 1000))).OrderBy(x => x.CreatedAt.Ticks))
				{
					var temp = Formatting.RemoveMarkdownChars(Formatting.FormatNonDM(msg), true);
					if ((charCount += temp.Length) < Context.BotSettings.MaxMessageGatherSize)
					{
						formattedMessages.Add(temp);
					}
					else
					{
						break;
					}
				}

				await Uploads.WriteAndUploadTextFile(Context.Guild, Context.Channel,
					String.Join("\n-----\n", formattedMessages),
					String.Format("{0}_Messages", channel.Name),
					String.Format("Successfully got `{0}` messages", formattedMessages.Count));
			}
		}

		[Group("makeanembed"), Alias("mae")]
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
				await CommandRunner(input);
			}

			private async Task CommandRunner(string input)
			{
				var returnedArgs = Gets.GetArgs(Context, input, 0, 100, new[] { "title", "desc", "img", "url", "thumb", "author", "authoricon", "authorurl", "foot", "footicon" });
				if (returnedArgs.Reason != FailureReason.NotFailure)
				{
					await Messages.HandleArgsGettingErrors(Context, returnedArgs);
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
				var colorRGB = Gets.GetVariableAndRemove(returnedArgs.Arguments, "color")?.Split('/');
				if (colorRGB != null && colorRGB.Length == 3)
				{
					const byte MAX_VAL = 255;
					if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
					{
						color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
					}
				}

				var embed = Embeds.MakeNewEmbed(title, description, color, imageURL, URL, thumbnail);
				Embeds.AddAuthor(embed, authorName, authorIcon, authorURL);
				Embeds.AddFooter(embed, footerText, footerIcon);

				//Add in the fields and text
				for (int i = 1; i < 25; ++i)
				{
					var field = Gets.GetVariableAndRemove(returnedArgs.Arguments, "field" + i);
					var fieldText = Gets.GetVariableAndRemove(returnedArgs.Arguments, "fieldtext" + i);
					//If either is null break out of this loop because they shouldn't be null
					if (field == null || fieldText == null)
						break;

					bool.TryParse(Gets.GetVariableAndRemove(returnedArgs.Arguments, "fieldinline" + i), out bool inlineBool);
					Embeds.AddField(embed, field, fieldText, inlineBool);
				}

				await Messages.SendEmbedMessage(Context.Channel, embed);
			}
		}

		[Group("mentionrole"), Alias("mnr")]
		[Usage("[Role] [Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class MentionRole : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder] string text)
			{
				await CommandRunner(role, text);
			}

			private async Task CommandRunner(IRole role, string text)
			{
				if (role.IsMentionable)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("You can already mention this role."));
				}
				else
				{
					await role.ModifyAsync(x => x.Mentionable = true);
					await Messages.SendChannelMessage(Context, String.Format("From `{0}`, {1}: {2}", Context.User.FormatUser(), role.Mention, text.Substring(0, Math.Min(text.Length, 250))));
					await role.ModifyAsync(x => x.Mentionable = false);
				}
			}
		}

		[Group("messagebotowner"), Alias("mbo")]
		[Usage("[Message]")]
		[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class MessageBotOwner : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder] string input)
			{
				await CommandRunner(input);
			}

			private async Task CommandRunner(string input)
			{
				var newMsg = String.Format("From `{0}` in `{1}`:\n```\n{2}```", Context.User.FormatUser(), Context.Guild.FormatGuild(), input.Substring(0, Math.Min(input.Length, 250)));

				var owner = await Users.GetGlobalUser(Context.Client, Context.BotSettings.BotOwnerID);
				if (owner != null)
				{
					var DMChannel = await owner.GetOrCreateDMChannelAsync();
					await Messages.SendDMMessage(DMChannel, newMsg);
				}
				else
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The owner is unable to be gotten."));
				}
			}
		}

		[Group("getpermnamesfromvalue"), Alias("getperms")]
		[Usage("[Number]")]
		[Summary("Lists all the perms that come from the given value.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public sealed class GetPermNamesFromValue : MyModuleBase
		{
			[Command]
			public async Task Command(ulong permNum)
			{
				await CommandRunner(permNum);
			}

			private async Task CommandRunner(ulong permNum)
			{
				var perms = Gets.GetPermissionNames(permNum);
				if (!perms.Any())
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The given number holds no permissions."));
				}
				else
				{
					await Messages.SendChannelMessage(Context.Channel, String.Format("The number `{0}` has the following permissions: `{1}`.", permNum, String.Join("`, `", perms)));
				}
			}
		}

		[Group("getbotdms"), Alias("gbd")]
		[Usage("<User>")]
		[Summary("Lists all the people who have sent the bot DMs or shows the DMs with a person if one is specified.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class GetBotDMs : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command([Optional] IUser user)
			{
				await CommandRunner(user);
			}

			private async Task CommandRunner(IUser user)
			{
				if (user != null)
				{
					var channel = (await Context.Client.GetDMChannelsAsync()).FirstOrDefault(x => x.Recipient?.Id == user.Id);
					if (channel == null)
					{
						await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The bot does not have a DM open with that user."));
						return;
					}

					var messages = await Messages.GetBotDMs(channel);
					if (messages.Any())
					{
						var text = String.Join("\n-----\n", Formatting.FormatDMs(messages));
						var name = String.Format("DMs_From_{0}", user.Id);
						var content = String.Format("{0} Direct Messages", messages.Count);
						await Uploads.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, name, content);
					}
					else
					{
						await channel.CloseAsync();
						await Messages.MakeAndDeleteSecondaryMessage(Context, "There are no DMs from that user. I don't know why the bot is saying there were some.");
					}
				}
				else
				{
					var users = (await Context.Client.GetDMChannelsAsync()).Select(x => x.Recipient).Where(x => x != null);

					var desc = users.Any() ? String.Format("`{0}`", String.Join("`\n`", users.OrderBy(x => x.Id).Select(x => x.FormatUser()))) : "`None`";
					await Messages.SendEmbedMessage(Context.Channel, Embeds.MakeNewEmbed("Users Who Have DMd The Bot", desc));
				}
			}
		}

		[Group("test"), Alias("t")]
		[Usage("")]
		[Summary("Mostly just makes the bot say test.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class Test : MyModuleBase
		{
			[Command]
			public async Task TestCommand()
			{
				await CommandRunner();
			}

			private event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

			private async Task CommandRunner()
			{
				CollectionChanged += SaveSettings;
				var test = new TestClass(CollectionChanged);
				var collection = test.Collection;
				collection.Add("fish");
				await Messages.SendChannelMessage(Context, "test");
			}

			private void SaveSettings(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				throw new NotImplementedException();
			}
		}
	}}