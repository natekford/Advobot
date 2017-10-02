using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Services.Log.Loggers
{
	internal class GuildLogger : Logger, IGuildLogger
	{
		internal GuildLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		protected override void HookUpEvents()
		{
			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.GuildAvailable += OnGuildAvailable;
				socketClient.GuildUnavailable += OnGuildUnavailable;
				socketClient.JoinedGuild += OnJoinedGuild;
				socketClient.LeftGuild += OnLeftGuild;
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.GuildAvailable += OnGuildAvailable;
				shardedClient.GuildUnavailable += OnGuildUnavailable;
				shardedClient.JoinedGuild += OnJoinedGuild;
				shardedClient.LeftGuild += OnLeftGuild;
			}
			else
			{
				throw new ArgumentException($"Invalid client provided. Must be either a {nameof(DiscordSocketClient)} or a {nameof(DiscordShardedClient)}.");
			}
		}

		/// <summary>
		/// Writes to the console telling that the guild is online. If the guild's settings are not loaded, creates them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal async Task OnGuildAvailable(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"{guild.FormatGuild()} is now online on shard {ClientActions.GetShardIdFor(_Client, guild)}.");
			ConsoleActions.WriteLine($"Current memory usage is: {GetActions.GetMemory().ToString("0.00")}MB.");

			if (!_GuildSettings.ContainsGuild(guild.Id))
			{
				_Logging.TotalUsers.Add(guild.MemberCount);
				_Logging.TotalGuilds.Increment();
				await _GuildSettings.GetOrCreateSettings(guild);
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild is offline.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal Task OnGuildUnavailable(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"Guild is now offline {guild.FormatGuild()}.");
			return Task.CompletedTask;
		}
		/// <summary>
		/// Writes to the console telling that the guild has added the bot. Leaves if too many bots are in the server. Warns about shard issues.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal async Task OnJoinedGuild(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"Bot has joined {guild.FormatGuild()}.");

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
				await guild.LeaveAsync();
			}

			//Warn if at the maximum else leave
			var guilds = (await _Client.GetGuildsAsync()).Count;
			var curMax = ClientActions.GetShardCount(_Client) * 2500;
			if (guilds > curMax)
			{
				await guild.LeaveAsync();
				ConsoleActions.WriteLine($"Left the guild {guild.FormatGuild()} due to having too many guilds on the client and not enough shards.");
			}
			else if (guilds + 100 >= curMax)
			{
				ConsoleActions.WriteLine($"The bot currently has {guilds} out of {curMax} possible spots for servers filled. Increase the shard count soon.");
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild has kicked the bot. Removes the guild's settings.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal async Task OnLeftGuild(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"Bot has left {guild.FormatGuild()}.");

			_Logging.TotalUsers.Remove(guild.MemberCount);
			_Logging.TotalGuilds.Decrement();
			await _GuildSettings.RemoveGuild(guild.Id);
		}
	}
}
