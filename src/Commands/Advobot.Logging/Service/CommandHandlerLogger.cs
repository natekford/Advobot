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

namespace Advobot.Logging.Service;

public sealed class CommandHandlerLogger(
	ILogger logger,
	BaseSocketClient client,
	ILoggingDatabase db,
	IRuntimeConfig botSettings)
{
	private readonly IRuntimeConfig _BotSettings = botSettings;
	private readonly ILoggingDatabase _Db = db;
	private readonly ILogger _Logger = logger;

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

		var info = new
		{
			Guild = context.Guild.Id,
			Channel = context.Channel.Id,
			User = context.User.Id,
			Command = command.Aliases[0],
			Content = context.Message.Content,
			Elapsed = context is IElapsed elapsed
				? elapsed.Elapsed.Milliseconds : (int?)null,
			Error = result.IsSuccess ? null : result.ErrorReason,
		};
		_Logger.LogInformation(
			message: "Command executed. {@Info}",
			args: info
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

		var ignoredChannels = await _Db.GetIgnoredChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (ignoredChannels.Contains(context.Channel.Id))
		{
			return;
		}

		var channels = await _Db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
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

	public Task OnReady()
	{
		var launchDuration = DateTime.UtcNow - AdvobotUtils.StartTime;
		Console.WriteLine($"Bot: '{client.CurrentUser.Username}'; " +
			$"Version: {Constants.BOT_VERSION}; " +
			$"D.Net Version: {Constants.DISCORD_NET_VERSION}; " +
			$"Prefix: {_BotSettings.Prefix}; " +
			$"Launch Time: {launchDuration.TotalMilliseconds:n}ms");
		return Task.CompletedTask;
	}
}