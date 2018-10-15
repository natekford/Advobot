using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
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
		private ulong _OwnerId;

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
			foreach (var type in AdvobotUtils.RegisterStaticSettingParsers())
			{
				var tr = (TypeReader)Activator.CreateInstance(typeof(ParsableTypeReader<>).MakeGenericType(type));
				_Commands.AddTypeReader(type, tr);
			}

			_Client.ShardReady += (client) => OnReady(client, commands);
			_Client.MessageReceived += HandleCommand;
		}

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
			_OwnerId = await ClientUtils.GetOwnerIdAsync(_Client).CAF();

			await ClientUtils.UpdateGameAsync(client, _BotSettings).CAF();
			//Add in all the commands
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
		/// <inheritdoc />
		public async Task HandleCommand(SocketMessage message)
		{
			var argPos = -1;
			if (!_Loaded
				|| _BotSettings.Pause
				//Disallow running commands if the user is blocked, unless the owner of the bot blocks themselves either accidentally or idiotically
				|| (_BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id) && message.Author.Id != _OwnerId)
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

			var context = new AdvobotCommandContext(settings, _Client, msg);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider).CAF();

			if (CanBeIgnored(result) || (result is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(x))))
			{
				return;
			}
			if (result.IsSuccess)
			{
				//_Logging?.SuccessfulCommands?.Add(1);
				await context.Message.DeleteAsync(ClientUtils.CreateRequestOptions("logged command")).CAF();
				if (context.GuildSettings.ModLogId != 0 && !context.GuildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					await MessageUtils.SendMessageAsync(context.Guild.GetTextChannel(context.GuildSettings.ModLogId), embedWrapper: new EmbedWrapper
					{
						Description = context.Message.Content,
						Author = context.User.CreateAuthor(),
						Footer = new EmbedFooterBuilder { Text = "Mod Log", },
					}).CAF();
				}
			}
			else
			{
#warning convert back to error
				await MessageUtils.SendMessageAsync(context.Channel, result.ErrorReason).CAF();
			}

			CommandInvoked?.Invoke(result);
			ConsoleUtils.WriteLine(context.ToString(result), result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
		}
		private bool CanBeIgnored(IResult result)
			=> result.Error == CommandError.UnknownCommand || (!result.IsSuccess && result.ErrorReason == null);
	}
}
