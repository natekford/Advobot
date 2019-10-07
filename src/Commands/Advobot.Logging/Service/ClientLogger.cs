using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Context;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class ClientLogger
	{
		private readonly BaseSocketClient _Client;
		private readonly ILoggingService _Logging;

		public ClientLogger(ILoggingService logging, BaseSocketClient client)
		{
			_Logging = logging;
			_Client = client;
		}

		public async Task OnCommandInvoked(ICommandContext context, IResult _)
		{
			var loggingContext = await _Logging.CreateAsync(context.Message).CAF();
			if (loggingContext.ModLog == null || !loggingContext.ChannelCanBeLogged())
			{
				return;
			}

			await MessageUtils.SendMessageAsync(loggingContext.ModLog, embed: new EmbedWrapper
			{
				Description = context.Message.Content,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Mod Log", },
			}).CAF();
		}

		public Task OnGuildAvailable(SocketGuild guild)
		{
			var shardId = _Client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
			var memory = ProcessInfoUtils.GetMemoryMB().ToString("0.00");
			ConsoleUtils.WriteLine($"{guild.Format()} ({shardId}, {guild.MemberCount}, {memory}MB)");
			return Task.CompletedTask;
		}

		public Task OnGuildUnavailable(SocketGuild guild)
		{
			ConsoleUtils.WriteLine($"Guild is now offline {guild.Format()}.");
			return Task.CompletedTask;
		}

		public Task OnJoinedGuild(SocketGuild guild)
		{
			ConsoleUtils.WriteLine($"Bot has joined {guild.Format()}.");

			//Determine what percentage of bot users to leave at and leave if too many bots
			var percentage = guild.MemberCount switch
			{
				int users when users < 9 => .7,
				int users when users < 26 => .5,
				int users when users < 41 => .4,
				int users when users < 121 => .3,
				_ => .2,
			};
			if ((double)guild.Users.Count(x => x.IsBot) / guild.MemberCount > percentage)
			{
				return guild.LeaveAsync();
			}
			return Task.CompletedTask;
		}

		public Task OnLeftGuild(SocketGuild guild)
		{
			ConsoleUtils.WriteLine($"Bot has left {guild.Format()}.");
			return Task.CompletedTask;
		}

		public Task OnLogMessageSent(LogMessage message)
		{
			message.Write();
			return Task.CompletedTask;
		}
	}
}