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

namespace Advobot.Services.Commands
{
	/// <summary>
	/// Handles user input commands.
	/// </summary>
	internal sealed class CommandHandlerService : ICommandHandlerService
	{
		private static readonly RequestOptions _Options = DiscordUtils.GenerateRequestOptions("Command successfully executed.");

		private readonly IServiceProvider _Provider;
		private readonly CommandService _Commands;
		private readonly DiscordShardedClient _Client;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IHelpEntryService _HelpEntries;
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

			_Commands.Log += (e) =>
			{
				ConsoleUtils.WriteLine(e.ToString());
				return Task.CompletedTask;
			};
			_Commands.CommandExecuted += (_, context, result) => LogExecutionAsync((AdvobotCommandContext)context, result);
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
		private async Task HandleCommand(SocketMessage message)
		{
			var argPos = -1;
			if (!_Loaded || _BotSettings.Pause || message.Author.IsBot || string.IsNullOrWhiteSpace(message.Content)
				//Disallow running commands if the user is blocked, unless the owner of the bot blocks themselves either accidentally or idiotically
				|| (_BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id) && message.Author.Id != _OwnerId)
				|| !(message is SocketUserMessage msg)
				|| !(msg.Author is SocketGuildUser user)
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredCommandChannels.Contains(msg.Channel.Id)
				|| !(msg.HasStringPrefix(settings.GetPrefix(_BotSettings), ref argPos)
					|| msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(settings.Culture);
			var context = new AdvobotCommandContext(settings, _Client, msg);
			await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}
		private Task LogExecutionAsync(AdvobotCommandContext context, IResult result)
		{
			//Ignore annoying unknown command errors and errors with no reason
			static bool CanBeIgnored(AdvobotCommandContext c, IResult r)
				=> r == null || r.Error == CommandError.UnknownCommand
				|| (!r.IsSuccess && (r.ErrorReason == null || c.GuildSettings.NonVerboseErrors));

			return result switch
			{
				IResult r when CanBeIgnored(context, result) => Task.CompletedTask,
				PreconditionGroupResult g when g.PreconditionResults.All(x => CanBeIgnored(context, x)) => Task.CompletedTask,
				IResult r => HandleResult(context, r),
				_ => throw new ArgumentException(nameof(result)),
			};
		}
		private async Task HandleResult(AdvobotCommandContext c, IResult result)
		{
			if (result.IsSuccess)
			{
				await c.Message.DeleteAsync(_Options).CAF();
				if (c.GuildSettings.ModLogId != 0 && !c.GuildSettings.IgnoredLogChannels.Contains(c.Channel.Id))
				{
					await MessageUtils.SendMessageAsync(c.Guild.GetTextChannel(c.GuildSettings.ModLogId), embedWrapper: new EmbedWrapper
					{
						Description = c.Message.Content,
						Author = c.User.CreateAuthor(),
						Footer = new EmbedFooterBuilder { Text = "Mod Log", },
					}).CAF();
				}
			}

			CommandInvoked?.Invoke(result);
			ConsoleUtils.WriteLine(c.FormatResult(result), result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			await (result switch
			{
				AdvobotResult a => a.SendAsync(c),
				IResult i when i.IsSuccess => Task.CompletedTask,
				//TODO: delete after time
				IResult error => MessageUtils.SendMessageAsync(c.Channel, error.ErrorReason),
				_ => throw new ArgumentException(nameof(result)),
			}).CAF();
		}
	}
}
