using AdvorangesUtils;

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
		var memory = ProcessInfoUtils.GetMemoryMB();
		_Logger.LogInformation(
			eventId: new EventId(1, nameof(OnGuildAvailable)),
			message: "Guild is now online {Guild} ({Shard}, {MemberCount}, {Memory:0.00}MB)",
			guild.Id, shard, guild.MemberCount, memory
		);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(SocketGuild guild)
	{
		_Logger.LogInformation(
			eventId: new EventId(2, nameof(OnGuildUnavailable)),
			message: "Guild is now offline {Guild}",
			guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnJoinedGuild(SocketGuild guild)
	{
		_Logger.LogInformation(
			eventId: new EventId(3, nameof(OnJoinedGuild)),
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
				eventId: new EventId(5, "TooManyBots"),
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
			eventId: new EventId(4, nameof(OnLeftGuild)),
			message: "Left guild {Guild}",
			guild.Id
		);
		return Task.CompletedTask;
	}
}