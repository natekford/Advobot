using Advobot.Core.Actions;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

[assembly: CommandAssembly]
//Something has to be referenced in this assembly so that the attribute
//gets picked up and can then be used to get the commands out of this
//assembly, so that's why the commandhandler is in here and not in core.
namespace Advobot.Commands
{
	public static class CommandHandler
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
		public static IDiscordClient Install(IServiceProvider provider)
		{
			if (_Loaded)
			{
				return _Client;
			}

			_Provider = provider;
			_Commands = _Provider.GetRequiredService<CommandService>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsService>();
			_Client = _Provider.GetRequiredService<IDiscordClient>();
			_Timers = _Provider.GetRequiredService<ITimersService>();
			_Logging = _Provider.GetRequiredService<ILogService>();

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

		private static async Task OnConnected()
		{
			if (!_Loaded)
			{
				await EventActions.OnConnected(_Client, _BotSettings).CAF();
				_Loaded = true;
			}
		}
		private static async Task OnUserJoined(SocketGuildUser user)
		{
			await EventActions.OnUserJoined(user, _BotSettings, await _GuildSettings.GetOrCreateSettings(user.Guild), _Timers).CAF();
		}
		private static async Task OnUserLeft(SocketGuildUser user)
		{
			await EventActions.OnUserLeft(user, _BotSettings, await _GuildSettings.GetOrCreateSettings(user.Guild), _Timers).CAF();
		}
		private static async Task OnMessageReceived(SocketMessage message)
		{
			await EventActions.OnMessageReceived(message, _BotSettings, await _GuildSettings.GetOrCreateSettings(message.GetGuild()), _Timers).CAF();
		}

		private static async Task HandleCommand(SocketMessage message)
		{
			//Bot isn't paused and the message isn't a system message
			var loggedCommand = new LoggedCommand();
			if (_BotSettings.Pause || !(message is SocketUserMessage userMessage))
			{
				return;
			}

			//Guild settings
			var guildSettings = await _GuildSettings.GetOrCreateSettings(message.Channel.GetGuild()).CAF();
			if (guildSettings == null)
			{
				return;
			}

			//Prefix
			var argPos = -1;
			if (true
				&& !userMessage.HasMentionPrefix(_Client.CurrentUser, ref argPos)
				&& !userMessage.HasStringPrefix(GetActions.GetPrefix(_BotSettings, guildSettings), ref argPos))
			{
				return;
			}

			var context = new AdvobotCommandContext(_Provider, guildSettings, _Client, userMessage);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();
			await loggedCommand.LogCommand(context, result, _Logging).CAF();
		}
	}
}

