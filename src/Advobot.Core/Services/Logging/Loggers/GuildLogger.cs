﻿using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class GuildLogger : Logger, IGuildLogger
	{
		private readonly BaseSocketClient _Client;

		public GuildLogger(
			IBotSettings botSettings,
			IGuildSettingsFactory guildSettings,
			BaseSocketClient client)
			: base(botSettings, guildSettings)
		{
			_Client = client;
		}

		public Task OnGuildAvailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), 1);

			var shardId = _Client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
			var memory = ProcessInfoUtils.GetMemoryMB().ToString("0.00");
			ConsoleUtils.WriteLine($"{guild.Format()} ({shardId}, {guild.MemberCount}, {memory}MB)");
			return Task.CompletedTask;
		}
		public Task OnGuildUnavailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Guild is now offline {guild.Format()}.");
			return Task.CompletedTask;
		}
		public async Task OnJoinedGuild(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), 1);
			ConsoleUtils.WriteLine($"Bot has joined {guild.Format()}.");

			//Determine what percentage of bot users to leave at
			var percentage = guild.MemberCount switch
			{
				int users when users <= 8 => .7,
				int users when users <= 25 => .5,
				int users when users <= 40 => .4,
				int users when users <= 120 => .3,
				_ => .2,
			};
			//Leave if too many bots
			if ((double)guild.Users.Count(x => x.IsBot) / guild.MemberCount > percentage)
			{
				await guild.LeaveAsync().CAF();
			}
		}
		public async Task OnLeftGuild(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Bot has left {guild.Format()}.");
			await GuildSettingsFactory.RemoveAsync(guild.Id).CAF();
		}
	}
}
