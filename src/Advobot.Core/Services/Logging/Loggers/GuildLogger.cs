using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Services.Logging.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Handles logging guild events.
	/// </summary>
	internal sealed class GuildLogger : Logger, IGuildLogger
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildLogger"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildLogger(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public async Task OnGuildAvailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), 1);
			ConsoleUtils.WriteLine($"{guild.Format()} ({Client.GetShardIdFor(guild)}, {guild.MemberCount}, {ProcessInfoUtils.GetMemory().ToString("0.00")}MB)");
			await GuildSettings.GetOrCreateAsync(guild).CAF();
		}
		/// <inheritdoc />
		public Task OnGuildUnavailable(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Guild is now offline {guild.Format()}.");
			return Task.CompletedTask;
		}
		/// <inheritdoc />
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
			}
		}
		/// <inheritdoc />
		public async Task OnLeftGuild(SocketGuild guild)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -guild.MemberCount);
			NotifyLogCounterIncrement(nameof(ILogService.TotalGuilds), -1);
			ConsoleUtils.WriteLine($"Bot has left {guild.Format()}.");
			await GuildSettings.RemoveAsync(guild.Id).CAF();
		}
	}
}
