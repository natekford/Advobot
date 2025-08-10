using Advobot.Embeds;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System.Net.WebSockets;

namespace Advobot.Logging.Service;

public sealed class ClientLogger(
	ILogger logger,
	BaseSocketClient client,
	ILoggingDatabase db,
	IRuntimeConfig botSettings
)
{
	public async Task OnCommandInvoked(CommandInfo command, ICommandContext context, IResult result)
	{
		static bool CanBeIgnored(ICommandContext context, IResult result)
		{
			return result == null
				|| result.Error == CommandError.UnknownCommand
				|| (!result.IsSuccess && result.ErrorReason == null)
				|| (result is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(context, x)));
		}
		if (CanBeIgnored(context, result))
		{
			return;
		}

		logger.LogInformation(
			message: "Command executed. {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				User = context.User.Id,
				Command = command.Aliases[0],
				Content = context.Message.Content,
				Elapsed = context is IElapsed elapsed
					? elapsed.Elapsed.Milliseconds : (int?)null,
				Error = result.IsSuccess ? null : result.ErrorReason,
			}
		);

		if (result is AdvobotResult advobotResult)
		{
			await advobotResult.SendAsync(context).ConfigureAwait(false);
		}
		else if (!result.IsSuccess)
		{
			await context.Channel.SendMessageAsync(new SendMessageArgs
			{
				Content = result.ErrorReason
			}).ConfigureAwait(false);
		}

		var ignoredChannels = await db.GetIgnoredChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (ignoredChannels.Contains(context.Channel.Id))
		{
			return;
		}

		var channels = await db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		var modLog = await context.Guild.GetTextChannelAsync(channels.ModLogId).ConfigureAwait(false);
		if (modLog is null)
		{
			return;
		}

		await modLog.SendMessageAsync(new EmbedWrapper
		{
			Description = context.Message.Content,
			Author = context.User.CreateAuthor(),
			Footer = new() { Text = "Mod Log", },
		}.ToMessageArgs()).ConfigureAwait(false);
	}

	public Task OnGuildAvailable(SocketGuild guild)
	{
		var shard = client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
		logger.LogInformation(
			message: "Guild is now online {Guild} ({Shard}, {MemberCount})",
			args: [guild.Id, shard, guild.MemberCount]
		);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Guild is now offline {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnJoinedGuild(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Joined guild {Guild}",
			args: guild.Id
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
			logger.LogInformation(
				message: "Too many bots in guild {Guild} ({Percentage}%)",
				args: [guild.Id, botPercentage]
			);
			return guild.LeaveAsync();
		}
		return Task.CompletedTask;
	}

	public Task OnLeftGuild(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Left guild {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnLog(LogMessage message)
	{
		var e = message.Exception;
		// Gateway reconnects have a warning severity, but all they are is spam
		if (e is GatewayReconnectException
			|| (e?.InnerException is WebSocketException wse && wse.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely))
		{
			message = new(LogSeverity.Info, message.Source, message.Message, e);
		}

		var msg = message.Message;
		switch (message.Severity)
		{
			case LogSeverity.Critical:
				logger.LogCritical(e, msg);
				break;

			case LogSeverity.Error:
				logger.LogError(e, msg);
				break;

			case LogSeverity.Info:
				logger.LogInformation(e, msg);
				break;

			case LogSeverity.Warning:
				logger.LogWarning(e, msg);
				break;

			default:
				logger.LogDebug(e, msg);
				break;
		}

		return Task.CompletedTask;
	}

	public Task OnReady()
	{
		var launchDuration = DateTime.UtcNow - Constants.START;
		Console.WriteLine($"Bot: '{client.CurrentUser.Username}'; " +
			$"Version: {Constants.BOT_VERSION}; " +
			$"D.Net Version: {Constants.DISCORD_NET_VERSION}; " +
			$"Prefix: {botSettings.Prefix}; " +
			$"Launch Time: {launchDuration.TotalMilliseconds:n}ms");
		return Task.CompletedTask;
	}
}