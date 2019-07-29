using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Advobot.CommandAssemblies;
using Advobot.Services.Timers;

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
		private readonly CommandService _Commands;
		private readonly DiscordShardedClient _Client;
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
			_Commands = _Provider.GetRequiredService<CommandService>();
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsFactory>();
			_HelpEntries = _Provider.GetRequiredService<IHelpEntryService>();
			_Timers = _Provider.GetRequiredService<ITimerService>();

			_Commands.Log += (e) =>
			{
				ConsoleUtils.WriteLine(e.ToString());
				return Task.CompletedTask;
			};
			_Commands.CommandExecuted += (_, context, result) => LogExecutionAsync((IAdvobotCommandContext)context, result);
			_Client.ShardReady += OnReady;
			_Client.MessageReceived += HandleCommand;
		}

		public async Task AddCommandsAsync(IEnumerable<CommandAssembly> commands)
		{
			foreach (var assembly in commands)
			{
				var modules = await _Commands.AddModulesAsync(assembly.Assembly, _Provider).CAF();
				foreach (var categoryModule in modules)
				{
					foreach (var commandModule in categoryModule.Submodules)
					{
						_HelpEntries.Add(new HelpEntry(commandModule));
					}
				}
				if (assembly.Attribute.Instantiator != null)
				{
					await assembly.Attribute.Instantiator.ConfigureServicesAsync(_Provider).CAF();
				}
				ConsoleUtils.WriteLine($"Successfully loaded {modules.Count()} command modules from {assembly.Assembly.GetName().Name}.");
			}
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

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(settings.Culture);
			var context = new AdvobotCommandContext(settings, _Client, (SocketUserMessage)msg);
			await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}
		private Task LogExecutionAsync(IAdvobotCommandContext context, IResult result)
		{
			//Ignore annoying unknown command errors and errors with no reason
			static bool CanBeIgnored(IAdvobotCommandContext c, IResult r)
				=> r == null || r.Error == CommandError.UnknownCommand
				|| (!r.IsSuccess && (r.ErrorReason == null || c.Settings.NonVerboseErrors));

			return result switch
			{
				IResult r when CanBeIgnored(context, result) => Task.CompletedTask,
				PreconditionGroupResult g when g.PreconditionResults.All(x => CanBeIgnored(context, x)) => Task.CompletedTask,
				IResult r => HandleResult(context, r),
				_ => throw new ArgumentException(nameof(result)),
			};
		}
		private async Task HandleResult(IAdvobotCommandContext context, IResult result)
		{
			if (result.IsSuccess)
			{
				await context.Message.DeleteAsync(_Options).CAF();
				await HandleModLog(context).CAF();
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
		private async Task HandleModLog(IAdvobotCommandContext context)
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
		private string FormatResult(IAdvobotCommandContext context, IResult result)
		{
			var resp = $"\n\tGuild: {context.Guild.Format()}" +
				$"\n\tChannel: {context.Channel.Format()}" +
				$"\n\tUser: {context.User.Format()}" +
				$"\n\tTime: {context.Message.CreatedAt.UtcDateTime.ToReadable()} ({context.ElapsedMilliseconds}ms)" +
				$"\n\tText: {context.Message.Content}";
			if (result.ErrorReason != null)
			{
				resp += $"\n\tError: {result.ErrorReason}";
			}
			return resp;
		}
	}
}
