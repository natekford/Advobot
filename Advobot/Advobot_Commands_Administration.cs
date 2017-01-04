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
	[Name("Administration")]
	public class Administration_Commands : ModuleBase
	{
		[Command("setgame")]
		[Alias("sg")]
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

			await CommandHandler.Client.SetGameAsync(input);
			await Actions.sendChannelMessage(Context.Channel, String.Format("Game set to `{0}`.", input));
		}

		[Command("disconnect")]
		[Alias("dc", "runescapeservers")]
		[Usage(Constants.BOT_PREFIX + "disconnect")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		public async Task Disconnect()
		{
			if (Context.User.Id == Constants.OWNER_ID || Constants.DISCONNECT)
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
				while (msgs.Any(x => x == null))
				{
					Thread.Sleep(100);
				}
				await CommandHandler.Client.SetStatusAsync(UserStatus.Invisible);
				Environment.Exit(1);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("restart")]
		[Alias("res")]
		[Usage(Constants.BOT_PREFIX + "restart")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		public async Task Restart()
		{
			//Does not work, need to fix it
			if (Context.User.Id == Constants.OWNER_ID || Constants.DISCONNECT)
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
				while (msgs.Any(x => x == null))
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

		[Command("listguilds")]
		[Usage(Constants.BOT_PREFIX + "listguilds")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[BotOwnerRequirement]
		public async Task ListGuilds()
		{
			//Initialize a string
			String info = "";

			//Go through each guild and add them to the list
			int count = 1;
			CommandHandler.Client.Guilds.ToList().ForEach(async x =>
			{
				IGuildUser owner = await x.GetOwnerAsync();
				info += String.Format("{0}. {1} ID: {2} Owner: {3}#{4} ID: {5}\n", count++.ToString("00"), x.Name, x.Id, owner.Username, owner.Discriminator, owner.Id);
			});

			//Make an embed and put the link to the hastebin in it
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Guilds", Actions.uploadToHastebin(info)));
		}

		[Command("leaveguild")]
		[Usage(Constants.BOT_PREFIX + "leaveguild <Guild ID>")]
		[Summary("Makes the bot leave the guild.")]
		[BotOwnerOrGuildOwnerRequirement]
		public async Task LeaveServer([Optional, Remainder] String input)
		{
			//Get the guild out of an ID
			ulong guildID = 0;
			if (UInt64.TryParse(input, out guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id.Equals(Constants.OWNER_ID))
				{
					SocketGuild guild = CommandHandler.Client.GetGuild(guildID);
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
				await Actions.sendChannelMessage(Context.Channel, "Bye.");
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("serverlog")]
		[Alias("slog")]
		[Usage(Constants.BOT_PREFIX + "serverlog [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[PermissionRequirements]
		public async Task Serverlog([Remainder] String input)
		{
			ITextChannel serverlog = await Actions.setServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}

		[Command("modlog")]
		[Alias("mlog")]
		[Usage(Constants.BOT_PREFIX + "modlog [#Channel|Off]")]
		[Summary("Puts the modlog on the specified channel. Modlof is a log of all commands used.")]
		[PermissionRequirements]
		public async Task Modlog([Remainder] String input)
		{
			ITextChannel modlog = await Actions.setServerOrModLog(Context, input, Constants.MOD_LOG_CHECK_STRING);
			if (modlog != null)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, modlog.Id));
			}
		}

		[Command("botchannel")]
		[Alias("bchan")]
		[Usage(Constants.BOT_PREFIX + "botchannel")]
		[Summary("Recreates the bot channel if lost for some reason.")]
		[PermissionRequirements]
		public async Task BotChannel()
		{
			//If no bot channel, create it
			if (!(await Context.Guild.GetTextChannelsAsync()).ToList().Any(x => x.Name == Variables.Bot_Name))
			{
				//Create the channel
				ITextChannel channel = await Context.Guild.CreateTextChannelAsync(Variables.Bot_Name);
				//Make it so not everyone can read it
				await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
			}
		}

		[Command("enablepreferences")]
		[Alias("eprefs")]
		[Usage(Constants.BOT_PREFIX + "enablepreferences")]
		[Summary("Gives the guild preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands.")]
		[GuildOwnerRequirement]
		public async Task EnablePreferences()
		{
			//Member limit
			if ((Context.Guild as SocketGuild).MemberCount < Constants.MEMBER_LIMIT && Context.User.Id != Constants.OWNER_ID)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Sorry, but this server is too small to warrant preferences. {0} or more members are required.",
					Constants.MEMBER_LIMIT));
				return;
			}

			//Confirmation of agreement
			await Actions.sendChannelMessage(Context.Channel, "By turning preferences on you will be enabling the ability to toggle commands, change who can use commands, " +
				"and many more features. This data will be stored in a text file off of the server, and whoever is hosting the bot will most likely have " +
				"access to it. A new text channel will be automatically created to display preferences and the server/mod log. If you agree to this, say `Yes`.");

			//Add them to the list for a few seconds
			Variables.GuildsEnablingPreferences.Add(Context.Guild);
			//Remove them
			Actions.turnOffEnableYes(Context.Guild);

			//The actual enabling happens in OnMessageReceived in Serverlogs
		}

		[Command("deletepreferences")]
		[Alias("dprefs")]
		[Usage(Constants.BOT_PREFIX + "deletepreferences")]
		[Summary("Deletes the preferences file.")]
		[GuildOwnerRequirement]
		public async Task DeletePreferences()
		{
			//Confirmation of agreement
			await Actions.sendChannelMessage(Context.Channel, "If you are sure you want to delete your preferences, say `Yes`.");

			//Add them to the list for a few seconds
			Variables.GuildsDeletingPreferences.Add(Context.Guild);
			//Remove them
			Actions.turnOffDeleteYes(Context.Guild);

			//The actual deleting happens in OnMessageReceived in Serverlogs
		}

		[Command("currentpreferences")]
		[Alias("cprefs")]
		[Usage(Constants.BOT_PREFIX + "currentpreferences")]
		[Summary("Gives the file containing the current preferences of the guild.")]
		[PermissionRequirements]
		public async Task CurrentPreferences()
		{
			await Actions.readPreferences(Context.Channel, Actions.getServerFilePath(Context.Guild.Id, Constants.PREFERENCES_FILE));
		}
	}
}