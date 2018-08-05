using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Settings;
using Advobot.Enums;
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
		/// <inheritdoc />
		public event Func<BaseSocketClient, Task> RestartRequired;

		private readonly IServiceProvider _Provider;
		private readonly CommandService _Commands;
		private readonly HelpEntryHolder _HelpEntries;
		private readonly DiscordShardedClient _Client;
		private readonly LowLevelConfig _Config;
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
			_Config = _Provider.GetRequiredService<LowLevelConfig>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_Levels = _Provider.GetRequiredService<ILevelService>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsService>();
			_Timers = _Provider.GetRequiredService<ITimerService>();
			_Logging = _Provider.GetRequiredService<ILogService>();

			_Client.ShardReady += OnReady;
			_Client.UserJoined += OnUserJoined;
			_Client.UserLeft += OnUserLeft;
			_Client.MessageReceived += OnMessageReceived;
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
			if (_Config.BotId != client.CurrentUser.Id)
			{
				_Config.BotId = client.CurrentUser.Id;
				_Config.Save();
				ConsoleUtils.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				RestartRequired?.Invoke(_Client);
				_Loaded = false;
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
		/// <summary>
		/// Handles banned names, anti raid, persistent roles, and the welcome message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task OnUserJoined(SocketGuildUser user)
		{
			if (!_Loaded || !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings))
			{
				return;
			}
			//Banned names
			if (settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(user.Username)))
			{
				var giver = new Punisher(TimeSpan.FromMinutes(0), _Timers);
				await giver.GiveAsync(Punishment.Ban, user.Guild, user.Id, 0, ClientUtils.CreateRequestOptions("banned name")).CAF();
			}
			//Antiraid
			if (settings.RaidPreventionDictionary[RaidType.Regular] is RaidPreventionInfo antiRaid && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(settings, user).CAF();
			}
			if (settings.RaidPreventionDictionary[RaidType.RapidJoins] is RaidPreventionInfo antiJoin && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt?.UtcDateTime ?? default);
				if (antiJoin.GetSpamCount() >= antiJoin.UserCount)
				{
					await antiJoin.PunishAsync(settings, user).CAF();
				}
			}
			//Persistent roles
			var roles = settings.PersistentRoles.Where(x => x.UserId == user.Id)
				.Select(x => user.Guild.GetRole(x.RoleId)).Where(x => x != null).ToList();
			if (roles.Any())
			{
				await user.AddRolesAsync(roles, ClientUtils.CreateRequestOptions("persistent roles")).CAF();
			}
			//Welcome message
			if (settings.WelcomeMessage != null)
			{
				await settings.WelcomeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
		/// <summary>
		/// Handles the goodbye message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task OnUserLeft(SocketGuildUser user)
		{
			//Check if the bot was the one that left
			if (!_Loaded
				|| user.Id == _Config.BotId
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings))
			{
				return;
			}
			//Goodbye message
			if (settings.GoodbyeMessage != null)
			{
				await settings.GoodbyeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
		/// <summary>
		/// Handles close quotes and help entries.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task OnMessageReceived(SocketMessage message)
		{
			if (!_Loaded
				|| _Timers == null
				|| !int.TryParse(message.Content, out var i) || i < 0 || i > 7
				|| !((message.Channel as SocketTextChannel)?.Guild is SocketGuild guild)
				|| !(await _GuildSettings.GetOrCreateAsync(guild).CAF() is IGuildSettings settings))
			{
				return;
			}
			--i;

			var deleteMessage = false;
			if (await _Timers.RemoveActiveCloseQuoteAsync(guild.Id, message.Author.Id).CAF() is CloseQuotes q && q.List.Count > i)
			{
				var embed = new EmbedWrapper
				{
					Title = q.List[i].Name,
					Description = q.List[i].Text,
				};
				embed.TryAddFooter("Quote", null, out _);
				await MessageUtils.SendMessageAsync(message.Channel, null, embed).CAF();
				deleteMessage = true;
			}
			if (await _Timers.RemoveActiveCloseHelpAsync(guild.Id, message.Author.Id).CAF() is CloseHelpEntries h && h.List.Count > i)
			{
				var embed = new EmbedWrapper
				{
					Title = h.List[i].Name,
					Description = h.List[i].Text.Replace(Constants.PLACEHOLDER_PREFIX, _BotSettings.InternalGetPrefix(settings)),
				};
				embed.TryAddFooter("Help", null, out _);
				await MessageUtils.SendMessageAsync(message.Channel, null, embed).CAF();
				deleteMessage = true;
			}
			if (deleteMessage)
			{
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("help entry or quote")).CAF();
			}
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
