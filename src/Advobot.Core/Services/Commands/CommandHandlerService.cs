using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Services.Timers;
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
		private static readonly RequestOptions _Options = DiscordUtils.GenerateRequestOptions("Command successfully executed.");
		private static readonly TimeSpan _RemovalTime = TimeSpan.FromSeconds(10);

		private readonly IServiceProvider _Provider;
		private readonly DiscordShardedClient _Client;
		private readonly CommandServiceConfig _CommandConfig;
		private readonly Localized<CommandService> _CommandService;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IHelpEntryService _HelpEntries;
		private readonly ITimerService _Timers;
		private bool _Loaded;
		private ulong _OwnerId;

		/// <inheritdoc />
		public event Action<IResult> CommandInvoked;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandlerService"/> and gets the required services.
		/// </summary>
		/// <param name="provider"></param>
		public CommandHandlerService(IServiceProvider provider)
		{
			_Provider = provider;
			_CommandConfig = _Provider.GetRequiredService<CommandServiceConfig>();
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsFactory>();
			_HelpEntries = _Provider.GetRequiredService<IHelpEntryService>();
			_Timers = _Provider.GetRequiredService<ITimerService>();
			_CommandService = new Localized<CommandService>(x =>
			{
				var commands = new CommandService(_CommandConfig);
				commands.Log += OnLog;
				commands.CommandExecuted += OnCommandExecuted;
				return commands;
			});

			_Client.ShardReady += OnReady;
			_Client.MessageReceived += HandleCommand;
		}

		public async Task AddCommandsAsync(IEnumerable<CommandAssembly> aseemblies)
		{
			var currentCulture = CultureInfo.CurrentUICulture;
			foreach (var assembly in aseemblies)
			{
				if (assembly.Attribute.Instantiator != null)
				{
					await assembly.Attribute.Instantiator.ConfigureServicesAsync(_Provider).CAF();
				}

				foreach (var culture in assembly.Attribute.SupportedCultures)
				{
					CultureInfo.CurrentUICulture = culture;

					var commands = _CommandService.Get();
					var typeReaders = new[] { Assembly.GetExecutingAssembly(), assembly.Assembly }
						.SelectMany(x => x.GetTypes())
						.Select(x => (Attribute: x.GetCustomAttribute<TypeReaderTargetTypeAttribute>(), Type: x))
						.Where(x => x.Attribute != null);
					foreach (var typeReader in typeReaders)
					{
						var instance = (TypeReader)Activator.CreateInstance(typeReader.Type);
						foreach (var type in typeReader.Attribute.TargetTypes)
						{
							commands.AddTypeReader(type, instance, true);
						}
					}

					var modules = await commands.AddModulesAsync(assembly.Assembly, _Provider).CAF();
					var count = 0;
					foreach (var category in modules)
					{
						++count;
						foreach (var command in category.Submodules)
						{
							_HelpEntries.Add(new HelpEntry(command));
						}
					}

					ConsoleUtils.WriteLine($"Successfully loaded {count} command modules " +
						$"from {assembly.Assembly.GetName().Name} in the {culture} culture.");
				}
			}
			CultureInfo.CurrentUICulture = currentCulture;
		}
		private async Task OnReady(DiscordSocketClient _)
		{
			if (_Loaded)
			{
				return;
			}
			_Loaded = true;

			_OwnerId = await _Client.GetOwnerIdAsync().CAF();
			await _Client.UpdateGameAsync(_BotSettings).CAF();
			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; " +
				$"Prefix: {_BotSettings.Prefix}; " +
				$"Launch Time: {ProcessInfoUtils.GetUptime().TotalMilliseconds:n}ms");
		}
		private async Task HandleCommand(IMessage message)
		{
			var argPos = -1;
			if (!_Loaded || _BotSettings.Pause || message.Author.IsBot || string.IsNullOrWhiteSpace(message.Content)
				//Disallow running commands if the user is blocked, unless the owner of the bot blocks themselves either accidentally or idiotically
				|| (_BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id) && message.Author.Id != _OwnerId)
				|| !(message is IUserMessage msg)
				|| !(msg.Author is IGuildUser user)
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredCommandChannels.Contains(msg.Channel.Id)
				|| !(msg.HasStringPrefix(settings.GetPrefix(_BotSettings), ref argPos)
					|| msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settings.Culture);
			var context = new AdvobotCommandContext(settings, _Client, (SocketUserMessage)msg);
			var commands = _CommandService.Get();
			await commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}

		private Task OnLog(LogMessage e)
		{
			ConsoleUtils.WriteLine(e.ToString());
			return Task.CompletedTask;
		}
		private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext originalContext, IResult result)
		{
			if (!(originalContext is IAdvobotCommandContext context))
			{
				throw new ArgumentException(nameof(originalContext));
			}
			else if (CanBeIgnored(context, result))
			{
				return;
			}

			if (result.IsSuccess)
			{
				await context.Message.DeleteAsync(_Options).CAF();
				await ModLogAsync(context).CAF();
			}

			CommandInvoked?.Invoke(result);
			ConsoleUtils.WriteLine(FormatResult(context, result), result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);

			if (result is AdvobotResult a)
			{
				await a.SendAsync(context).CAF();
			}
			else if (!result.IsSuccess)
			{
				var message = await MessageUtils.SendMessageAsync(context.Channel, result.ErrorReason).CAF();
				var removable = new RemovableMessage(context, new[] { message }, _RemovalTime);
				_Timers.Add(removable);
			}
		}
		private static async Task ModLogAsync(IAdvobotCommandContext context)
		{
			if (context.Settings.IgnoredLogChannels.Contains(context.Channel.Id))
			{
				return;
			}

			var modLog = await context.Guild.GetTextChannelAsync(context.Settings.ModLogId).CAF();
			if (modLog == null)
			{
				return;
			}

			await MessageUtils.SendMessageAsync(modLog, embedWrapper: new EmbedWrapper
			{
				Description = context.Message.Content,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Mod Log", },
			}).CAF();
		}
		private static string FormatResult(IAdvobotCommandContext context, IResult result)
		{
			var resp = $"\n\tGuild: {context.Guild.Format()}" +
				$"\n\tChannel: {context.Channel.Format()}" +
				$"\n\tUser: {context.User.Format()}" +
				$"\n\tTime: {context.Message.CreatedAt.UtcDateTime.ToReadable()} ({context.ElapsedMilliseconds}ms)" +
				$"\n\tText: {context.Message.Content}";
			if (!result.IsSuccess && result.ErrorReason != null)
			{
				resp += $"\n\tError: {result.ErrorReason}";
			}
			return resp;
		}
		private static bool CanBeIgnored(IAdvobotCommandContext c, IResult r)
		{
			return r == null
				|| r.Error == CommandError.UnknownCommand
				|| (!r.IsSuccess && (r.ErrorReason == null || c.Settings.NonVerboseErrors))
				|| (r is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(c, r)));
		}
	}
}
