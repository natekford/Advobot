using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
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
		private static readonly RequestOptions _Options = DiscordUtils.GenerateRequestOptions("Command successfully executed.");

		private readonly IServiceProvider _Provider;
		private readonly CommandService _Commands;
		private readonly DiscordShardedClient _Client;
		private readonly IBotSettings _BotSettings;
		private readonly IGuildSettingsFactory _GuildSettings;
		private bool _Loaded;
		private ulong _OwnerId;

		/// <inheritdoc />
		public event Action<IResult> CommandInvoked;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandlerService"/> and gets the required services.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="commands"></param>
		public CommandHandlerService(IServiceProvider provider, IEnumerable<Assembly> commands)
		{
			_Provider = provider;
			_Commands = provider.GetRequiredService<CommandService>();
			_Client = _Provider.GetRequiredService<DiscordShardedClient>();
			_BotSettings = _Provider.GetRequiredService<IBotSettings>();
			_GuildSettings = _Provider.GetRequiredService<IGuildSettingsFactory>();

			var typeReaders = Assembly.GetExecutingAssembly().GetTypes()
				.Select(x => (Attribute: x.GetCustomAttribute<TypeReaderTargetTypeAttribute>(), Type: x))
				.Where(x => x.Attribute != null);
			foreach (var typeReader in typeReaders)
			{
				var instance = (TypeReader)Activator.CreateInstance(typeReader.Type);
				_Commands.AddTypeReader(typeReader.Attribute.TargetType, instance);
			}

			_Commands.Log += LogInfo;
			_Commands.CommandExecuted += (_, context, result) => LogExecution((AdvobotCommandContext)context, result);

			_Client.ShardReady += (client) => OnReady(client, commands);
			_Client.MessageReceived += HandleCommand;
		}

		private async Task OnReady(DiscordSocketClient client, IEnumerable<Assembly> commands)
		{
			if (_Loaded)
			{
				return;
			}
			_Loaded = true;

			_OwnerId = await _Client.GetOwnerIdAsync().CAF();
			await client.UpdateGameAsync(_BotSettings).CAF();
			foreach (var assembly in commands)
			{
				await _Commands.AddModulesAsync(assembly, _Provider).CAF();
			}
			_Provider.GetRequiredService<IHelpEntryService>().Add(_Commands.Modules);

			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; " +
				$"Modules: {_Commands.Modules.Count()}; " +
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
				|| !(msg.HasStringPrefix(_BotSettings.GetPrefix(settings), ref argPos) || msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			ConsoleUtils.DebugWrite($"Culture in command handler: {CultureInfo.CurrentCulture.Name}");
#warning set culture
			var context = new AdvobotCommandContext(settings, _Client, msg);
			await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}
		private Task LogExecution(AdvobotCommandContext context, IResult result) => result switch
		{
			IResult r when CanBeIgnored(context, result) => Task.CompletedTask,
			PreconditionGroupResult g when g.PreconditionResults.All(x => CanBeIgnored(context, x)) => Task.CompletedTask,
			IResult r => HandleResult(context, r),
			_ => throw new ArgumentException(nameof(result)),
		};
		private async Task HandleResult(AdvobotCommandContext c, IResult result)
		{
			ConsoleUtils.DebugWrite($"Culture in result handler: {CultureInfo.CurrentCulture.Name}");
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
				AdvobotResult a => MessageUtils.SendMessageAsync(c.Channel, a.Reason, a.Embed, a.File),
#warning delete after time
				IResult i => MessageUtils.SendMessageAsync(c.Channel, i.ErrorReason),
				_ => throw new ArgumentException(nameof(result)),
			}).CAF();
		}
		private Task LogInfo(LogMessage arg)
		{
			ConsoleUtils.WriteLine(arg.ToString());
			return Task.CompletedTask;
		}
		private bool CanBeIgnored(AdvobotCommandContext context, IResult r) //Ignore annoying unknown command errors and errors with no reason
			=> r == null || r.Error == CommandError.UnknownCommand || (!r.IsSuccess && (r.ErrorReason == null || context.GuildSettings.NonVerboseErrors));
	}
}
