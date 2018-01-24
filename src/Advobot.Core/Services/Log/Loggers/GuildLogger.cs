using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord.WebSocket;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class GuildLogger : Logger, IGuildLogger
	{
		internal GuildLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Writes to the console telling that the guild is online. If the guild's settings are not loaded, creates them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task OnGuildAvailable(SocketGuild guild)
		{
			ConsoleUtils.WriteLine($"{guild.Format()} is now online on shard {ClientUtils.GetShardIdFor(Client, guild)}.");
			ConsoleUtils.WriteLine($"Current memory usage is: {IOUtils.GetMemory().ToString("0.00")}MB.");

			if (!GuildSettings.Contains(guild.Id))
			{
				Logging.TotalUsers.Add(guild.MemberCount);
				Logging.TotalGuilds.Increment();
				await GuildSettings.GetOrCreateAsync(guild).CAF();
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild is offline.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public Task OnGuildUnavailable(SocketGuild guild)
		{
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

			//Warn if at the maximum else leave
			var guilds = (await Client.GetGuildsAsync().CAF()).Count;
			var curMax = ClientUtils.GetShardCount(Client) * 2500;
			if (guilds > curMax)
			{
				await guild.LeaveAsync().CAF();
				ConsoleUtils.WriteLine($"Left the guild {guild.Format()} due to having too many guilds on the client and not enough shards.");
			}
			else if (guilds + 100 >= curMax)
			{
				ConsoleUtils.WriteLine($"The bot currently has {guilds} out of {curMax} possible spots for servers filled. Increase the shard count soon.");
			}

			if (!GuildSettings.Contains(guild.Id))
			{
				Logging.TotalUsers.Add(guild.MemberCount);
				Logging.TotalGuilds.Increment();
				await GuildSettings.GetOrCreateAsync(guild).CAF();
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild has kicked the bot. Removes the guild's settings.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task OnLeftGuild(SocketGuild guild)
		{
			ConsoleUtils.WriteLine($"Bot has left {guild.Format()}.");

			Logging.TotalUsers.Remove(guild.MemberCount);
			Logging.TotalGuilds.Decrement();
			await GuildSettings.RemoveAsync(guild.Id).CAF();
		}
	}
}
