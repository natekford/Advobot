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
		public async Task Help([Optional][Remainder] String input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.sendChannelMessage(Context.Channel, "Type `>commands` for the list of commands." +
					"\nType `" + Constants.BOT_PREFIX + "help [Command]` for help with a command.\nLink to the documentation of this bot: https://gist.github.com/advorange/3da9140889b20009816e4c9629de51c9");
				return;
			}

			//Get the input command, if nothing then link to documentation
			String[] commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].ToLower().Equals("command"))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
						"If you do not know what commands this bot has, type `>commands` for a list of commands.", 10000);
					return;
				}
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
					"[] means required information. <> means optional information. | means or.", 10000);
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
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Nonexistent command."), Constants.WAIT_TIME);
					return;
				}
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("```Aliases: {0}\nUsage: {1}\nBase Permission(s): {2}\nDescription: {3}```",
				helpEntry.Aliases, helpEntry.Usage, helpEntry.basePerm, helpEntry.Text));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage(Constants.BOT_PREFIX + "commands <Category|All>")]
		[Summary("Prints out the commands in that section of the command list.")]
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
				await Actions.sendChannelMessage(Context.Channel, String.Format("**ADMINISTRATION:** ```\n{0}```", String.Join("\n", commands)));
			}
			else if (section.Equals("moderation"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 1);
				await Actions.sendChannelMessage(Context.Channel, String.Format("**MODERATION:** ```\n{0}```", String.Join("\n", commands)));
			}
			else if (section.Equals("votemute"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 2);
				await Actions.sendChannelMessage(Context.Channel, String.Format("**VOTEMUTE:** ```\n{0}```", String.Join("\n", commands)));
			}
			else if (section.Equals("slowmode"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 3);
				await Actions.sendChannelMessage(Context.Channel, String.Format("**SLOWMODE:** ```\n{0}```", String.Join("\n", commands)));
			}
			else if (section.Equals("banphrase"))
			{
				String[] commands = Actions.getCommands(Context.Guild, 4);
				await Actions.sendChannelMessage(Context.Channel, String.Format("**BANPHRASE:** ```\n{0}```", String.Join("\n", commands)));
			}
			else if (section.Equals("all"))
			{
				if (allCommandsBool)
				{
					List<String> commands = new List<String>();
					foreach (String command in Variables.mCommandNames)
					{
						commands.Add(command);
					}
					await Actions.sendChannelMessage(Context.Channel, String.Format("**ALL:** ```\n{0}```", String.Join("\n", commands)));
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, "All is currently turned off.", Constants.WAIT_TIME);
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Category does not exist."), Constants.WAIT_TIME);
			}
		}

		[Command("serverid")]
		[Alias("sid")]
		[Usage(Constants.BOT_PREFIX + "serverid")]
		[Summary("Shows the ID of the server.")]
		public async Task ServerID()
		{
			await Actions.sendChannelMessage(Context.Channel, String.Format("This server has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("channelid")]
		[Alias("cid")]
		[Usage(Constants.BOT_PREFIX + "channelid " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		public async Task ChannelID([Remainder] String input)
		{
			IGuildChannel channel = Actions.getChannel(Context.Guild, input).Result;
			if (channel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.CHANNEL_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The {0} channel `{1}` has the ID `{2}`.",
				channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? "text" : "voice", channel.Name, channel.Id));
		}

		[Command("roleid")]
		[Alias("rid")]
		[Usage(Constants.BOT_PREFIX + "roleid [Role]")]
		[Summary("Shows the ID of the given role.")]
		public async Task RoleID([Remainder] String input)
		{
			IRole role = Actions.getRole(Context.Guild, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("userid")]
		[Alias("uid")]
		[Usage(Constants.BOT_PREFIX + "userid [@User]")]
		[Summary("Shows the ID of the given user.")]
		public async Task UserID([Remainder] String input)
		{
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Format("The user  has the ID `{1}`.", user.Mention, user.Id));
		}

		[Command("userinfo")]
		[Alias("uinf")]
		[Usage(Constants.BOT_PREFIX + "userinfo [@User]")]
		[Summary("Displays various information about the user.")]
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
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
			await (Context.Guild as SocketGuild).DownloadUsersAsync();
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			List<IGuildUser> users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			await Actions.sendChannelMessage(Context.Channel, String.Format(
					"{0}```" +
					"\nUsername: {1}#{2}" +
					"\nID: {3}" +
					"\n" +
					"\nNickname: {4}" +
					"\nJoined: {5} (#{6} to join the server)" +
					//"\nLast activity: {6}" +
					"\nRoles: {7}" +
					"\nAble to access: {8}" +
					"\n" +
					"\nIn voice channel: {9}" +
					"{10}" +
					"{11}" +
					"{12}" +
					"{13}" +
					"\n" +
					"\nCurrent game: {14}" +
					"\nAvatar URL: {15}" +
					"\nOnline status: {16}```",
					user.Mention,
					user.Username, user.Discriminator,
					user.Id,
					user.Nickname == null ? "N/A" : user.Nickname,
					user.JoinedAt.Value.UtcDateTime,
					//user.LastActivityAt == null ? "N/A" : user.LastActivityAt.ToString(),
					users.IndexOf(user) + 1,
					roles.Count() == 0 ? "N/A" : String.Join(", ", roles),
					channels.Count() == 0 ? "N/A" : String.Join(", ", channels),
					user.VoiceChannel == null ? "N/A" : user.VoiceChannel.ToString(),
					user.VoiceChannel == null ? "" : "\nServer mute: " + user.IsMuted.ToString(),
					user.VoiceChannel == null ? "" : "\nServer deafen: " + user.IsDeafened.ToString(),
					user.VoiceChannel == null ? "" : "\nSelf mute: " + user.IsSelfMuted.ToString(),
					user.VoiceChannel == null ? "" : "\nSelf deafen: " + user.IsSelfDeafened.ToString(),
					user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
					user.AvatarUrl,
					user.Status));
		}

		//await user.ModifyAsync(x => x.Nickname = "test");
	}
}
