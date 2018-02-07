using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public class CommandHandler
	{
		private static string _Joiner = "\n" + new string(' ', 28);
		private IServiceProvider _Provider;
		private CommandService _Commands;
		private IDiscordClient _Client;
		private IBotSettings _BotSettings;
		private IGuildSettingsService _GuildSettings;
		private ITimersService _Timers;
		private ILogService _Logging;
		private bool _Loaded;

		public CommandHandler(IServiceProvider provider)
		{
			_Provider = provider;
			_Commands = _Provider.GetRequiredService<CommandService>();
			_Client = _Provider.GetRequiredService<IDiscordClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsService>();
			_Timers = _Provider.GetRequiredService<ITimersService>();
			_Logging = _Provider.GetRequiredService<ILogService>();

			switch (_Client)
			{
				case DiscordSocketClient socketClient:
					socketClient.Connected += OnConnected;
					socketClient.UserJoined += OnUserJoined;
					socketClient.UserLeft += OnUserLeft;
					socketClient.MessageReceived += OnMessageReceived;
					socketClient.MessageReceived += HandleCommand;
					return;
				case DiscordShardedClient shardedClient:
					shardedClient.Shards.Last().Connected += OnConnected;
					shardedClient.UserJoined += OnUserJoined;
					shardedClient.UserLeft += OnUserLeft;
					shardedClient.MessageReceived += OnMessageReceived;
					shardedClient.MessageReceived += HandleCommand;
					return;
				default:
					throw new ArgumentException("invalid type", nameof(_Client));
			}
		}

		private async Task OnConnected()
		{
			if (!_Loaded)
			{
				if (Config.Configuration[Config.ConfigDict.ConfigKey.BotId] != _Client.CurrentUser.Id.ToString())
				{
					Config.Configuration[Config.ConfigDict.ConfigKey.BotId] = _Client.CurrentUser.Id.ToString();
					Config.Save();
					ConsoleUtils.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
					ClientUtils.RestartBot();
				}

				await ClientUtils.UpdateGameAsync(_Client, _BotSettings).CAF();

				var startTime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;
				ConsoleUtils.WriteLine($"Current version: {Constants.BOT_VERSION}");
				ConsoleUtils.WriteLine($"Current bot prefix is: {_BotSettings.Prefix}");
				ConsoleUtils.WriteLine($"Bot took {startTime:n} milliseconds to start up.");
				_Loaded = true;
			}
		}
		private async Task OnUserJoined(SocketGuildUser user)
		{
			var settings = await _GuildSettings.GetOrCreateAsync(user.Guild);
			if (settings == null)
			{
				return;
			}

			//Banned names
			if (settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(user.Username)))
			{
				var giver = new PunishmentGiver(0, _Timers);
				await giver.PunishAsync(PunishmentType.Ban, user, null, ClientUtils.CreateRequestOptions("banned name")).CAF();
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
				await RoleUtils.GiveRolesAsync(user, roles, ClientUtils.CreateRequestOptions("persistent roles")).CAF();
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
			var settings = await _GuildSettings.GetOrCreateAsync(user.Guild);
			if (settings == null || user.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId])
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
			var guild = message.GetGuild();
			if (guild == null || _Timers == null || int.TryParse(message.Content, out var i) || i < 0 || i > 7 || !(message.Author is IGuildUser user))
			{
				return;
			}
			--i;

			var settings = await _GuildSettings.GetOrCreateAsync(guild);

			var quotes = await _Timers.RemoveActiveCloseQuoteAsync(user).CAF();
			var validQuotes = quotes != null && quotes.List.Count > i;
			if (validQuotes)
			{
				var quote = quotes.List[i].Word;
				var embed = new EmbedWrapper
				{
					Title = quote.Name,
					Description = quote.Description
				};
				embed.TryAddFooter("Quote", null, out _);
				await MessageUtils.SendEmbedMessageAsync(message.Channel, embed).CAF();
			}
			var helpEntries = await _Timers.RemoveActiveCloseHelpAsync(user).CAF();
			var validHelpEntries = helpEntries != null && helpEntries.List.Count > i;
			if (validHelpEntries)
			{
				var prefix = String.IsNullOrWhiteSpace(settings.Prefix) ? _BotSettings.Prefix : settings.Prefix;
				var help = helpEntries.List[i].Word;
				var embed = new EmbedWrapper
				{
					Title = help.Name,
					Description = help.ToString().Replace(Constants.PLACEHOLDER_PREFIX, prefix)
				};
				embed.TryAddFooter("Help", null, out _);
				await MessageUtils.SendEmbedMessageAsync(message.Channel, embed).CAF();
			}

			if (validQuotes || validHelpEntries)
			{
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("help entry or quote")).CAF();
			}
		}
		private async Task HandleCommand(SocketMessage message)
		{
			//Bot isn't paused and the message isn't a system message
			if (_BotSettings.Pause || !(message.Channel is IGuildChannel channel)
				|| !(message is SocketUserMessage userMessage) || String.IsNullOrWhiteSpace(userMessage.Content))
			{
				return;
			}
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			//Guild settings
			var settings = await _GuildSettings.GetOrCreateAsync(channel.Guild).CAF();
			if (settings == null)
			{
				return;
			}
			//Prefix
			var argPos = -1;
			if (!userMessage.HasStringPrefix(String.IsNullOrWhiteSpace(settings.Prefix) ? _BotSettings.Prefix : settings.Prefix, ref argPos) &&
				!userMessage.HasMentionPrefix(_Client.CurrentUser, ref argPos))
			{
				return;
			}

			IAdvobotCommandContext context;
			switch (_Client)
			{
				case DiscordSocketClient socketClient:
					context = new AdvobotSocketCommandContext(_Provider, settings, socketClient, userMessage);
					break;
				case DiscordShardedClient shardedClient:
					context = new AdvobotShardedCommandContext(_Provider, settings, shardedClient, userMessage);
					break;
				default:
					throw new ArgumentException("invalid type", nameof(_Client));
			}
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();

			if ((!result.IsSuccess && result.ErrorReason == null) || result.Error == CommandError.UnknownCommand)
			{
				return;
			}
			else if (result.IsSuccess)
			{
				_Logging.SuccessfulCommands.Increment();
				await MessageUtils.DeleteMessageAsync(context.Message, ClientUtils.CreateRequestOptions("logged command")).CAF();

				if (settings.ModLog != null && !settings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new EmbedWrapper
					{
						Description = context.Message.Content
					};
					embed.TryAddAuthor(context.User, out _);
					embed.TryAddFooter("Mod Log", null, out _);
					await MessageUtils.SendEmbedMessageAsync(settings.ModLog, embed).CAF();
				}
			}
			else
			{
				_Logging.FailedCommands.Increment();
				await MessageUtils.SendErrorMessageAsync(context, new Error(result.ErrorReason)).CAF();
			}

			var response = $"Guild: {context.Guild.Format()}" +
				$"{_Joiner}Channel: {context.Channel.Format()}" +
				$"{_Joiner}User: {context.User.Format()}" +
				$"{_Joiner}Time: {context.Message.CreatedAt.UtcDateTime.ToReadable()}" +
				$"{_Joiner}Text: {context.Message.Content}" +
				$"{_Joiner}Time taken: {stopwatch.ElapsedMilliseconds}ms";
			response += result.ErrorReason == null ? "" : $"{_Joiner}Error: {result.ErrorReason}";

			ConsoleUtils.WriteLine(response, color: result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			_Logging.AttemptedCommands.Increment();
		}
	}
}
