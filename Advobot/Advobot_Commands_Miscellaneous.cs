using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Advobot
{
	//Miscellaneous commands are random commands that don't exactly fit the other groups
	[Name("Miscellaneous")]
	public class Miscellaneous_Commands : ModuleBase
	{
		#region Help
		[Command("help")]
		[Alias("h", "info")]
		[Usage("help <Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		[UserHasAPermission]
		public async Task Help([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				//Description string
				var text = "Type `" + Properties.Settings.Default.Prefix + "commands` for the list of commands.\nType `" + Properties.Settings.Default.Prefix + "help [Command]` for help with a command.";

				//Make the embed
			    var embed = Actions.makeNewEmbed("Commands", text);
				//Add the first field
				Actions.addField(embed, "Syntax", "[] means required.\n<> means optional.\n| means or.");
				//Add the second field
				Actions.addField(embed, "Current Repo", "[Advobot](https://github.com/advorange/Advobot)");
				//Add the footer
				Actions.addFooter(embed, "Help");
				await Actions.sendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Get the input command, if nothing then link to documentation
			var commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].Equals("command", StringComparison.OrdinalIgnoreCase))
				{
					var text = "If you do not know what commands this bot has, type `" + Constants.BOT_PREFIX + "commands` for a list of commands.";
					await Actions.makeAndDeleteSecondaryMessage(Context, text, 10000);
					return;
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, "[] means required information. <> means optional information. | means or.", 10000);
				return;
			}

			//Send the message for that command
			HelpEntry helpEntry = Variables.HelpList.FirstOrDefault(x => x.Name.Equals(input));
			if (helpEntry == null)
			{
				foreach (HelpEntry commands in Variables.HelpList)
				{
					if (commands.Aliases.Contains(input))
					{
						helpEntry = commands;
					}
				}
				if (helpEntry == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent command."));
					return;
				}
			}

			var description = String.Format("**Aliases:** {0}\n**Usage:** {1}\n\n**Base Permission(s):**\n{2}\n\n**Description:**\n{3}",
				String.Join(", ", helpEntry.Aliases),
				helpEntry.Usage,
				helpEntry.basePerm,
				helpEntry.Text);

			var guildPrefix = Variables.Guilds[Context.Guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				description = description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			await Actions.sendEmbedMessage(Context.Channel, Actions.addFooter(Actions.makeNewEmbed(helpEntry.Name, description), "Help"));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage("commands <Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[UserHasAPermission]
		public async Task Commands([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				var embed = Actions.makeNewEmbed("Commands", "Type `" + Constants.BOT_PREFIX + "commands [Category]` for commands from that category.");
				await Actions.sendEmbedMessage(Context.Channel, Actions.addField(embed, "Categories", "Administration\nModeration\nMiscellaneous\nAll"));
				return;
			}

			Actions.loadPreferences(Context.Guild);

			bool allCommandsBool = true;
			var section = input.ToLower();

			if (section.Equals("administration"))
			{
				var commands = Actions.getCommands(Context.Guild, (int)CommandCategory.Administration);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("ADMINISTRATION", String.Join("\n", commands)));
			}
			else if (section.Equals("moderation"))
			{
				var commands = Actions.getCommands(Context.Guild, (int)CommandCategory.Moderation);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("MODERATION", String.Join("\n", commands)));
			}
			else if (section.StartsWith("misc"))
			{
				var commands = Actions.getCommands(Context.Guild, (int)CommandCategory.Miscellaneous);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("MISCELLANEOUS", String.Join("\n", commands)));
			}
			else if (section.Equals("all"))
			{
				if (allCommandsBool)
				{
					var commands = new List<string>();
					foreach (string command in Variables.CommandNames)
					{
						commands.Add(command);
					}
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("ALL", String.Join("\n", commands)));
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "All is currently turned off.");
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."));
			}
		}
		#endregion

		#region IDs
		[Command("guildid")]
		[Alias("gid", "serverid", "sid")]
		[Usage("guildid")]
		[Summary("Shows the ID of the guild.")]
		[UserHasAPermission]
		public async Task ServerID()
		{
			await Actions.sendChannelMessage(Context, String.Format("This guild has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("channelid")]
		[Alias("cid")]
		[Usage("channelid " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		[UserHasAPermission]
		public async Task ChannelID([Remainder] string input)
		{
			IGuildChannel channel = await Actions.getChannel(Context, input);
			if (channel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.getChannelType(channel), channel.Name, channel.Id));
		}

		[Command("roleid")]
		[Alias("rid")]
		[Usage("roleid [Role]")]
		[Summary("Shows the ID of the given role.")]
		[UserHasAPermission]
		public async Task RoleID([Remainder] string input)
		{
			IRole role = await Actions.getRole(Context, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("userid")]
		[Alias("uid")]
		[Usage("userid <@User>")]
		[Summary("Shows the ID of the given user.")]
		[UserHasAPermission]
		public async Task UserID([Optional, Remainder] string input)
		{
			IGuildUser user = input == null ? Context.User as IGuildUser : await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", user.Username, user.Discriminator, user.Id));
		}
		#endregion

		#region User or other misc info
		[Command("botinfo")]
		[Alias("binf")]
		[Usage("botinfo")]
		[Summary("Displays various information about the bot.")]
		[UserHasAPermission]
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
			var embed = Actions.makeNewEmbed(null, description);
			//Add the author
			Actions.addAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.AvatarUrl);
			//Add the footer
			Actions.addFooter(embed, "Version " + Constants.BOT_VERSION);

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
			Actions.addField(embed, "Logged Actions", firstField);

			//Second field
			var secondField = String.Format(
				"Attempted commands: {0}\n" +
				"Successful commands: {1}\n" +
				"Failed commands: {2}\n",
				Variables.AttemptedCommands,
				Variables.AttemptedCommands - Variables.FailedCommands,
				Variables.FailedCommands);
			Actions.addField(embed, "Commands", secondField);

			//Third field
			var thirdField = String.Format(
				"Memory usage: {0:0.00}MB\n" +
				"Thread count: {1}\n",
				GC.GetTotalMemory(true) / 1048576.0,
				Process.GetCurrentProcess().Threads.Count);
			Actions.addField(embed, "Technical", thirdField);

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, embed);
		}

		[Command("userinfo")]
		[Alias("uinf")]
		[Usage("userinfo [@User]")]
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
				user = await Actions.getUser(Context.Guild, input);
			}

			//Check if valid user
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
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

			//Get an ordered list of when users joined the server
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			//Make the description
			var description = String.Format(
				"ID: {0}\n" +
				"Created: {1} {2}, {3} at {4}\n" +
				"Joined: {5} {6}, {7} at {8} (#{9} to join the server)\n" +
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
			var embed = Actions.makeNewEmbed(null, description);
			//Add the author
			Actions.addAuthor(embed, user.Username + "#" + user.Discriminator + " " + (user.Nickname == null ? "" : "(" + user.Nickname + ")"), user.AvatarUrl, user.AvatarUrl);
			//Add the footer
			Actions.addFooter(embed, "Userinfo");

			//Add the channels the user can access
			if (channels.Count != 0)
			{
				Actions.addField(embed, "Channels", String.Join(", ", channels));
			}
			//Add the roles the user has
			if (roles.Count != 0)
			{
				Actions.addField(embed, "Roles", String.Join(", ", roles));
			}
			//Add the voice channel
			if (user.VoiceChannel != null)
			{
				var text = String.Format("Server mute: {0}\nServer deafen: {1}\nSelf mute: {2}\nSelf deafen: {3}",
					user.IsMuted.ToString(), user.IsDeafened.ToString(), user.IsSelfMuted.ToString(), user.IsSelfDeafened.ToString());

				Actions.addField(embed, "Voice Channel: " + user.VoiceChannel.Name, text);
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, embed);
		}

		[Command("emojiinfo")]
		[Alias("einf")]
		[Usage("emojiinfo [Emoji]")]
		[Summary("Shows information about an emoji. Only global emojis where the bot is in a server that gives them will have a 'From...' text.")]
		public async Task EmojiInfo([Remainder] string input)
		{
			Emoji emoji;
			if (!Emoji.TryParse(input, out emoji))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid emoji supplied."));
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

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(emoji.Name, description, null, emoji.Url));
		}

		[Command("useravatar")]
		[Alias("uav")]
		[Usage("useravatar <@user>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily). Currently every avatar is displayed with an extension type of gif.")]
		[UserHasAPermission]
		public async Task UserAvatar([Optional, Remainder] string input)
		{
			IGuildUser user = input == null ? Context.User as IGuildUser : await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Context.Channel.SendMessageAsync(user.AvatarUrl.Replace(".jpg", ".gif"));
		}

		[Command("currentmembercount")]
		[Alias("cmc")]
		[Usage("currentmembercount")]
		[Summary("Shows the current number of members in the guild.")]
		[UserHasAPermission]
		public async Task CurrentMemberCount()
		{
			await Actions.sendChannelMessage(Context, String.Format("The current member count is `{0}`.", (Context.Guild as SocketGuild).MemberCount));
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage("userjoinedat [Position]")]
		[Summary("Shows the user which joined the guild in that position. Mostly accurate, give or take ten places per thousand users on the guild.")]
		[UserHasAPermission]
		public async Task UserJoinedAt([Remainder] string input)
		{
			int position;
			if (Int32.TryParse(input, out position))
			{
				IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
				var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				if (position >= 1 && position < users.Count)
				{
					IGuildUser user = users[position - 1];
					await Actions.sendChannelMessage(Context, String.Format("`{0}#{1}` was #{2} to join the server on `{3} {4}, {5}` at `{6}`.",
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
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Something besides a number was input."));
			}
		}
		#endregion

		#region Instant Invites
		[Command("createinstantinvite")]
		[Alias("cii")]
		[Usage("createinstantinvite " + Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirements(1U << (int)GuildPermission.CreateInstantInvite)]
		public async Task CreateInstantInvite([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 4);
			if (inputArray.Length != 4)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check validity of channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[0]);
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

			await Actions.sendChannelMessage(Context, String.Format("Here is your invite for `{0} ({1})`: {2} \nIt will last for{3}, {4}{5}.",
				channel.Name,
				Actions.getChannelType(channel),
				inv.Url,
				nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : "and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership" : ""));
		}

		[Command("listinstantinvites")]
		[Alias("lii")]
		[Usage("listinstantinvites")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
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
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Instant Invites", description));
		}

		[Command("deleteinstantinvite")]
		[Alias("dii")]
		[Usage("deleteinstantinvite [Invite Code]")]
		[Summary("Deletes the invite with the given code.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteInstantInvite([Remainder] string input)
		{
			//Get the input
			var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			if (invite == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite doesn't exist."));
				return;
			}

			//Delete the invite and send a success message
			await invite.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the invite `{0}`.", invite.Code));
		}

		[Command("deletemultipleinvites")]
		[Alias("dmi")]
		[Usage("deletemultipleinvites [@User|" + Constants.CHANNEL_INSTRUCTIONS + "|Uses:Number|Expires:Number]")]
		[Summary("Deletes all invites satisfying the given condition of either user, creation channel, uses, or expiry time.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteMultipleInvites([Remainder] string input)
		{
			//Set the action telling what variable
			DeleteInvAction? action = null;

			//Check if user
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user != null)
			{
				action = DeleteInvAction.User;
			}

			//Check if channel
			IGuildChannel channel = null;
			if (action == null)
			{
				channel = await Actions.getChannelEditAbility(Context, input, true);
				if (channel != null)
				{
					action = DeleteInvAction.Channel;
				}
			}

			//Check if uses
			int uses = 0;
			if (action == null)
			{
				var usesString = Actions.getVariable(input, "uses");
				if (int.TryParse(usesString, out uses))
				{
					action = DeleteInvAction.Uses;
				}
			}

			//Check if expiry time
			int expiry = 0;
			if (action == null)
			{
				var expiryString = Actions.getVariable(input, "expires");
				if (int.TryParse(expiryString, out expiry))
				{
					action = DeleteInvAction.Expiry;
				}
			}

			//Have gone through every other check so it's an error at this point
			if (action == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid target supplied."));
				return;
			}

			//Get the guild's invites
			var guildInvites = await Context.Guild.GetInvitesAsync();
			//Check if the amount is greater than zero
			if (!guildInvites.Any())
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invites satisfied the given condition."));
				return;
			}

			//Get the count of how many invites matched the condition
			var count = invites.Count;

			//Delete the invites
			invites.ForEach(async x => await x.DeleteAsync());

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this server.", count));
		}
		#endregion

		#region Reminds
		[Command("modifyremind")]
		[Alias("mrem")]
		[Usage("modifyremind [Add|Remove] [Name]/<Text>")]
		[Summary("Adds the given text to a list that can be called through the `remind` command.")]
		[UserHasAPermission]
		public async Task ModifyRemind([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check what action to do
			bool addBool;
			if (inputArray[0].Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				addBool = true;
			}
			else if (inputArray[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				addBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			var name = "";
			var reminds = Variables.Guilds[Context.Guild.Id].Reminds;
			if (addBool)
			{
				//Check if at the max number of reminds
				if (reminds.Count >= Constants.MAX_REMINDS)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild already has the max number of reminds, which is 50."));
					return;
				}

				//Separate out the name and text
				var nameAndText = inputArray[1].Split(new char[] { '/' }, 2);
				if (nameAndText.Length != 2)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				name = nameAndText[0];
				var text = nameAndText[1];

				//Check if any reminds have already have the same name
				if (reminds.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A remind already has that name."));
					return;
				}

				//Add them to the list
				reminds.Add(new Remind(name, text));
			}
			else
			{
				//Make sure there are some reminds
				if (!reminds.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There needs to be at least one remind before you can remove any."));
					return;
				}
				name = inputArray[1];

				//Remove all reminds with the same name
				reminds.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}

			//Get the path
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.REMINDS);
			//Rewrite everything with the current reminds. Uses a different split character than the others because it's more user set than them.
			Actions.saveLines(path, null, null, reminds.Select(x => x.Name + "/" + x.Text).ToList(), true);

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following remind: `{1}`.", addBool ? "added" : "removed", Actions.replaceMessageCharacters(name)));
		}

		[Command("remind")]
		[Alias("rem")]
		[Usage("remind <Name>")]
		[Summary("Shows the content for the given remind. If null then shows the list of the current reminds.")]
		public async Task CurrentRemind([Optional, Remainder] string input)
		{
			var reminds = Variables.Guilds[Context.Guild.Id].Reminds;
			if (String.IsNullOrWhiteSpace(input))
			{
				//Check if any exist
				if (!reminds.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no reminds."));
					return;
				}

				//Send the names of all fo the reminds
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Reminds", String.Format("`{0}`", String.Join("`, `", reminds.Select(x => x.Name)))));
				return;
			}

			//Check if any reminds have the given name
			var remind = reminds.FirstOrDefault(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
			if (remind.Name != null)
			{
				await Actions.sendChannelMessage(Context, remind.Text);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no remind with the given name."));
			}
		}
		#endregion

		#region Miscellaneous
		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage("mentionrole [Role]/[Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserHasAPermission]
		public async Task MentionRole([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Get the role and see if it can be changed
			IRole role = await Actions.getRoleEditAbility(Context, inputArray[0]);
			if (role == null)
				return;

			//See if people can already mention the role
			if (role.IsMentionable)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You can already mention this role."));
				return;
			}

			//Make the role mentionable
			await role.ModifyAsync(x => x.Mentionable = true);
			//Send the message
			await Actions.sendChannelMessage(Context, role.Mention + ": " + inputArray[1]);
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("listemojis")]
		[Alias("lemojis")]
		[Usage("listemojis [Global|Guild]")]
		[Summary("Lists the emoji in the guild. As of right now, with the current API wrapper version this bot uses, there's no way to upload or remove emojis yet; sorry.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageEmojis)]
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option."));
				return;
			}

			//Check if the description is still null
			description = description ?? String.Format("This guild has no {0} emojis.", input.ToLower());

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Emojis", description));
		}

		[Command("test")]
		[BotOwnerRequirement]
		public async Task Test([Optional, Remainder] string input)
		{
			await Actions.sendChannelMessage(Context.Channel, "Yeyo");
		}
		#endregion
	}
}