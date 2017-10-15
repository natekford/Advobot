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

[assembly: CommandAssembly]
namespace Advobot.Commands
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

			_Commands.AddTypeReader(typeof(IInvite), new InviteTypeReader());
			_Commands.AddTypeReader(typeof(IBan), new BanTypeReader());
			_Commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
			_Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			_Commands.AddTypeReader(typeof(CommandSwitch), new CommandSwitchTypeReader());

			//Add in all custom argument typereaders
			var customArgumentsClasses = Assembly.GetAssembly(typeof(CustomArguments<>)).GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<CustomArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(CustomArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(CustomArgumentsTypeReader<>).MakeGenericType(c));
				_Commands.AddTypeReader(t, tr);
			}

			//Use executing assembly to get all of the commands from the core. Entry and Calling assembly give the launcher
			await _Commands.AddModulesAsync(Assembly.GetExecutingAssembly());

			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.Connected += OnConnected;
				socketClient.UserJoined += OnUserJoined;
				socketClient.UserLeft += OnUserLeft;
				socketClient.MessageReceived += OnMessageReceived;
				socketClient.MessageReceived += HandleCommand;
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.Shards.Last().Connected += OnConnected;
				shardedClient.UserJoined += OnUserJoined;
				shardedClient.UserLeft += OnUserLeft;
				shardedClient.MessageReceived += OnMessageReceived;
				shardedClient.MessageReceived += HandleCommand;
			}
			else
			{
				throw new ArgumentException($"Invalid client supplied. Must be {nameof(DiscordSocketClient)} or {nameof(DiscordShardedClient)}.");
			}

			return _Client;
		}

		/// <summary>
		/// Fires the <see cref="EventActions.OnConnected(IDiscordClient, IBotSettings)"/> method.
		/// </summary>
		/// <returns></returns>
		private static async Task OnConnected()
		{
			if (!_Loaded)
			{
				await EventActions.OnConnected(_Client, _BotSettings);
				_Loaded = true;
			}
		}
		/// <summary>
		/// Fires the <see cref="EventActions.OnUserJoined(SocketGuildUser, IGuildSettings, ITimersService)"/> method.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private static async Task OnUserJoined(SocketGuildUser user)
		{
			await EventActions.OnUserJoined(user, await _GuildSettings.GetOrCreateSettings(user.Guild), _Timers);
		}
		/// <summary>
		/// Fires the <see cref="EventActions.OnUserLeft(SocketGuildUser, IGuildSettings, ITimersService)"/> method.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private static async Task OnUserLeft(SocketGuildUser user)
		{
			await EventActions.OnUserLeft(user, await _GuildSettings.GetOrCreateSettings(user.Guild), _Timers);
		}
		/// <summary>
		/// Fires the <see cref="EventActions.OnMessageReceived(SocketMessage, ITimersService)"/> method.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private static async Task OnMessageReceived(SocketMessage message)
		{
			await EventActions.OnMessageReceived(message, _Timers);
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
				await MessageActions.DeleteMessageAsync(context.Message);

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog != null && !guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new MyEmbed(null, context.Message.Content)
						.AddAuthor(context.User)
						.AddFooter("Mod Log");
					await MessageActions.SendEmbedMessageAsync(guildSettings.ModLog, embed);
				}
			}
			//Failure in a valid fail way
			else if (loggedCommand.ErrorReason != null)
			{
				_Logging.FailedCommands.Increment();
				await MessageActions.SendErrorMessageAsync(context, new ErrorReason(loggedCommand.ErrorReason));
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