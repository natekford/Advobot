using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service;

public sealed class ClientLogger
{
	private static readonly Action<ILogger, ulong, int, int, double, Exception?> _OnGuildAvailable
		= LoggerMessage.Define<ulong, int, int, double>(
			LogLevel.Information,
			new EventId(1, nameof(OnGuildAvailable)),
			"Guild is now online: {Guild} ({Shard}, {MemberCount}, {Memory:0.00}MB)"
		);
	private static readonly Action<ILogger, ulong, Exception?> _OnGuildUnavailable
		= LoggerMessage.Define<ulong>(
			LogLevel.Information,
			new EventId(2, nameof(OnGuildUnavailable)),
			"Guild is now offline: {Guild}"
		);
	private static readonly Action<ILogger, ulong, Exception?> _OnJoinedGuild
		= LoggerMessage.Define<ulong>(
			LogLevel.Information,
			new EventId(3, nameof(OnJoinedGuild)),
			"Joined guild: {Guild}"
		);
	private static readonly Action<ILogger, ulong, Exception?> _OnLeftGuild
		= LoggerMessage.Define<ulong>(
			LogLevel.Information,
			new EventId(4, nameof(OnLeftGuild)),
			"Left guild: {Guild}"
		);
	private static readonly Action<ILogger, ulong, double, Exception?> _TooManyBots
		= LoggerMessage.Define<ulong, double>(
			LogLevel.Warning,
			new EventId(5, "TooManyBots"),
			"Too many bots in guild: {Guild} ({Percentage}%)"
		);

	private readonly BaseSocketClient _Client;
	private readonly ILogger _Logger;

	public ClientLogger(ILogger logger, BaseSocketClient client)
	{
		_Logger = logger;
		_Client = client;
	}

	public Task OnGuildAvailable(SocketGuild guild)
	{
		var shard = _Client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
		var memory = ProcessInfoUtils.GetMemoryMB();
		_OnGuildAvailable(_Logger, guild.Id, shard, guild.MemberCount, memory, null);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(SocketGuild guild)
	{
		_OnGuildUnavailable(_Logger, guild.Id, null);
		return Task.CompletedTask;
	}

	public Task OnJoinedGuild(SocketGuild guild)
	{
		_OnJoinedGuild(_Logger, guild.Id, null);

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
			_TooManyBots(_Logger, guild.Id, botPercentage, null);
			return guild.LeaveAsync();
		}
		return Task.CompletedTask;
	}

	public Task OnLeftGuild(SocketGuild guild)
	{
		_OnLeftGuild(_Logger, guild.Id, null);
		return Task.CompletedTask;
	}
}