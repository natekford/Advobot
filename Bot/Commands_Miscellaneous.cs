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

namespace Advobot
{
	public class Miscellaneous_Commands : ModuleBase
	{
		[Command("help")]
		[Alias("h")]
		[Usage(Constants.BOT_PREFIX + "help <Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
			"If left blank will print out a link to the documentation of this bot.")]
		[UserHasAPermission()]
		public async Task Help([Optional][Remainder] String input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.sendChannelMessage(Context.Channel, "Type `" + Constants.BOT_PREFIX + "commands` for the list of commands.\n" +
					"Type `" + Constants.BOT_PREFIX + "help [Command]` for help with a command.\nLink to the current repo of this bot: https://github.com/advorange/Advobot");
				return;
			}

			//Get the input command, if nothing then link to documentation
			String[] commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].ToLower().Equals("command"))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "If you do not know what commands this bot has, type `>commands` for a list of commands.", 10000);
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
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent command."), Constants.WAIT_TIME);
					return;
				}
			}
			String description = String.Format("**Aliases:** {0}\n**Usage:** {1}\n**Base Permission(s):** {2}\n\n**Description:** {3}",
				String.Join(", ", helpEntry.Aliases), helpEntry.Usage, helpEntry.basePerm, helpEntry.Text);
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, helpEntry.Name, description));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage(Constants.BOT_PREFIX + "commands <Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[UserHasAPermission()]
		public async Task Commands([Optional][Remainder] String input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.sendChannelMessage(Context.Channel, "The following categories exist: `Administration`, `Moderation`, `Votemute`, `Slowmode`, `Banphrase`, and `All`." +
					"\nType `" + Constants.BOT_PREFIX + "commands [Category]` for commands from that category.");
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
					await Actions.makeAndDeleteSecondaryMessage(Context, "All is currently turned off.", Constants.WAIT_TIME);
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."), Constants.WAIT_TIME);
			}
		}

		[Command("setgame")]
		[Usage(Constants.BOT_PREFIX + "setgame [New name]")]
		[Summary("Changes the game the bot is currently listed as playing. By default only the person hosting the bot can do this.")]
		[BotOwnerRequirement()]
		public async Task SetGame([Remainder] String input)
		{
			//Check the game name length
			if (input.Length > 128)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Game name cannot be longer than 128 characters or else it doesn't show to other people."), 10000);
				return;
			}

			await CommandHandler.client.SetGame(input);
			await Actions.sendChannelMessage(Context.Channel, String.Format("Game set to `{0}`.", input));
		}

		[Command("disconnect")]
		[Alias("runescapeservers")]
		[Usage(Constants.BOT_PREFIX + "disconnect")]
		[Summary("Turns the bot off. By default only the person hosting the bot can do this.")]
		[BotOwnerRequirement()]
		public async Task Disconnect()
		{
			if ((Context.User.Id == Constants.OWNER_ID) || (Constants.DISCONNECT == true))
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * .75)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendChannelMessage(channel, String.Format("{0} Bot is disconnecting...", time)));
						}
					}
				}
				while (msgs.Any(x => x.CreatedAt == null))
				{
					Thread.Sleep(100);
				}
				Environment.Exit(1);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.", Constants.WAIT_TIME);
			}
		}

		[Command("restart")]
		[Usage(Constants.BOT_PREFIX + "restart")]
		[Summary("Restarts the bot. By default only the person hosting the bot can do this.")]
		[BotOwnerRequirement()]
		public async Task Restart()
		{
			//Does not work, need to fix it
			if ((Context.User.Id == Constants.OWNER_ID) || (Constants.DISCONNECT == true))
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * .75)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendChannelMessage(channel, String.Format("{0} Bot is restarting...", time)));
						}
					}
				}
				while (msgs.Any(x => x.CreatedAt == null))
				{
					Thread.Sleep(100);
				}

				try
				{
					//Create a new instance of the bot
					System.Windows.Forms.Application.Restart();
					//Close the old one
					Environment.Exit(1);
				}
				catch (Exception)
				{
					Console.WriteLine("!!!BOT IS UNABLE TO RESTART!!!");
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.", Constants.WAIT_TIME);
			}
		}

		[Command("guildid")]
		[Alias("gid", "serverid", "sid")]
		[Usage(Constants.BOT_PREFIX + "guildid")]
		[Summary("Shows the ID of the guild.")]
		[UserHasAPermission()]
		public async Task ServerID()
		{
			await Actions.sendChannelMessage(Context.Channel, String.Format("This server has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("channelid")]
		[Alias("cid")]
		[Usage(Constants.BOT_PREFIX + "channelid " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		[UserHasAPermission()]
		public async Task ChannelID([Remainder] String input)
		{
			IGuildChannel channel = Actions.getChannel(Context.Guild, input).Result;
			if (channel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The {0} channel `{1}` has the ID `{2}`.",
				channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? "text" : "voice", channel.Name, channel.Id));
		}

		[Command("roleid")]
		[Alias("rid")]
		[Usage(Constants.BOT_PREFIX + "roleid [Role]")]
		[Summary("Shows the ID of the given role.")]
		[UserHasAPermission()]
		public async Task RoleID([Remainder] String input)
		{
			IRole role = await Actions.getRole(Context, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("userid")]
		[Alias("uid")]
		[Usage(Constants.BOT_PREFIX + "userid [@User]")]
		[Summary("Shows the ID of the given user.")]
		[UserHasAPermission()]
		public async Task UserID([Remainder] String input)
		{
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The user  has the ID `{1}`.", user.Mention, user.Id));
		}

		[Command("currentmembercount")]
		[Alias("cmc")]
		[Usage(Constants.BOT_PREFIX + "currentmembercount")]
		[Summary("Shows the current number of members in the guild.")]
		[UserHasAPermission()]
		public async Task CurrentMemberCount()
		{
			await Actions.sendChannelMessage(Context.Channel, String.Format("The current member count is `{0}`.", (Context.Guild as SocketGuild).MemberCount));
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage(Constants.BOT_PREFIX + "userjoinedat [int]")]
		[Summary("Shows the user which joined the guild in that position. Mostly accurate, give or take ten places per thousand users on the guild.")]
		[UserHasAPermission()]
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
					await Actions.sendChannelMessage(Context.Channel, String.Format("{0} was #{1} to join the server on `{2} {3}, {4}` at `{5}`.",
						user.Mention, position,
						System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
						user.JoinedAt.Value.UtcDateTime.Day,
						user.JoinedAt.Value.UtcDateTime.Year,
						user.JoinedAt.Value.UtcDateTime.ToString("HH:mm:ss")));
					return;
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."), Constants.WAIT_TIME);
					return;
				}
			}
			await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Something besides a number was input."), Constants.WAIT_TIME);
			return;
		}

		[Command("userinfo")]
		[Alias("uinf")]
		[Usage(Constants.BOT_PREFIX + "userinfo [@User]")]
		[Summary("Displays various information about the user. Join position is mostly accurate, give or take ten places per thousand users on the guild.")]
		[UserHasAPermission()]
		public async Task UserInfo([Optional][Remainder] String input)
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
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
				if (channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE))
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
				"Discriminator: {0}\n" +
				"ID: {1}\n" +
				"\n" +
				"Nickname: {2}\n" +
				"Joined: {3} {4}, {5} at {6} (#{7} to join the server)\n" +
				"Roles: {8}\n" +
				"Able to access: {9}\n" +
				"\n" +
				"Voice channel: {10}\n" +
				"{11}" +
				"{12}" +
				"{13}" +
				"{14}" +
				"\n" +
				"Current game: {15}\n" +
				"Online status: {16}\n" +
				"Avatar:",	
				user.Discriminator,
				user.Id,
				user.Nickname == null ? "N/A" : user.Nickname,
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
				user.JoinedAt.Value.UtcDateTime.Day,
				user.JoinedAt.Value.UtcDateTime.Year,
				user.JoinedAt.Value.UtcDateTime.ToLongTimeString(),
				users.IndexOf(user) + 1,
				roles.Count == 0 ? "N/A" : String.Join(", ", roles),
				channels.Count == 0 ? "N/A" : String.Join(", ", channels),
				user.VoiceChannel == null ? "N/A" : user.VoiceChannel.ToString(),
				user.VoiceChannel == null ? "" : "Server mute: " + user.IsMuted.ToString() + "\n",
				user.VoiceChannel == null ? "" : "Server deafen: " + user.IsDeafened.ToString() + "\n",
				user.VoiceChannel == null ? "" : "Self mute: " + user.IsSelfMuted.ToString() + "\n",
				user.VoiceChannel == null ? "" : "Self deafen: " + user.IsSelfDeafened.ToString() + "\n",
				user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
				user.Status);

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, user.Username, description, user.AvatarUrl));
		}

		[Command("botinfo")]
		[Alias("binf")]
		[Usage(Constants.BOT_PREFIX + "botinfo")]
		[Summary("Displays various information about the bot.")]
		[UserHasAPermission()]
		public async Task BotInfo()
		{
			TimeSpan span = DateTime.UtcNow.Subtract(Variables.StartupTime);

			//Make the description
			String description = String.Format(
				"Online since: {0}\n" +
				"Uptime: {1}:{2}:{3}:{4}\n" +
				"Guild count: {5}\n" +
				"Cumulative member count: {6}\n" +
				"\n" +
				"Attempted commands: {7}\n" +
				"Successful commands: {8}\n" +
				"Failed commands: {9}\n" +
				"\n" +
				"Logged joins: {10}\n" +
				"Logged leaves: {11}\n" +
				"Logged bans: {12}\n" +
				"Logged unbans: {13}\n" +
				"Logged user changes: {14}\n" +
				"Logged edits: {15}\n" +
				"Logged deletes: {16}\n",
				Variables.StartupTime,
				span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"),
				Variables.TotalGuilds,
				Variables.TotalUsers,
				Variables.AttemptedCommands,
				Variables.AttemptedCommands - Variables.FailedCommands,
				Variables.FailedCommands,
				Variables.LoggedJoins,
				Variables.LoggedLeaves,
				Variables.LoggedBans,
				Variables.LoggedUnbans,
				Variables.LoggedUserChanges,
				Variables.LoggedEdits,
				Variables.LoggedDeletes);

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, Constants.BOT_NAME, description, null));
		}

		[Command("createinstantinvite")]
		[Alias("crinv")]
		[Usage(Constants.BOT_PREFIX + "createinstantinvite " + Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. " +
			"The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.CreateInstantInvite))]
		public async Task CreateInstantInvite([Remainder] String input)
		{
			String[] inputArray = input.Split(new char[] { ' ' }, 4);
			if (inputArray.Length != 4)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
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

			await Actions.sendChannelMessage(Context.Channel, String.Format("Here is your invite for `{0}`: {1} \nIt will last for{2}, {3}{4}",
				channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? channel.Name + " (Voice)" : channel.Name + " (Text)", inv.Url,
				nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : " and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership." : "."));
		}

		[Command("test")]
		[BotOwnerRequirement]
		public async Task Test([Optional][Remainder] String input)
		{
			EmbedBuilder embed = Actions.makeNewEmbed(null, "yeyo", "sample text", null);
			embed.ThumbnailUrl = "http://i.imgur.com/Xbk5Yyd.jpg";
			embed.ImageUrl = "http://i.imgur.com/Xbk5Yyd.jpg";
			embed.Url = "http://i.imgur.com/Xbk5Yyd.jpg";

			EmbedAuthorBuilder author = new EmbedAuthorBuilder().WithIconUrl("http://i.imgur.com/Xbk5Yyd.jpg").WithName("ADVSAUCY").WithUrl("http://i.imgur.com/Xbk5Yyd.jpg");
			embed.Author = author;

			EmbedFooterBuilder footer = new EmbedFooterBuilder();
			footer.IconUrl = "http://i.imgur.com/Xbk5Yyd.jpg";
			footer.Text = "footertext ";
			embed.Footer = footer;

			Actions.addField(embed, "Test", "Test", true);
			Actions.addField(embed, "Test2", "Test", true);

			await Context.Channel.SendMessageAsync("", embed: embed);
		}
	}
}
