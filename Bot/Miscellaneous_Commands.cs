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
using static Advobot.Constants;
using static Advobot.Actions;

namespace Advobot
{
	public class Miscellaneous_Commands : ModuleBase
	{
		[Command("help")]
		[Alias("h")]
		[Usage(BOT_PREFIX + "help <Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
			"If left blank will print out a link to the documentation of this bot.")]
		public async Task Help([Remainder] String input)
		{
			//Get the input command, if nothing then link to documentation
			String[] commandParts = input.Split(new char[] { '[' }, 2);
			//See if nothing was input
			if (String.IsNullOrWhiteSpace(input))
			{
				await sendChannelMessage(Context.Channel, "Type `>commands` for the list of commands." +
					"\nType `>help [Command]` for help with a command.\nLink to the documentation of this bot: https://gist.github.com/advorange/3da9140889b20009816e4c9629de51c9");
				return;
			}
			else if (input.IndexOf('[') == 0)
			{
				if (commandParts[1].ToLower().Equals("command"))
				{
					await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
						"If you do not know what commands this bot has, type `>commands` for a list of commands.", 10000);
					return;
				}
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
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
					await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR("Nonexistent command."), WAIT_TIME);
					return;
				}
			}
			await sendChannelMessage(Context.Channel, String.Format("```Aliases: {0}\nUsage: {1}\nBase Permission(s): {2}\nDescription: {3}```",
				helpEntry.Aliases, helpEntry.Usage, helpEntry.basePerm, helpEntry.Text));
		}

		[Command("serverid")]
		[Alias("sid")]
		[Usage(BOT_PREFIX + "serverid")]
		[Summary("Shows the ID of the server.")]
		public async Task ServerID()
		{
			await sendChannelMessage(Context.Channel, String.Format("This server has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("channelid")]
		[Alias("cid")]
		[Usage(BOT_PREFIX + "channelid " + CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		public async Task ChannelID([Remainder] String input)
		{
			IGuildChannel channel = getChannel(Context.Guild, input).Result;
			if (channel == null)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
				return;
			}
			await sendChannelMessage(Context.Channel, String.Format("The {0} channel `{1}` has the ID `{2}`.",
				channel.GetType().Name.ToLower().Contains(TEXT_TYPE) ? "text" : "voice", channel.Name, channel.Id));
		}

		[Command("roleid")]
		[Alias("rid")]
		[Usage(BOT_PREFIX + "roleid [Role]")]
		[Summary("Shows the ID of the given role.")]
		public async Task RoleID([Remainder] String input)
		{
			IRole role = getRole(Context.Guild, input);
			if (role == null)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(ROLE_ERROR), WAIT_TIME);
				return;
			}
			await sendChannelMessage(Context.Channel, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("userid")]
		[Alias("uid")]
		[Usage(BOT_PREFIX + "userid [@User]")]
		[Summary("Shows the ID of the given user.")]
		public async Task UserID([Remainder] String input)
		{
			IGuildUser user = await getUser(Context.Guild, input);
			if (user == null)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(USER_ERROR), WAIT_TIME);
				return;
			}
			await sendChannelMessage(Context.Channel, String.Format("The user  has the ID `{1}`.", user.Mention, user.Id));
		}

		[Command("userinfo")]
		[Alias("uinf")]
		[Usage(BOT_PREFIX + "userinfo [@User]")]
		[Summary("Displays various information about the user.")]
		public async Task UserInfo([Remainder] String input)
		{
			IGuildUser user = await getUser(Context.Guild, input);
			if (user == null)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(USER_ERROR), WAIT_TIME);
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
				if (channel.GetType().Name.ToLower().Contains(VOICE_TYPE))
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
			List<IGuildUser> users = guildUsers.ToList();
			users.RemoveAll(x => x.JoinedAt == null);
			users.Sort((x, y) => (x.JoinedAt.Value.Ticks) < (y.JoinedAt.Value.Ticks) ? -1 : 1);

			await sendChannelMessage(Context.Channel, String.Format(
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
