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
	[Name("Miscellaneous")]
	public class Miscellaneous_Commands : ModuleBase
	{
		[Command("help")]
		[Alias("h")]
		[Usage(Constants.BOT_PREFIX + "help <Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
			"If left blank will print out a link to the documentation of this bot.")]
		[UserHasAPermission]
		public async Task Help([Optional, Remainder] String input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				//Description string
				String text = "Type `" + Constants.BOT_PREFIX + "commands` for the list of commands.\nType `" + Constants.BOT_PREFIX + "help [Command]` for help with a command.";

				//Make the embed
			    EmbedBuilder embed = Actions.makeNewEmbed(null, "Commands", text);
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
			String[] commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].ToLower().Equals("command"))
				{
					String text = "If you do not know what commands this bot has, type `" + Constants.BOT_PREFIX + "commands` for a list of commands.";
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
			String description = String.Format("**Aliases:** {0}\n**Usage:** {1}\n\n**Base Permission(s):**\n{2}\n\n**Description:**\n{3}",
				String.Join(", ", helpEntry.Aliases), helpEntry.Usage, helpEntry.basePerm, helpEntry.Text);
			await Actions.sendEmbedMessage(Context.Channel, Actions.addFooter(Actions.makeNewEmbed(null, helpEntry.Name, description), "Help"));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage(Constants.BOT_PREFIX + "commands <Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[UserHasAPermission]
		public async Task Commands([Optional, Remainder] String input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				EmbedBuilder embed = Actions.makeNewEmbed(null, "Commands", "Type `" + Constants.BOT_PREFIX + "commands [Category]` for commands from that category.");
				await Actions.sendEmbedMessage(Context.Channel, Actions.addField(embed, "Categories", "Administration\nModeration\nVotemute\nSlowmode\nBanphrase\nAll"));
				return;
			}

			Actions.loadPreferences(Context.Guild);

			bool allCommandsBool = true;
			String section = input.ToLower();

			if (section.Equals("administration"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 0);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "ADMINISTRATION", String.Join("\n", commands)));
			}
			else if (section.Equals("moderation"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 1);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "MODERATION", String.Join("\n", commands)));
			}
			else if (section.Equals("votemute"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 2);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "VOTEMUTE", String.Join("\n", commands)));
			}
			else if (section.Equals("slowmode"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 3);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "SLOWMODE", String.Join("\n", commands)));
			}
			else if (section.Equals("banphrase"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 4);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "BANPHRASE", String.Join("\n", commands)));
			}
			else if (section.Equals("all"))
			{
				if (allCommandsBool)
				{
					List<String> commands = new List<String>();
					foreach (String command in Variables.CommandNames)
					{
						commands.Add(command);
					}
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "ALL", String.Join("\n", commands)));
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

		[Command("guildid")]
		[Alias("gid", "serverid", "sid")]
		[Usage(Constants.BOT_PREFIX + "guildid")]
		[Summary("Shows the ID of the guild.")]
		[UserHasAPermission]
		public async Task ServerID()
		{
			await Actions.sendChannelMessage(Context.Channel, String.Format("This guild has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("channelid")]
		[Alias("cid")]
		[Usage(Constants.BOT_PREFIX + "channelid " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		[UserHasAPermission]
		public async Task ChannelID([Remainder] String input)
		{
			IGuildChannel channel = await Actions.getChannel(Context, input);
			if (channel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.getChannelType(channel), channel.Name, channel.Id));
		}

		[Command("roleid")]
		[Alias("rid")]
		[Usage(Constants.BOT_PREFIX + "roleid [Role]")]
		[Summary("Shows the ID of the given role.")]
		[UserHasAPermission]
		public async Task RoleID([Remainder] String input)
		{
			IRole role = await Actions.getRole(Context, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("userid")]
		[Alias("uid")]
		[Usage(Constants.BOT_PREFIX + "userid <@User>")]
		[Summary("Shows the ID of the given user.")]
		[UserHasAPermission]
		public async Task UserID([Optional, Remainder] String input)
		{
			IGuildUser user = input == null ? Context.User as IGuildUser : await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The user `{0}#{1}` has the ID `{2}`.", user.Username, user.Discriminator, user.Id));
		}

		[Command("useravatar")]
		[Alias("uav")]
		[Usage(Constants.BOT_PREFIX + "useravatar <@user>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily). Currently every avatar is displayed with an extension type of gif.")]
		[UserHasAPermission]
		public async Task UserAvatar([Optional, Remainder] String input)
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
		[Usage(Constants.BOT_PREFIX + "currentmembercount")]
		[Summary("Shows the current number of members in the guild.")]
		[UserHasAPermission]
		public async Task CurrentMemberCount()
		{
			await Actions.sendChannelMessage(Context.Channel, String.Format("The current member count is `{0}`.", (Context.Guild as SocketGuild).MemberCount));
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage(Constants.BOT_PREFIX + "userjoinedat [int]")]
		[Summary("Shows the user which joined the guild in that position. Mostly accurate, give or take ten places per thousand users on the guild.")]
		[UserHasAPermission]
		public async Task UserJoinedAt([Remainder] String input)
		{
			int position;
			if (Int32.TryParse(input, out position))
			{
				IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
				List<IGuildUser> users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				if (position >= 1 && position < users.Count)
				{
					IGuildUser user = users[position - 1];
					await Actions.sendChannelMessage(Context.Channel, String.Format("`{0}#{1}` was #{2} to join the server on `{3} {4}, {5}` at `{6}`.",
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

		[Command("userinfo")]
		[Alias("uinf")]
		[Usage(Constants.BOT_PREFIX + "userinfo [@User]")]
		[Summary("Displays various information about the user. Join position is mostly accurate, give or take ten places per thousand users on the guild.")]
		[UserHasAPermission]
		public async Task UserInfo([Optional, Remainder] String input)
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
			List<String> roles = new List<String>();
			foreach (UInt64 roleID in user.RoleIds)
			{
				roles.Add(Context.Guild.GetRole(roleID).Name);
			}
			roles.Remove(Context.Guild.EveryoneRole.Name);

			//Get a list of channels
			List<String> channels = new List<String>();
			IReadOnlyCollection<IGuildChannel> guildChannels = await Context.Guild.GetChannelsAsync();
			foreach (IGuildChannel channel in guildChannels)
			{
				if (Actions.getChannelType(channel) == Constants.VOICE_TYPE)
				{
					if (user.GetPermissions(channel).Connect)
					{
						channels.Add(channel.Name + " (Voice)");
					}
				}
				else
				{
					using (var channelUsers = channel.GetUsersAsync().GetEnumerator())
					{
						while (await channelUsers.MoveNext())
						{
							if (channelUsers.Current.Contains(user))
							{
								channels.Add(channel.Name);
								break;
							}
						}
					}
				}
			}

			//Get an ordered list of when users joined the server
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			List<IGuildUser> users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			//Make the description
			String description = String.Format(
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
			EmbedBuilder embed = Actions.makeNewEmbed(null, null, description);
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
				String text = String.Format("Server mute: {0}\nServer deafen: {1}\nSelf mute: {2}\nSelf deafen: {3}",
					user.IsMuted.ToString(), user.IsDeafened.ToString(), user.IsSelfMuted.ToString(), user.IsSelfDeafened.ToString());

				Actions.addField(embed, "Voice Channel: " + user.VoiceChannel.Name, text);
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, embed);
		}

		[Command("botinfo")]
		[Alias("binf")]
		[Usage(Constants.BOT_PREFIX + "botinfo")]
		[Summary("Displays various information about the bot.")]
		[UserHasAPermission]
		public async Task BotInfo()
		{
			TimeSpan span = DateTime.UtcNow.Subtract(Variables.StartupTime);

			//Make the description
			String description = String.Format(
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
			EmbedBuilder embed = Actions.makeNewEmbed(null, null, description);
			//Add the author
			Actions.addAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.AvatarUrl);
			//Add the footer
			Actions.addFooter(embed, "Version " + Constants.BOT_VERSION);

			//First field
			String firstField = String.Format(
				"Logged joins: {0}\n" +
				"Logged leaves: {1}\n" +
				"Logged bans: {2}\n" +
				"Logged unbans: {3}\n" +
				"Logged user changes: {4}\n" +
				"Logged edits: {5}\n" +
				"Logged deletes: {6}\n" +
				"Logged images: {7}\n" +
				"Logged gifs: {8}\n" +
				"Logged files: {9}\n" +
				"Logged commands: {10}",
				Variables.LoggedJoins,
				Variables.LoggedLeaves,
				Variables.LoggedBans,
				Variables.LoggedUnbans,
				Variables.LoggedUserChanges,
				Variables.LoggedEdits,
				Variables.LoggedDeletes,
				Variables.LoggedImages,
				Variables.LoggedGifs,
				Variables.LoggedFiles,
				Variables.LoggedCommands);
			Actions.addField(embed, "Logged Actions", firstField);

			//Second field
			String secondField = String.Format(
				"Attempted commands: {0}\n" +
				"Successful commands: {1}\n" +
				"Failed commands: {2}\n",
				Variables.AttemptedCommands,
				Variables.AttemptedCommands - Variables.FailedCommands,
				Variables.FailedCommands);
			Actions.addField(embed, "Commands", secondField);

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, embed);
		}

		[Command("createinstantinvite")]
		[Alias("crinv")]
		[Usage(Constants.BOT_PREFIX + "createinstantinvite " + Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. " +
			"The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirements(1U << (int)GuildPermission.CreateInstantInvite)]
		public async Task CreateInstantInvite([Remainder] String input)
		{
			String[] inputArray = input.Split(new char[] { ' ' }, 4);
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
			if (inputArray[3].ToLower().Equals("true"))
			{
				tempMembership = true;
			}

			//Make into valid invite link
			IInvite inv = await channel.CreateInviteAsync(nullableTime, nullableUsers, tempMembership);

			await Actions.sendChannelMessage(Context.Channel, String.Format("Here is your invite for `{0} ({1})`: {2} \nIt will last for{3}, {4}{5}",
				channel.Name, Actions.getChannelType(channel), inv.Url, nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : " and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership." : "."));
		}

		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage(Constants.BOT_PREFIX + "mentionrole [Role]/[Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserHasAPermission]
		public async Task MentionRole([Remainder] String input)
		{
			String[] inputArray = input.Split(new char[] { '/' }, 2);

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
			await Actions.sendChannelMessage(Context.Channel, role.Mention + ": " + inputArray[1]);
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("test")]
		public async Task Test([Optional, Remainder] String input)
		{
			await Actions.sendChannelMessage(Context.Channel, "Test");
		}
	}
}