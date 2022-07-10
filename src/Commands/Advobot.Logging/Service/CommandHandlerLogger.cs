using Advobot.Embeds;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service;

public sealed class CommandHandlerLogger
{
	private readonly IBotSettings _BotSettings;
	private readonly ILoggingDatabase _Db;
	private readonly ILogger _Logger;

	public CommandHandlerLogger(
		ILogger logger,
		ILoggingDatabase db,
		IBotSettings botSettings)
	{
		_Logger = logger;
		_Db = db;
		_BotSettings = botSettings;
	}

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
			eventId: new EventId(1, nameof(OnCommandInvoked)),
			message: "Command executed. {@Info}",
			args: info
		);

		if (result is AdvobotResult advobotResult)
		{
			await advobotResult.SendAsync(context).CAF();
		}
		else if (!result.IsSuccess)
		{
			await context.Channel.SendMessageAsync(new SendMessageArgs
			{
				Content = result.ErrorReason
			}).CAF();
		}

		var ignoredChannels = await _Db.GetIgnoredChannelsAsync(context.Guild.Id).CAF();
		if (ignoredChannels.Contains(context.Channel.Id))
		{
			return;
		}

		var channels = await _Db.GetLogChannelsAsync(context.Guild.Id).CAF();
		var modLog = await context.Guild.GetTextChannelAsync(channels.ModLogId).CAF();
		if (modLog is null)
		{
			return;
		}

		await modLog.SendMessageAsync(new EmbedWrapper
		{
			Description = context.Message.Content,
			Author = context.User.CreateAuthor(),
			Footer = new() { Text = "Mod Log", },
		}.ToMessageArgs()).CAF();
	}

	public Task OnReady()
	{
		ConsoleUtils.WriteLine($"Bot version: {Constants.BOT_VERSION}; " +
			$"Discord.Net version: {Constants.DISCORD_NET_VERSION}; " +
			$"Prefix: {_BotSettings.Prefix}; " +
			$"Launch Time: {ProcessInfoUtils.GetUptime().TotalMilliseconds:n}ms");
		return Task.CompletedTask;
	}
}