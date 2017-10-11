using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;

namespace Advobot
{
	public class CommandHandler
	{
		private static IServiceProvider _Provider;
		private static CommandService _Commands;
		private static IBotSettings _BotSettings;
		private static IGuildSettingsService _GuildSettings;
		private static IDiscordClient _Client;
		private static ITimersService _Timers;
		private static ILogService _Logging;
		private static bool _Loaded;

		/// <summary>
		/// Sets variables and some events up.
		/// </summary>
		/// <param name="provider"></param>
		public static async Task<IDiscordClient> Install(IServiceProvider provider)
		{
			_Provider = provider;
			_Commands = _Provider.GetService<CommandService>();
			_BotSettings = _Provider.GetService<IBotSettings>();
			_GuildSettings = _Provider.GetService<IGuildSettingsService>();
			_Client = _Provider.GetService<IDiscordClient>();
			_Timers = _Provider.GetService<ITimersService>();
			_Logging = _Provider.GetService<ILogService>();

			AddTypeReaders();

			//Use executing assembly to get all of the commands from the core. Entry and Calling assembly give the launcher
			await _Commands.AddModulesAsync(Assembly.GetExecutingAssembly());

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

			return _Client;
		}
		private static void AddTypeReaders()
		{
			_Commands.AddTypeReader(typeof(IInvite), new InviteTypeReader());
			_Commands.AddTypeReader(typeof(IBan), new BanTypeReader());
			_Commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
			_Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			_Commands.AddTypeReader(typeof(CommandSwitch), new CommandSwitchTypeReader());
			_Commands.AddTypeReader(typeof(ImageUrl), new ImageUrlTypeReader());

			//Add in all custom argument typereaders
			var customArgumentsClasses = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<CustomArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(CustomArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(CustomArgumentsTypeReader<>).MakeGenericType(c));
				_Commands.AddTypeReader(t, tr);
			}
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

			var context = new AdvobotCommandContext(_Provider, guildSettings, _Client, userMessage);
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
		private static async Task LogCommand(LoggedCommand loggedCommand, IAdvobotCommandContext context, IResult result)
		{
			loggedCommand.Finalize(context, result);

			//Success
			if (result.IsSuccess)
			{
				_Logging.SuccessfulCommands.Increment();
				await MessageActions.DeleteMessage(context.Message);

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog != null && !guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new MyEmbed(null, context.Message.Content)
						.AddAuthor(context.User)
						.AddFooter("Mod Log");
					await MessageActions.SendEmbedMessage(guildSettings.ModLog, embed);
				}
			}
			//Failure in a valid fail way
			else if (loggedCommand.ErrorReason != null)
			{
				_Logging.FailedCommands.Increment();
				await MessageActions.SendErrorMessage(context, new ErrorReason(loggedCommand.ErrorReason));
			}
			//Failure in a way that doesn't need to get logged (unknown command, etc)
			else
			{
				return;
			}

			loggedCommand.Write();
			_Logging.RanCommands.Add(loggedCommand);
		}
	}
}