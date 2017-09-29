﻿using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Modules.GuildSettings;
using Advobot.Modules.Log;
using Advobot.Modules.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	public class CommandHandler
	{
		private static IServiceProvider _Provider;
		private static CommandService _Commands;
		private static IBotSettings _BotSettings;
		private static IGuildSettingsModule _GuildSettings;
		private static IDiscordClient _Client;
		private static ITimersModule _Timers;
		private static ILogModule _Logging;
		private static bool _Loaded;

		/// <summary>
		/// Sets variables and some events up.
		/// </summary>
		/// <param name="provider"></param>
		public static async Task<IDiscordClient> Install(IServiceProvider provider)
		{
			_Provider		= provider;
			_Commands		= _Provider.GetService<CommandService>();
			_BotSettings	= _Provider.GetService<IBotSettings>();
			_GuildSettings	= _Provider.GetService<IGuildSettingsModule>();
			_Client			= _Provider.GetService<IDiscordClient>();
			_Timers			= _Provider.GetService<ITimersModule>();
			_Logging		= _Provider.GetService<ILogModule>();

			//Use executing assembly to get all of the commands from the core. Entry and Calling assembly give the launcher
			await _Commands.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly());

			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.MessageReceived += HandleCommand;
				socketClient.Connected += OnConnected;
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.MessageReceived += HandleCommand;
				shardedClient.Shards.Last().Connected += OnConnected;
			}
			else
			{
				throw new ArgumentException($"Invalid client supplied. Must be {nameof(DiscordSocketClient)} or {nameof(DiscordShardedClient)}.");
			}

			Punishments.Install(provider);

			return _Client;
		}

		/// <summary>
		/// Says some start up messages, updates the game, and restarts the bot if this is the first instance of the bot starting up.
		/// </summary>
		/// <returns></returns>
		private static async Task OnConnected()
		{
			if (_Loaded)
			{
				return;
			}

			if (Config.Configuration[Config.ConfigKeys.Bot_Id] != _Client.CurrentUser.Id.ToString())
			{
				Config.Configuration[Config.ConfigKeys.Bot_Id] = _Client.CurrentUser.Id.ToString();
				Config.Save();
				ConsoleActions.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				ClientActions.RestartBot();
			}

			await ClientActions.UpdateGameAsync(_Client, _BotSettings);

			ConsoleActions.WriteLine("The current bot prefix is: " + _BotSettings.Prefix);
			ConsoleActions.WriteLine($"Bot took {DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds:n} milliseconds to start up.");
			_Loaded = true;
		}

		/// <summary>
		/// Attempts to find a matching command and fire it.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private static async Task HandleCommand(SocketMessage message)
		{
			//Bot isn't paused and the message isn't a system message
			var loggedCommand = new LoggedCommand();
			if (_BotSettings.Pause || !(message is SocketUserMessage userMessage))
			{
				return;
			}

			//Guild settings
			var guildSettings = await _GuildSettings.GetOrCreateSettings(message.Channel.GetGuild());
			if (guildSettings == null)
			{
				return;
			}

			//Prefix
			var argPos = -1;
			if (!userMessage.HasMentionPrefix(_Client.CurrentUser, ref argPos)
				&& !userMessage.HasStringPrefix(GetActions.GetPrefix(_BotSettings, guildSettings), ref argPos))
			{
				return;
			}

			var context = new MyCommandContext(_Provider, guildSettings, _Client, userMessage);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider);
			await LogCommand(loggedCommand, context, result);
		}
		/// <summary>
		/// Prints the status of the command to the console.
		/// </summary>
		/// <param name="loggedCommand"></param>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private static async Task LogCommand(LoggedCommand loggedCommand, IMyCommandContext context, IResult result)
		{
			//Success
			if (result.IsSuccess)
			{
				_Logging.SuccessfulCommands.Increment();
				await MessageActions.DeleteMessage(context.Message);

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog != null && guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = EmbedActions.MakeNewEmbed(null, context.Message.Content)
						.MyAddAuthor(context.User)
						.MyAddFooter("Mod Log");
					await MessageActions.SendEmbedMessage(guildSettings.ModLog, embed);
				}
			}
			//Failure in a valid fail way
			else if (loggedCommand.ErrorReason != null)
			{
				_Logging.FailedCommands.Increment();
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR(loggedCommand.ErrorReason));
			}
			//Failure in a way that doesn't need to get logged (unknown command, etc)
			else
			{
				return;
			}

			loggedCommand.FinalizeAndWrite(context, result, _Logging);
		}
	}
}