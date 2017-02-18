using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Miscellaneous commands are random commands that don't exactly fit the other groups
	[Name("Miscellaneous")]
	public class Miscellaneous_Commands : ModuleBase
	{
		#region Help
		[Command("help")]
		[Alias("h", "info")]
		[Usage("<Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		public async Task Help([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				//Description string
				var text = "Type `" + Properties.Settings.Default.Prefix + "commands` for the list of commands.\nType `" + Properties.Settings.Default.Prefix + "help [Command]` for help with a command.";

				//Make the embed
			    var embed = Actions.MakeNewEmbed("Commands", text);
				//Add the first field
				Actions.AddField(embed, "Syntax", "[] means required.\n<> means optional.\n| means or.");
				//Add the second field
				Actions.AddField(embed, "Current Repo", "[Advobot](https://github.com/advorange/Advobot)");
				//Add the footer
				Actions.AddFooter(embed, "Help");
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Get the input command, if nothing then link to documentation
			var commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].Equals("command", StringComparison.OrdinalIgnoreCase))
				{
					var text = "If you do not know what commands this bot has, type `" + Constants.BOT_PREFIX + "commands` for a list of commands.";
					await Actions.MakeAndDeleteSecondaryMessage(Context, text, 10000);
					return;
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, "[] means required information. <> means optional information. | means or.", 10000);
				return;
			}

			//Send the message for that command
			var helpEntry = Variables.HelpList.FirstOrDefault(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
			if (helpEntry == null)
			{
				foreach (var command in Variables.HelpList)
				{
					if (command.Aliases.Contains(input))
					{
						helpEntry = command;
					}
				}
				if (helpEntry == null)
				{
					//Find close words
					var closeHelps = new List<CloseHelp>();
					Variables.HelpList.ForEach(x =>
					{
						//Check how close the word is to the input
						var closeness = Actions.FindCloseName(x.Name, input);
						//Ignore all closewords greater than a difference of five
						if (closeness > 5)
							return;
						//If no words in the list already, add it
						if (closeHelps.Count < 3)
						{
							closeHelps.Add(new CloseHelp(x, closeness));
						}
						else
						{
							//If three words in the list, check closeness value now
							foreach (var closeHelp in closeHelps)
							{
								if (closeness < closeHelp.Closeness)
								{
									closeHelps.Insert(closeHelps.IndexOf(closeHelp), new CloseHelp(x, closeness));
									break;
								}
							}

							//Remove all words that are now after the third item
							closeHelps.RemoveRange(3, closeHelps.Count - 3);
						}
						closeHelps.OrderBy(y => y.Closeness);
					});

					closeHelps = Actions.GetCommandsWithInputInName(closeHelps, input);

					if (closeHelps != null && closeHelps.Any())
					{
						//Format a message to be said
						int counter = 1;
						var msg = "Did you mean any of the following:\n" + String.Join("\n", closeHelps.Select(x => String.Format("`{0}.` {1}", counter++.ToString("00"), x.Help.Name)));

						//Remove all active closeword lists that the user has made
						Variables.ActiveCloseHelp.RemoveAll(x => x.User == Context.User);

						//Create the list
						var list = new ActiveCloseHelp(Context.User as IGuildUser, closeHelps);

						//Add them to the active close word list, thus allowing them to say the number of the remind they want. Remove after 5 seconds
						Variables.ActiveCloseHelp.Add(list);
						Actions.RemoveActiveCloseHelp(list);

						//Send the message
						await Actions.MakeAndDeleteSecondaryMessage(Context, msg, 10000);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent command."));
					}
					return;
				}
			}

			var description = Actions.GetHelpString(helpEntry);

			var guildPrefix = Variables.Guilds[Context.Guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				description = description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.AddFooter(Actions.MakeNewEmbed(helpEntry.Name, description), "Help"));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage("<Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		public async Task Commands([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				var embed = Actions.MakeNewEmbed("Commands", "Type `" + Constants.BOT_PREFIX + "commands [Category]` for commands from that category.");
				var catString = String.Join("\n", Enum.GetNames(typeof(CommandCategory)));
				await Actions.SendEmbedMessage(Context.Channel, Actions.AddField(embed, "Categories", catString));
				return;
			}
			//Check if all
			else if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("ALL", String.Join("\n", Variables.CommandNames)));
				return;
			}

			//Check what category
			CommandCategory category;
			if (!Enum.TryParse(input, true, out category))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."));
				return;
			}

			//Send the message
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(Enum.GetName(typeof(CommandCategory), category), String.Join("\n", Actions.GetCommands(Context.Guild, (int)category))));
		}
		#endregion

		#region IDs
		[Command("idguild")]
		[Alias("idg")]
		[Usage("")]
		[Summary("Shows the ID of the guild.")]
		[UserHasAPermission]
		public async Task ServerID()
		{
			await Actions.SendChannelMessage(Context, String.Format("This guild has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("idchannel")]
		[Alias("idc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		[UserHasAPermission]
		public async Task ChannelID([Remainder] string input)
		{
			IGuildChannel channel = await Actions.GetChannel(Context, input);
			if (channel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.GetChannelType(channel), channel.Name, channel.Id));
		}

		[Command("idrole")]
		[Alias("idr")]
		[Usage("[Role]")]
		[Summary("Shows the ID of the given role.")]
		[UserHasAPermission]
		public async Task RoleID([Remainder] string input)
		{
			IRole role = await Actions.GetRole(Context, input);
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("iduser")]
		[Alias("idu")]
		[Usage("<@User>")]
		[Summary("Shows the ID of the given user.")]
		[UserHasAPermission]
		public async Task UserID([Optional, Remainder] string input)
		{
			IGuildUser user = input == null ? Context.User as IGuildUser : await Actions.GetUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", user.Username, user.Discriminator, user.Id));
		}
		#endregion

		#region User or other misc info
		[Command("infobot")]
		[Alias("infb")]
		[Usage("")]
		[Summary("Displays various information about the bot.")]
		public async Task BotInfo()
		{
			TimeSpan span = DateTime.UtcNow.Subtract(Variables.StartupTime);

			//Make the description
			var description = String.Format(
				"Online since: {0}\n" +
				"Uptime: {1}:{2}:{3}:{4}\n" +
				"Guild count: {5}\n" +
				"Cumulative member count: {6}\n",
				Variables.StartupTime,
				span.Days, span.Hours.ToString("00"),
				span.Minutes.ToString("00"),
				span.Seconds.ToString("00"),
				Variables.TotalGuilds,
				Variables.TotalUsers);

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, description);
			//Add the author
			Actions.AddAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.AvatarUrl);
			//Add the footer
			Actions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

			//First field
			var firstField = String.Format(
				"Logged joins: {0}\n" +
				"Logged leaves: {1}\n" +
				"Logged bans: {2}\n" +
				"Logged unbans: {3}\n" +
				"Logged user changes: {4}\n" +
				"Logged edits: {5}\n" +
				"Logged deletes: {6}\n" +
				"Logged images: {7}\n" +
				"Logged gifs: {8}\n" +
				"Logged files: {9}\n",
				Variables.LoggedJoins,
				Variables.LoggedLeaves,
				Variables.LoggedBans,
				Variables.LoggedUnbans,
				Variables.LoggedUserChanges,
				Variables.LoggedEdits,
				Variables.LoggedDeletes,
				Variables.LoggedImages,
				Variables.LoggedGifs,
				Variables.LoggedFiles);
			Actions.AddField(embed, "Logged Actions", firstField);

			//Second field
			var secondField = String.Format(
				"Attempted commands: {0}\n" +
				"Successful commands: {1}\n" +
				"Failed commands: {2}\n",
				Variables.AttemptedCommands,
				Variables.AttemptedCommands - Variables.FailedCommands,
				Variables.FailedCommands);
			Actions.AddField(embed, "Commands", secondField);

			//Third field
			var thirdField = String.Format(
				"Latency: {0}ms\n" +
				"Memory usage: {1:0.00}MB\n" +
				"Thread count: {2}\n",
				Variables.Client.GetLatency(),
				Process.GetCurrentProcess().WorkingSet64 / 1000000.0,
				Process.GetCurrentProcess().Threads.Count);
			Actions.AddField(embed, "Technical", thirdField);

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infouser")]
		[Alias("infu")]
		[Usage("<@User>")]
		[Summary("Displays various information about the user. Join position is mostly accurate, give or take ten places per thousand users on the guild.")]
		public async Task UserInfo([Optional, Remainder] string input)
		{
			IGuildUser user = null;

			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				user = await Context.Guild.GetUserAsync(Context.User.Id);
			}
			else
			{
				user = await Actions.GetUser(Context.Guild, input);
			}

			//Check if valid user
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Get a list of roles
			var roles = new List<string>();
			foreach (UInt64 roleID in user.RoleIds)
			{
				roles.Add(Context.Guild.GetRole(roleID).Name);
			}
			roles.Remove(Context.Guild.EveryoneRole.Name);

			//Get a list of channels
			var channels = new List<string>();
			//Text channels
			(await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(async x =>
			{
				using (var channelUsers = x.GetUsersAsync().GetEnumerator())
				{
					while (await channelUsers.MoveNext())
					{
						if (channelUsers.Current.Contains(user))
						{
							channels.Add(x.Name);
							break;
						}
					}
				}
			});
			//Voice channels
			(await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (user.GetPermissions(x).Connect)
				{
					channels.Add(x.Name + " (Voice)");
				}
			});

			//Get an ordered list of when users joined the guild
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			//Make the description
			var description = String.Format(
				"ID: {0}\n" +
				"Created: {1} {2}, {3} at {4}\n" +
				"Joined: {5} {6}, {7} at {8} (#{9} to join the guild)\n" +
				"\n" +
				"Current game: {10}\n" +
				"Online status: {11}\n",
				user.Id,
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.CreatedAt.Month),
				user.CreatedAt.UtcDateTime.Day,
				user.CreatedAt.UtcDateTime.Year,
				user.CreatedAt.UtcDateTime.ToLongTimeString(),
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
				user.JoinedAt.Value.UtcDateTime.Day,
				user.JoinedAt.Value.UtcDateTime.Year,
				user.JoinedAt.Value.UtcDateTime.ToLongTimeString(),
				users.IndexOf(user) + 1,
				user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
				user.Status);

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, description);
			//Add the author
			Actions.AddAuthor(embed, user.Username + "#" + user.Discriminator + " " + (user.Nickname == null ? "" : "(" + user.Nickname + ")"), user.AvatarUrl, user.AvatarUrl);
			//Add the footer
			Actions.AddFooter(embed, "Userinfo");

			//Add the channels the user can access
			if (channels.Count != 0)
			{
				Actions.AddField(embed, "Channels", String.Join(", ", channels));
			}
			//Add the roles the user has
			if (roles.Count != 0)
			{
				Actions.AddField(embed, "Roles", String.Join(", ", roles));
			}
			//Add the voice channel
			if (user.VoiceChannel != null)
			{
				var text = String.Format("Server mute: {0}\nServer deafen: {1}\nSelf mute: {2}\nSelf deafen: {3}",
					user.IsMuted.ToString(), user.IsDeafened.ToString(), user.IsSelfMuted.ToString(), user.IsSelfDeafened.ToString());

				Actions.AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, text);
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infoemoji")]
		[Alias("infe")]
		[Usage("[Emoji]")]
		[Summary("Shows information about an emoji. Only global emojis where the bot is in a guild that gives them will have a 'From...' text.")]
		public async Task EmojiInfo([Remainder] string input)
		{
			Emoji emoji;
			if (!Emoji.TryParse(input, out emoji))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid emoji supplied."));
				return;
			}

			//Try to find the emoji
			var guild = (await Context.Client.GetGuildsAsync()).FirstOrDefault(x => x.Emojis.FirstOrDefault(y => y.Id == emoji.Id).IsManaged && x.Emojis.FirstOrDefault(y => y.Id == emoji.Id).RequireColons);

			//Format a description
			var description = "ID: `" + emoji.Id + "`\n";
			if (guild != null)
			{
				description += "From: `" + guild.Name + "`";
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(emoji.Name, description, null, emoji.Url));
		}

		[Command("useravatar")]
		[Alias("uav")]
		[Usage("<@user>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily). Currently every avatar is displayed with an extension type of gif.")]
		public async Task UserAvatar([Optional, Remainder] string input)
		{
			var user = input == null ? Context.User as IGuildUser : await Actions.GetUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Context.Channel.SendMessageAsync(user.AvatarUrl.Replace(".jpg", ".gif"));
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage("[Position]")]
		[Summary("Shows the user which joined the guild in that position. Mostly accurate, give or take ten places per thousand users on the guild.")]
		public async Task UserJoinedAt([Remainder] string input)
		{
			int position;
			if (Int32.TryParse(input, out position))
			{
				var guildUsers = await Context.Guild.GetUsersAsync();
				var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				if (position >= 1 && position < users.Count)
				{
					var user = users[position - 1];
					await Actions.SendChannelMessage(Context, String.Format("`{0}#{1}` was #{2} to join the guild on `{3} {4}, {5}` at `{6}`.",
						user.Username,
						user.Discriminator,
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

		[Command("userswithrole")]
		[Alias("uwr")]
		[Usage("<File|Upload> [Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Upload specifies to use a text uploader.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task AllWithRole([Remainder] string input)
		{
			//Split into the bools and role
			var values = input.Split(new char[] { ' ' }, 2).ToList();

			//Initializing input and variables
			var role = await Actions.GetRole(Context, values.Last());
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//If two args, check what action to take
			var overwriteBool = false;
			var textFileBool = false;
			if (values.Count == 2)
			{
				if (values[0].Equals("file", StringComparison.OrdinalIgnoreCase))
				{
					textFileBool = true;
					overwriteBool = true;
				}
				else if (values[0].Equals("upload", StringComparison.OrdinalIgnoreCase))
				{
					overwriteBool = true;
				}
			}

			//Initialize the lists
			var usersMentions = new List<string>();
			var usersText = new List<string>();
			var characters = 0;
			var count = 1;

			//Grab each user
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			guildUsers.Where(x => x.JoinedAt != null).ToList().OrderBy(x => x.JoinedAt.Value.Ticks).ToList().ForEach(x =>
			{
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					var text = "`" + x.Username + "#" + x.Discriminator + "`";
					usersMentions.Add(text);
					usersText.Add("`" + count++.ToString("00") + ".` " + x.Username + "#" + x.Discriminator + " ID: " + x.Id);
					characters += text.Length + 3;
				}
			});

			//Checking if the message can fit in a single message
			var roleName = role.Name.Substring(0, 3) + Constants.ZERO_LENGTH_CHAR + role.Name.Substring(3);
			if (characters > 1000 || overwriteBool)
			{
				var info = Actions.ReplaceMarkdownChars(String.Join("\n", usersText));
				if (!textFileBool)
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(roleName, Actions.UploadToHastebin(info)));
					return;
				}
				//Upload the file
				await Actions.UploadTextFile(Context.Guild, Context.Channel, info, roleName + "_" + role.Name.ToUpper() + "_", roleName);
			}
			else
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(roleName, String.Join(", ", usersMentions)));
			}
		}

		[Command("userswithname")]
		[Alias("uwn")]
		[Usage("[Name]")]
		[Summary("Lists all users where their username contains the given string.")]
		[UserHasAPermission]
		public async Task ListUsersWithName([Remainder] string input)
		{
			//Find the users
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.JoinedAt).ToList();

			//Initialize the string
			var description = "";

			//Add them to the string
			var count = 1;
			users.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}#{2}` ID: `{3}`\n", count++.ToString("00"), x.Username, x.Discriminator, x.Id);
			});

			//Set the title
			var title = String.Format("Users With Names Containing '{0}'", input);

			//See if the string length is over the check amount
			if (description.Length > Constants.LENGTH_CHECK)
			{
				if (!Constants.TEXT_FILE)
				{
					//Upload the embed with the hastebin links
					var uploadEmbed = Actions.MakeNewEmbed(title, Actions.UploadToHastebin(Actions.ReplaceMarkdownChars(description)));
					await Actions.SendEmbedMessage(Context.Channel, uploadEmbed);
					return;
				}
				else
				{
					//Upload a file that is deleted after upload
					await Actions.UploadTextFile(Context.Guild, Context.Channel, Actions.ReplaceMarkdownChars(description), "List_Users_With_Name_", title);
					return;
				}
			}

			//Make and send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, description));
		}

		[Command("currentmembercount")]
		[Alias("cmc")]
		[Usage("")]
		[Summary("Shows the current number of members in the guild.")]
		public async Task CurrentMemberCount()
		{
			await Actions.SendChannelMessage(Context, String.Format("The current member count is `{0}`.", (Context.Guild as SocketGuild).MemberCount));
		}
		#endregion

		#region Instant Invites
		[Command("invitecreate")]
		[Alias("invc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirement(1U << (int)GuildPermission.CreateInstantInvite)]
		public async Task CreateInstantInvite([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 4);
			if (inputArray.Length != 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check validity of channel
			IGuildChannel channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//Set the time in seconds
			Int32 time = 0;
			Int32? nullableTime = null;
			if (Int32.TryParse(inputArray[1], out time))
			{
				Int32[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
				if (validTimes.Contains(time))
				{
					nullableTime = time;
				}
			}

			//Set the max amount of users
			int users = 0;
			int? nullableUsers = null;
			if (int.TryParse(inputArray[2], out users))
			{
				int[] validUsers = { 1, 5, 10, 25, 50, 100 };
				if (validUsers.Contains(users))
				{
					nullableUsers = users;
				}
			}

			//Set tempmembership
			bool tempMembership = false;
			if (inputArray[3].Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				tempMembership = true;
			}

			//Make into valid invite link
			IInvite inv = await channel.CreateInviteAsync(nullableTime, nullableUsers, tempMembership);

			await Actions.SendChannelMessage(Context, String.Format("Here is your invite for `{0} ({1})`: {2} \nIt will last for{3}, {4}{5}.",
				channel.Name,
				Actions.GetChannelType(channel),
				inv.Url,
				nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : "and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership" : ""));
		}

		[Command("invitelist")]
		[Alias("invl")]
		[Usage("")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ListInstantInvites()
		{
			//Format the description
			int count = 1;
			var description = "";
			(await Context.Guild.GetInvitesAsync()).OrderBy(x => x.Uses).ToList().ForEach(x =>
			{
				var code = x.Code.Length < 7 ? (x.Code + "       ").Substring(0, 7) : x.Code;
				description += String.Format("`{0}.` `{1}` `{2}` `{3}`\n", count++.ToString("000"), code, x.Uses, x.Inviter.Username);
			});

			//Send a success message
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Instant Invites", description));
		}

		[Command("invitedelete")]
		[Alias("invd")]
		[Usage("[Invite Code]")]
		[Summary("Deletes the invite with the given code.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteInstantInvite([Remainder] string input)
		{
			//Get the input
			var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			if (invite == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite doesn't exist."));
				return;
			}

			//Delete the invite and send a success message
			await invite.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the invite `{0}`.", invite.Code));
		}

		[Command("invitedeletemultiple")]
		[Alias("invdm")]
		[Usage("[@User|" + Constants.CHANNEL_INSTRUCTIONS + "|Uses:Number|Expires:Number]")]
		[Summary("Deletes all invites satisfying the given condition of either user, creation channel, uses, or expiry time.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteMultipleInvites([Remainder] string input)
		{
			//Set the action telling what variable
			DeleteInvAction? action = null;

			//Check if user
			var user = await Actions.GetUser(Context.Guild, input);
			if (user != null)
			{
				action = DeleteInvAction.User;
			}

			//Check if channel
			IGuildChannel channel = null;
			if (action == null)
			{
				channel = await Actions.GetChannelEditAbility(Context, input, true);
				if (channel != null)
				{
					action = DeleteInvAction.Channel;
				}
			}

			//Check if uses
			int uses = 0;
			if (action == null)
			{
				var usesString = Actions.GetVariable(input, "uses");
				if (int.TryParse(usesString, out uses))
				{
					action = DeleteInvAction.Uses;
				}
			}

			//Check if expiry time
			int expiry = 0;
			if (action == null)
			{
				var expiryString = Actions.GetVariable(input, "expires");
				if (int.TryParse(expiryString, out expiry))
				{
					action = DeleteInvAction.Expiry;
				}
			}

			//Have gone through every other check so it's an error at this point
			if (action == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid target supplied."));
				return;
			}

			//Get the guild's invites
			var guildInvites = await Context.Guild.GetInvitesAsync();
			//Check if the amount is greater than zero
			if (!guildInvites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
				return;
			}

			//Make a new list to store the invites that match the conditions in
			var invites = new List<IInvite>();

			//Follow through with the action
			switch (action)
			{
				case DeleteInvAction.User:
				{
					invites.AddRange(guildInvites.Where(x => x.Inviter.Id == user.Id));
					break;
				}
				case DeleteInvAction.Channel:
				{
					invites.AddRange(guildInvites.Where(x => x.ChannelId == channel.Id));
					break;
				}
				case DeleteInvAction.Uses:
				{
					invites.AddRange(guildInvites.Where(x => x.Uses == uses));
					break;
				}
				case DeleteInvAction.Expiry:
				{
					invites.AddRange(guildInvites.Where(x => x.MaxAge == expiry));
					break;
				}
			}

			//Check if any invites were gotten
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invites satisfied the given condition."));
				return;
			}

			//Get the count of how many invites matched the condition
			var count = invites.Count;

			//Delete the invites
			invites.ForEach(async x => await x.DeleteAsync());

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", count));
		}
		#endregion

		#region Miscellaneous
		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage("[Role]/[Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserHasAPermission]
		public async Task MentionRole([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Get the role and see if it can be changed
			IRole role = await Actions.GetRoleEditAbility(Context, inputArray[0]);
			if (role == null)
				return;

			//See if people can already mention the role
			if (role.IsMentionable)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You can already mention this role."));
				return;
			}

			//Make the role mentionable
			await role.ModifyAsync(x => x.Mentionable = true);
			//Send the message
			await Actions.SendChannelMessage(Context, role.Mention + ": " + inputArray[1]);
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("listemojis")]
		[Alias("lemojis")]
		[Usage("[Global|Guild]")]
		[Summary("Lists the emoji in the guild. As of right now, with the current API wrapper version this bot uses, there's no way to upload or remove emojis yet; sorry.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageEmojis)]
		public async Task ListEmojis([Remainder] string input)
		{
			//Make the string
			string description = null;

			//Add the emojis to the string
			int count = 1;
			if (input.Equals("guild", StringComparison.OrdinalIgnoreCase))
			{
				//Get all of the guild emojis
				Context.Guild.Emojis.Where(x => !x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else if (input.Equals("global", StringComparison.OrdinalIgnoreCase))
			{
				//Get all of the global emojis
				Context.Guild.Emojis.Where(x => x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option."));
				return;
			}

			//Check if the description is still null
			description = description ?? String.Format("This guild has no {0} emojis.", input.ToLower());

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Emojis", description));
		}

		[Command("test")]
		[BotOwnerRequirement]
		public async Task Test([Optional, Remainder] string input)
		{
			await Actions.MakeAndDeleteSecondaryMessage(Context, "test");
		}
		#endregion
	}
}