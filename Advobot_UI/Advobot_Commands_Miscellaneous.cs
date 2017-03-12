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
				if (Actions.CaseInsEquals(commandParts[1], "command"))
				{
					var text = "If you do not know what commands this bot has, type `" + Constants.BOT_PREFIX + "commands` for a list of commands.";
					await Actions.MakeAndDeleteSecondaryMessage(Context, text, 10000);
					return;
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, "[] means required information. <> means optional information. | means or.", 10000);
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
					//Find close words
					var closeHelps = new List<CloseHelp>();
					Variables.HelpList.ForEach(HelpEntry =>
					{
						//Check how close the word is to the input
						var closeness = Actions.FindCloseName(HelpEntry.Name, input);
						//Ignore all closewords greater than a difference of five
						if (closeness > 5)
							return;
						//If no words in the list already, add it
						if (closeHelps.Count < 3)
						{
							closeHelps.Add(new CloseHelp(HelpEntry, closeness));
						}
						else
						{
							//If three words in the list, check closeness value now
							foreach (var help in closeHelps)
							{
								if (closeness < help.Closeness)
								{
									closeHelps.Insert(closeHelps.IndexOf(help), new CloseHelp(HelpEntry, closeness));
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
				var embed = Actions.MakeNewEmbed("Categories",
					String.Format("Type `{0}commands [Category]` for commands from that category.\n\n{1}", Constants.BOT_PREFIX, String.Join("\n", Enum.GetNames(typeof(CommandCategory)))));
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}
			//Check if all
			else if (Actions.CaseInsEquals(input, "all"))
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
			var channel = await Actions.GetChannel(Context, input);
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
			var role = await Actions.GetRole(Context, input);
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
			var user = await Actions.GetUser(Context.Guild, input) ?? await Context.Guild.GetUserAsync(Context.User.Id);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", user.Username, user.Discriminator, user.Id));
		}
		#endregion

		#region Info
		[Command("infobot")]
		[Alias("infb")]
		[Usage("")]
		[Summary("Displays various information about the bot.")]
		public async Task BotInfo()
		{
			var span = DateTime.UtcNow.Subtract(Variables.StartupTime);

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
			Actions.AddAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.GetAvatarUrl());
			//Add the footer
			Actions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

			//First field
			var firstField = Actions.FormatLoggedThings();
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
		[Summary("Displays various information about the user. Join position is mostly accurate on a small guild, while horribly inaccurate on a large guild.")]
		public async Task UserInfo([Optional, Remainder] string input)
		{
			//Get the user
			var user = await Actions.GetUser(Context.Guild, input) ?? await Context.Guild.GetUserAsync(Context.User.Id);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Get a list of roles
			var roles = user.RoleIds.Where(x => x != Context.Guild.Id).Select(x => Context.Guild.GetRole(x));

			//Get a list of channels
			var channels = new List<string>();
			//Text channels
			(await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.ReadMessages == PermValue.Allow) || user.GetPermissions(x).ReadMessages)
				{
					channels.Add(x.Name);
				}
			});
			//Voice channels
			(await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.Connect == PermValue.Allow) || user.GetPermissions(x).Connect)
				{
					channels.Add(x.Name + " (Voice)");
				}
			});

			//Get an ordered list of when users joined the guild
			var guildUsers = await Context.Guild.GetUsersAsync();
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
			Actions.AddAuthor(embed, user.Username + "#" + user.Discriminator + " " + (user.Nickname == null ? "" : "(" + user.Nickname + ")"), user.GetAvatarUrl(), user.GetAvatarUrl());
			//Add the footer
			Actions.AddFooter(embed, "Userinfo");

			//Add the channels the user can access
			if (channels.Count() != 0)
			{
				Actions.AddField(embed, "Channels", String.Join(", ", channels));
			}
			//Add the roles the user has
			if (roles.Count() != 0)
			{
				Actions.AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
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
			//Parse out the emoji
			Emoji emoji;
			if (!Emoji.TryParse(input, out emoji))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid emoji supplied."));
				return;
			}

			//Try to find the emoji if global
			var guilds = (await Context.Client.GetGuildsAsync()).Where(x =>
			{
				var placeholder = x.Emojis.FirstOrDefault(y => y.Id == emoji.Id);
				return placeholder.IsManaged && placeholder.RequireColons;
			});

			//Format a description
			var description = String.Format("**ID:** `{0}`\n", emoji.Id);
			if (guilds.Any())
			{
				description += String.Format("**From:** `{0}`", String.Join("`, `", guilds.Select(x => x.Name)));
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(emoji.Name, description, thumbnailURL: emoji.Url));
		}

		[Command("infoinvite")]
		[Alias("infi")]
		[Usage("[Invite Code]")]
		[Summary("Lists the user who created the invite, the channel it was created on, the uses, and the creation date/time.")]
		[UserHasAPermission]
		public async Task InviteInfo([Remainder] string input)
		{
			//Get the invite
			var inv = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			//Check if null
			if (inv == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite cannot be gotten."));
				return;
			}
			//Get all of the variables
			var user = Actions.FormatUser(inv.Inviter);
			var channel = Actions.FormatChannel(await Context.Guild.GetChannelAsync(inv.ChannelId));
			var uses = inv.Uses;
			var time = inv.CreatedAt.UtcDateTime.ToShortTimeString();
			var date = inv.CreatedAt.UtcDateTime.ToShortDateString();
			//Make the embed
			var embed = Actions.MakeNewEmbed(inv.Code, String.Format("**Inviter:** `{0}`\n**Channel:** `{1}`\n**Uses:** `{2}`\n**Created At:** `{3}` on `{4}`", user, channel, uses, time, date));
			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("useravatar")]
		[Alias("uav")]
		[Usage("[Gif|Png|Jpg|Webp] <@user>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily). Currently every avatar is displayed with an extension type of gif.")]
		public async Task UserAvatar([Optional, Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Get the type of image
			AvatarFormat format;
			if (!Enum.TryParse(inputArray[0], true, out format))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid avatar format supplied."));
				return;
			}

			//Get the user
			var user = Context.User as IGuildUser;
			if (inputArray.Length == 2)
			{
				user = await Actions.GetUser(Context.Guild, inputArray[1]);
				if (user == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
					return;
				}
			}

			//Send a message with the URL
			await Context.Channel.SendMessageAsync(user.GetAvatarUrl(format));
		}

		[Command("userjoins")]
		[Alias("ujs")]
		[Usage("")]
		[Summary("Lists most of the users who have joined the guild. Will not list users who are offline on a large guild, sadly.")]
		public async Task UserJoins()
		{
			//Grab the users and format the message
			var counter = 1;
			var userMsg = String.Join("\n", (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt.HasValue).OrderBy(x => x.JoinedAt).Select(x =>
			{
				var time = x.JoinedAt.Value.UtcDateTime;
				return String.Format("`{0}.` `{1}` joined at `{2}` on `{3}`.", counter++.ToString("0000"), Actions.FormatUser(x), time.ToShortTimeString(), time.ToShortDateString());
			}));

			await Actions.SendPotentiallyBigEmbed(Context.Guild, Context.Channel, Actions.MakeNewEmbed("Users", userMsg), userMsg, "User_Joins_");
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage("[Position]")]
		[Summary("Shows the user which joined the guild in that position. Mostly accurate on a small guild, while horribly inaccurate on a large guild.")]
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
		[Usage("[Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Upload specifies to use a text uploader.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task UsersWithRole([Remainder] string input)
		{
			//Initializing input and variables
			var role = await Actions.GetRole(Context, input);
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//Grab each user
			var users = "";
			var count = 1;
			(await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt.HasValue).ToList().OrderBy(x => x.JoinedAt).ToList().ForEach(x =>
			{
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					users += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x));
				}
			});

			//Checking if the message can fit in a single message
			var roleName = role.Name.Substring(0, 3) + Constants.ZERO_LENGTH_CHAR + role.Name.Substring(3);
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(roleName, users));
		}

		[Command("userswithname")]
		[Alias("uwn")]
		[Usage("[Name]")]
		[Summary("Lists all users where their username contains the given string.")]
		[UserHasAPermission]
		public async Task UsersWithName([Remainder] string input)
		{
			//Find the users
			var users = (await Context.Guild.GetUsersAsync()).Where(x => Actions.CaseInsIndexOf(x.Username, input)).OrderBy(x => x.JoinedAt).ToList();

			//Initialize the string
			var description = "";

			//Add them to the string
			var count = 1;
			users.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x));
			});

			//Set the title
			var title = String.Format("Users With Names Containing '{0}'", input);

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
		[Command("invitelist")]
		[Alias("invl")]
		[Usage("")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[UserHasAPermission]
		public async Task ListInstantInvites()
		{
			//Get the invites
			var invites = (await Context.Guild.GetInvitesAsync()).OrderBy(x => x.Uses).Reverse().ToList();

			//Make sure there are some invites
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
				return;
			}

			//Format the description
			var description = "";
			var count = 1;
			var lengthForCount = invites.Count.ToString().Length;
			var lengthForCode = invites.Max(x => x.Code.Length);
			var lengthForUses = invites.Max(x => x.Uses).ToString().Length;
			invites.ForEach(x =>
			{
				var cnt = count++.ToString().PadLeft(lengthForCount, '0');
				var code = x.Code.PadRight(lengthForCode);
				var uses = x.Uses.ToString().PadRight(lengthForUses);
				description += String.Format("`{0}.` `{1}` `{2}` `{3}`\n", cnt, code, uses, x.Inviter.Username);
			});

			//Send a success message
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Instant Invite List", description));
		}

		[Command("invitecreate")]
		[Alias("invc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirement(1U << (int)GuildPermission.CreateInstantInvite)]
		public async Task CreateInstantInvite([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 4);
			if (inputArray.Length != 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check validity of channel
			var channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//Set the time in seconds
			int time = 0;
			int? nullableTime = null;
			if (int.TryParse(inputArray[1], out time))
			{
				int[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
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
			if (Actions.CaseInsEquals(inputArray[3], "true"))
			{
				tempMembership = true;
			}

			//Make into valid invite link
			var inv = await channel.CreateInviteAsync(nullableTime, nullableUsers, tempMembership);

			await Actions.SendChannelMessage(Context, String.Format("Here is your invite for `{0}`: {1} \nIt will last for{2}, {3}{4}.",
				Actions.FormatChannel(channel),
				inv.Url,
				nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : "and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership" : ""));
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
			await invites.ForEachAsync(async x => await x.DeleteAsync());

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", count));
		}
		#endregion

		#region Miscellaneous
		[Command("makeanembed")]
		[Alias("mae")]
		[Usage("\"Title:input\" \"Desc:input\" Img:url Url:url Thumb:url Color:int/int/int \"Auth:input\" AuthIcon:url AuthUrl:url \"Foot:input\" FootIcon:url " +
				"\"Field[1-25]:input\" \"FieldText[1-25]:input\" FieldInline[1-10]:true|false ")]
		[Summary("Every single piece is optional. The stuff in quotes *must* be in quotes. URLs need the https:// in front. Fields need *both* Field and FieldText to work.")]
		public async Task MakeAnEmbed([Remainder] string input)
		{
			//Split the input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ').ToList();

			//Get the inputs
			var title = Actions.GetVariableAndRemove(inputArray, "title");
			var description = Actions.GetVariableAndRemove(inputArray, "desc");
			var imageURL = Actions.GetVariableAndRemove(inputArray, "Img");
			var URL = Actions.GetVariableAndRemove(inputArray, "url");
			var thumbnail = Actions.GetVariableAndRemove(inputArray, "thumb");
			var authorName = Actions.GetVariableAndRemove(inputArray, "auth");
			var authorIcon = Actions.GetVariableAndRemove(inputArray, "authicon");
			var authorURL = Actions.GetVariableAndRemove(inputArray, "authurl");
			var footerText = Actions.GetVariableAndRemove(inputArray, "foot");
			var footerIcon = Actions.GetVariableAndRemove(inputArray, "footicon");

			//Get the color
			var color = Constants.BASE;
			var colorRGB = Actions.GetVariableAndRemove(inputArray, "color")?.Split('/');
			if (colorRGB != null && colorRGB.Length == 3)
			{
				byte r, g, b;
				const byte MAX_VAL = 255;
				if (byte.TryParse(colorRGB[0], out r) && byte.TryParse(colorRGB[1], out g) && byte.TryParse(colorRGB[2], out b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}

			//Make the embed
			var embed = Actions.MakeNewEmbed(title, description, color, imageURL, URL, thumbnail);
			//Add in the author
			Actions.AddAuthor(embed, authorName, authorIcon, authorURL);
			//Add in the footer
			Actions.AddFooter(embed, footerText, footerIcon);

			//Add in the fields and text
			for (int i = 1; i < 25; i++)
			{
				//Get the input for fields
				var field = Actions.GetVariableAndRemove(inputArray, "field" + i);
				var fieldText = Actions.GetVariableAndRemove(inputArray, "fieldtext" + i);
				//If either is null break out of this loop because they shouldn't be null
				if (field == null || fieldText == null)
					break;

				//Get the bool for the field
				var inlineBool = true;
				bool.TryParse(Actions.GetVariableAndRemove(inputArray, "fieldinline" + i), out inlineBool);

				//Add in the field
				Actions.AddField(embed, field, fieldText, inlineBool);
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage("[Role]/[Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserHasAPermission]
		public async Task MentionRole([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Get the role and see if it can be changed
			var role = await Actions.GetRoleEditAbility(Context, inputArray[0]);
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
			if (Actions.CaseInsEquals(input, "guild"))
			{
				//Get all of the guild emojis
				Context.Guild.Emojis.Where(x => !x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else if (Actions.CaseInsEquals(input, "global"))
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