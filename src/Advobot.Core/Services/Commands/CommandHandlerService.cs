using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
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
		private readonly DiscordShardedClient _Client;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsFactory _GuildSettings;
		private bool _Loaded;

		/// <inheritdoc />
		public event LogCounterIncrementEventHandler LogCounterIncrement;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandlerService"/> and gets the required services.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="commands"></param>
		public CommandHandlerService(IIterableServiceProvider provider, IEnumerable<Assembly> commands)
		{
			_Provider = provider;
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsFactory>();

			_Commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				ThrowOnError = false,
			});
			_Commands.AddTypeReader<IInvite>(new InviteTypeReader());
			_Commands.AddTypeReader<IBan>(new BanTypeReader());
			_Commands.AddTypeReader<IWebhook>(new WebhookTypeReader());
			_Commands.AddTypeReader<IHelpEntry>(new HelpEntryTypeReader());
			_Commands.AddTypeReader<Emote>(new EmoteTypeReader());
			_Commands.AddTypeReader<GuildEmote>(new GuildEmoteTypeReader());
			_Commands.AddTypeReader<Color>(new ColorTypeReader());
			_Commands.AddTypeReader<Uri>(new UriTypeReader());
			_Commands.AddTypeReader<ModerationReason>(new ModerationReasonTypeReader());
			_Commands.AddTypeReader<Quote>(new QuoteTypeReader());
			//Add in generic custom argument type readers
			var customArgumentsClasses = Assembly.GetAssembly(typeof(NamedArguments<>)).GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<NamedArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(NamedArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(NamedArgumentsTypeReader<>).MakeGenericType(c));
				_Commands.AddTypeReader(t, tr);
			}

			_Client.ShardReady += (client) => OnReady(client, commands);
			_Client.MessageReceived += HandleCommand;
		}

		/// <summary>
		/// Fires the log counter increment event.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="count"></param>
		private void NotifyLogCounterIncrement(string name, int count)
			=> LogCounterIncrement?.Invoke(this, new LogCounterIncrementEventArgs(name, count));
		/// <summary>
		/// Handles the bot using the correct settings, the game displayed, and the timers starting.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="commands"></param>
		/// <returns></returns>
		private async Task OnReady(DiscordSocketClient client, IEnumerable<Assembly> commands)
		{
			if (_Loaded)
			{
				return;
			}
			_Loaded = true;

			await ClientUtils.UpdateGameAsync(client, _BotSettings).CAF();
			//Add in all the commands
			foreach (var assembly in commands)
			{
				await _Commands.AddModulesAsync(assembly, _Provider).CAF();
			}
			//await CreateSettingModificationModuleAsync<IBotSettings, BotSettings>(_Provider, _Commands).CAF();
			_Provider.GetRequiredService<IHelpEntryService>().Add(_Commands.Modules);

			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; " +
				$"Modules: {_Commands.Modules.Count()}; " +
				$"Prefix: {_BotSettings.Prefix}; " +
				$"Launch Time: {ProcessInfoUtils.GetUptime().TotalMilliseconds:n}ms");
		}
		/// <inheritdoc />
		public async Task HandleCommand(SocketMessage message)
		{
			var argPos = -1;
			if (!_Loaded
				|| _BotSettings.Pause
				|| _BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id)
				|| message.Author.IsBot
				|| string.IsNullOrWhiteSpace(message.Content)
				|| !(message is SocketUserMessage msg)
				|| !(msg.Author is SocketGuildUser user)
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| !(msg.HasStringPrefix(_BotSettings.InternalGetPrefix(settings), ref argPos)
						|| msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			var context = new AdvobotCommandContext(_Provider, settings, _Client, msg);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();

			if ((!result.IsSuccess && result.ErrorReason == null) || result.Error == CommandError.UnknownCommand)
			{
				return;
			}
			if (result.IsSuccess)
			{
				NotifyLogCounterIncrement(nameof(ILogService.SuccessfulCommands), 1);
				await MessageUtils.DeleteMessageAsync(context.Message, ClientUtils.CreateRequestOptions("logged command")).CAF();
				if (context.GuildSettings.ModLogId != 0 && !context.GuildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new EmbedWrapper
					{
						Description = context.Message.Content
					};
					embed.TryAddAuthor(context.User, out _);
					embed.TryAddFooter("Mod Log", null, out _);
					await MessageUtils.SendMessageAsync(context.Guild.GetTextChannel(context.GuildSettings.ModLogId), null, embed).CAF();
				}
			}
			else
			{
				NotifyLogCounterIncrement(nameof(ILogService.FailedCommands), 1);
				await MessageUtils.SendErrorMessageAsync(context, new Error(result.ErrorReason)).CAF();
			}

			ConsoleUtils.WriteLine(context.ToString(result), result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			NotifyLogCounterIncrement(nameof(ILogService.AttemptedCommands), 1);
		}
	}
}
