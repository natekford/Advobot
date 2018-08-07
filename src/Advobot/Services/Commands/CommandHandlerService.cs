using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.Commands
{
	/// <summary>
	/// Handles user input commands.
	/// </summary>
	internal sealed class CommandHandlerService : ICommandHandlerService
	{
		private readonly IServiceProvider _Provider;
		private readonly CommandService _Commands;
		private readonly HelpEntryHolder _HelpEntries;
		private readonly DiscordShardedClient _Client;
		private readonly ILowLevelConfig _Config;
		private readonly IBotSettings _BotSettings;
		private readonly ILevelService _Levels;
		private readonly IGuildSettingsService _GuildSettings;
		private readonly ITimerService _Timers;
		private readonly ILogService _Logging;
		private bool _Loaded;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandlerService"/> and gets the required services.
		/// </summary>
		/// <param name="provider"></param>
		internal CommandHandlerService(IServiceProvider provider)
		{
			_Provider = provider;
			_Commands = _Provider.GetRequiredService<CommandService>();
			_HelpEntries = _Provider.GetRequiredService<HelpEntryHolder>();
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_Config = _Provider.GetRequiredService<ILowLevelConfig>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_Levels = _Provider.GetRequiredService<ILevelService>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsService>();
			_Timers = _Provider.GetRequiredService<ITimerService>();
			_Logging = _Provider.GetRequiredService<ILogService>();

			_Client.ShardReady += OnReady;
			_Client.MessageReceived += HandleCommand;
		}

		/// <summary>
		/// Handles the bot using the correct settings, the game displayed, and the timers starting.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		private async Task OnReady(DiscordSocketClient client)
		{
			if (_Loaded)
			{
				return;
			}

			await ClientUtils.UpdateGameAsync(client, _BotSettings).CAF();
			//Start everything which uses a database now that we know we're using the correct bot id.
			foreach (var field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.GetValue(this) is IUsesDatabase temp)
				{
					temp.Start();
				}
			}

			_Loaded = true;
			var startTime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;
			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; Prefix: {_BotSettings.Prefix}; Launch Time: {startTime:n}ms");
		}
		/// <inheritdoc />
		public async Task HandleCommand(SocketMessage message)
		{
			var argPos = -1;
			if (!_Loaded
				|| _BotSettings.Pause
				|| String.IsNullOrWhiteSpace(message.Content)
				|| !(message is SocketUserMessage uMsg)
				|| !(uMsg.Author is SocketGuildUser user)
				|| user.IsBot
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| !(uMsg.HasStringPrefix(_BotSettings.InternalGetPrefix(settings), ref argPos) && !uMsg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			var context = new AdvobotCommandContext(_Provider, settings, _Client, uMsg);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();

			if ((!result.IsSuccess && result.ErrorReason == null) || result.Error == CommandError.UnknownCommand)
			{
				return;
			}
			else if (result.IsSuccess)
			{
				_Logging.SuccessfulCommands.Add(1);
				await MessageUtils.DeleteMessageAsync(context.Message, ClientUtils.CreateRequestOptions("logged command")).CAF();

				if (settings.ModLogId != 0 && !settings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new EmbedWrapper
					{
						Description = context.Message.Content
					};
					embed.TryAddAuthor(context.User, out _);
					embed.TryAddFooter("Mod Log", null, out _);
					await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ModLogId), null, embed).CAF();
				}
			}
			else
			{
				_Logging.FailedCommands.Add(1);
				await MessageUtils.SendErrorMessageAsync(context, new Error(result.ErrorReason)).CAF();
			}

			ConsoleUtils.WriteLine(context.ToString(result), result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			_Logging.AttemptedCommands.Add(1);
		}
	}
}
