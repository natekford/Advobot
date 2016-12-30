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
	public class Administration_Commands : ModuleBase
	{
		[Command("setgame")]
		[Usage(Constants.BOT_PREFIX + "setgame [New name]")]
		[Summary("Changes the game the bot is currently listed as playing.")]
		[BotOwnerRequirement]
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
		[Alias("dc", "runescapeservers")]
		[Usage(Constants.BOT_PREFIX + "disconnect")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		public async Task Disconnect()
		{
			if ((Context.User.Id == Constants.OWNER_ID) || (Constants.DISCONNECT))
			{
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * .75)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendEmbedMessage(channel, embed: Actions.addFooter(Actions.makeNewEmbed(title: "Bot is disconnecting..."), "Disconnect")));
						}
					}
				}
				while (msgs.Any(x => x.CreatedAt == null))
				{
					Thread.Sleep(100);
				}
				await CommandHandler.client.SetStatus(UserStatus.Invisible);
				Environment.Exit(1);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("restart")]
		[Usage(Constants.BOT_PREFIX + "restart")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		public async Task Restart()
		{
			//Does not work, need to fix it
			if ((Context.User.Id == Constants.OWNER_ID) || (Constants.DISCONNECT))
			{
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * Constants.PERCENT_AVERAGE)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendEmbedMessage(channel, embed: Actions.addFooter(Actions.makeNewEmbed(title: "Bot is restarting..."), "Restart")));
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
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("leaveguild")]
		[Usage(Constants.BOT_PREFIX + "leaveguild")]
		[Summary("Makes the bot leave the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.Administrator, 0)]
		public async Task LeaveServer([Optional, Remainder] String input)
		{
			//Get the guild out of an ID
			ulong guildID = 0;
			if (UInt64.TryParse(input, out guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id.Equals(Constants.OWNER_ID))
				{
					SocketGuild guild = CommandHandler.client.GetGuild(guildID);
					if (guild == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
						return;
					}

					//Leave the server
					await guild.LeaveAsync();

					//Don't try to send a message if the guild left is the one the message was sent on
					if (Context.Guild == guild)
						return;

					await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
				}
			}
			//No input means to leave the current guild
			else if (input == null)
			{
				//Make sure it's the guild owner trying to kick the bot
				if (Context.Guild.OwnerId.Equals(Context.User.Id))
				{
					await Actions.sendChannelMessage(Context.Channel, "Bye.");
					await Context.Guild.LeaveAsync();
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("serverlog")]
		[Usage(Constants.BOT_PREFIX + "serverlog [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[PermissionRequirements(1U << (int)GuildPermission.Administrator, 0)]
		public async Task Serverlog([Remainder] String input)
		{
			ITextChannel serverlog = await Actions.setServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}
	}
}