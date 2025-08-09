using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service;

public sealed class ClientLogger(ILogger logger, BaseSocketClient client)
{
	private readonly BaseSocketClient _Client = client;
	private readonly ILogger _Logger = logger;

	public Task OnGuildAvailable(SocketGuild guild)
	{
		var shard = _Client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
		_Logger.LogInformation(
			message: "Guild is now online {Guild} ({Shard}, {MemberCount})",
			guild.Id, shard, guild.MemberCount
		);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(SocketGuild guild)
	{
		_Logger.LogInformation(
			message: "Guild is now offline {Guild}",
			guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnJoinedGuild(SocketGuild guild)
	{
		_Logger.LogInformation(
			message: "Joined guild {Guild}",
			guild.Id
		);

		//Determine what percentage of bot users to leave at and leave if too many bots
		var allowedPercentage = guild.MemberCount switch
		{
			int users when users < 9 => .7,
			int users when users < 26 => .5,
			int users when users < 41 => .4,
			int users when users < 121 => .3,
			_ => .2,
		};
		var botPercentage = (double)guild.Users.Count(x => x.IsBot) / guild.MemberCount;
		if (botPercentage > allowedPercentage)
		{
			_Logger.LogInformation(
				message: "Too many bots in guild {Guild} ({Percentage}%)",
				guild.Id, botPercentage
			);
			return guild.LeaveAsync();
		}
		return Task.CompletedTask;
	}

	public Task OnLeftGuild(SocketGuild guild)
	{
		_Logger.LogInformation(
			message: "Left guild {Guild}",
			guild.Id
		);
		return Task.CompletedTask;
	}
}