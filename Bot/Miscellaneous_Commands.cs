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
		[Usage(BOT_PREFIX + "help [Command]")]
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
		[Summary("Shows the ID of the server.")]
		public async Task Say()
		{
			await Context.Channel.SendMessageAsync(String.Format("This server has the ID `{0}`.", Context.Guild.Id) + " ");
		}
	}
}
