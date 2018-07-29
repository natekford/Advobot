using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles user input commands.
	/// </summary>
	public sealed class CommandHandler
	{
		private readonly IServiceProvider _Provider;
		private readonly CommandService _Commands;
		private readonly HelpEntryHolder _HelpEntries;
		private readonly DiscordShardedClient _Client;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsService _GuildSettings;
		private readonly ITimersService _Timers;
		private readonly ILogService _Logging;
		private bool _Loaded;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandler"/> and gets the required services.
		/// </summary>
		/// <param name="provider"></param>
		public CommandHandler(IServiceProvider provider)
		{
			_Provider = provider;
			_Commands = _Provider.GetRequiredService<CommandService>();
			_HelpEntries = _Provider.GetRequiredService<HelpEntryHolder>();
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsService>();
			_Timers = _Provider.GetRequiredService<ITimersService>();
			_Logging = _Provider.GetRequiredService<ILogService>();

			_Client.ShardConnected += OnConnected;
			_Client.UserJoined += OnUserJoined;
			_Client.UserLeft += OnUserLeft;
			_Client.MessageReceived += OnMessageReceived;
			_Client.MessageReceived += HandleCommand;
		}

		private async Task OnConnected(DiscordSocketClient client)
		{
			if (!_Loaded)
			{
				if (LowLevelConfig.Config.BotId != _Client.CurrentUser.Id)
				{
					LowLevelConfig.Config.BotId = _Client.CurrentUser.Id;
					LowLevelConfig.Config.Save();
					ConsoleUtils.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
					await ClientUtils.RestartBotAsync(client).CAF();
				}

				await ClientUtils.UpdateGameAsync(_Client, _BotSettings).CAF();

				var startTime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;
				ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; Prefix: {_BotSettings.Prefix}; Launch Time: {startTime:n}ms");
				_Timers.Start();
				_Loaded = true;
			}
		}
		private async Task OnUserJoined(SocketGuildUser user)
		{
			var settings = await _GuildSettings.GetOrCreateAsync(user.Guild).CAF();
			if (settings == null)
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
			var antiRaid = settings.RaidPreventionDictionary[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(settings, user).CAF();
			}
			var antiJoin = settings.RaidPreventionDictionary[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
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
		private async Task OnUserLeft(SocketGuildUser user)
		{
			//Check if the bot was the one that left
			var settings = await _GuildSettings.GetOrCreateAsync(user.Guild).CAF();
			if (settings == null || user.Id == LowLevelConfig.Config.BotId)
			{
				return;
			}

			//Goodbye message
			if (settings.GoodbyeMessage != null)
			{
				await settings.GoodbyeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
		private async Task OnMessageReceived(SocketMessage message)
		{
			if (!(message.Author is SocketGuildUser user) || _Timers == null || !int.TryParse(message.Content, out var i) || i < 0 || i > 7)
			{
				return;
			}
			--i;

			var settings = await _GuildSettings.GetOrCreateAsync(user.Guild).CAF();
			var quotes = await _Timers.RemoveActiveCloseQuoteAsync(user).CAF();
			var validQuotes = quotes != null && quotes.List.Count > i;
			if (validQuotes)
			{
				var quote = quotes.List[i];
				var embed = new EmbedWrapper
				{
					Title = quote.Name,
					Description = quote.Text,
				};
				embed.TryAddFooter("Quote", null, out _);
				await MessageUtils.SendMessageAsync(message.Channel, null, embed).CAF();
			}
			var helpEntries = await _Timers.RemoveActiveCloseHelpAsync(user).CAF();
			var validHelpEntries = helpEntries != null && helpEntries.List.Count > i;
			if (validHelpEntries)
			{
				var prefix = String.IsNullOrWhiteSpace(settings.Prefix) ? _BotSettings.Prefix : settings.Prefix;
				var help = helpEntries.List[i];
				var embed = new EmbedWrapper
				{
					Title = help.Name,
					Description = help.Text.Replace(Constants.PLACEHOLDER_PREFIX, prefix),
				};
				embed.TryAddFooter("Help", null, out _);
				await MessageUtils.SendMessageAsync(message.Channel, null, embed).CAF();
			}

			if (validQuotes || validHelpEntries)
			{
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("help entry or quote")).CAF();
			}
		}
		private async Task HandleCommand(SocketMessage message)
		{
			var argPos = -1;
			if (_BotSettings.Pause
				|| String.IsNullOrWhiteSpace(message.Content)
				|| !(message is SocketUserMessage uMsg)
				|| !(uMsg.Author is SocketGuildUser user)
				|| user.IsBot
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| !(uMsg.HasStringPrefix(_BotSettings.InternalGetPrefix(settings), ref argPos) && !uMsg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			var context = new AdvobotShardedCommandContext(_Provider, settings, _Client, uMsg);
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
