using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class GuildLogger : Logger, IGuildLogger
	{
		internal GuildLogger(IServiceProvider provider) : base(provider) { }

		/// <summary>
		/// Writes to the console telling that the guild is online. If the guild's settings are not loaded, creates them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public Task OnGuildAvailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), 1);
			ConsoleUtils.WriteLine($"{guild.Format()} ({Client.GetShardIdFor(guild)}, {guild.MemberCount}, {IOUtils.GetMemory().ToString("0.00")}MB)");
			return Task.CompletedTask;
		}
		/// <summary>
		/// Writes to the console telling that the guild is offline.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public Task OnGuildUnavailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Guild is now offline {guild.Format()}.");
			return Task.CompletedTask;
		}
		/// <summary>
		/// Writes to the console telling that the guild has added the bot. Leaves if too many bots are in the server. Warns about shard issues.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task OnJoinedGuild(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), 1);
			ConsoleUtils.WriteLine($"Bot has joined {guild.Format()}.");

			//Determine what percentage of bot users to leave at
			var users = guild.MemberCount;
			double percentage;
			if (users <= 8)
			{
				percentage = .7;
			}
			else if (users <= 25)
			{
				percentage = .5;
			}
			else if (users <= 40)
			{
				percentage = .4;
			}
			else if (users <= 120)
			{
				percentage = .3;
			}
			else
			{
				percentage = .2;
			}
			//Leave if too many bots
			if ((double)guild.Users.Count(x => x.IsBot) / users > percentage)
			{
				await guild.LeaveAsync().CAF();
				return;
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild has kicked the bot. Removes the guild's settings.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public Task OnLeftGuild(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Bot has left {guild.Format()}.");
			return Task.CompletedTask;
		}
	}
}
