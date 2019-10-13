using System;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Logging.Context;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Logging.Service
{
	public sealed class CommandHandlerLogger
	{
		private readonly IBotSettings _BotSettings;
		private readonly ILoggingService _Logging;

		private readonly InformationMatrixFormattingArgs _ResultFormattingArgs = new InformationMatrixFormattingArgs
		{
			InformationSeparator = "\n\t",
			TitleFormatter = x => x.FormatTitle() + ":",
		};

		public CommandHandlerLogger(ILoggingService logging, IBotSettings botSettings)
		{
			_Logging = logging;
			_BotSettings = botSettings;
		}

		public async Task OnCommandInvoked(CommandInfo command, ICommandContext context, IResult result)
		{
			var color = result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
			ConsoleUtils.WriteLine(FormatResult(command, context, result), color);

			if (result is AdvobotResult a)
			{
				await a.SendAsync(context).CAF();
			}
			else if (!result.IsSuccess)
			{
				await MessageUtils.SendMessageAsync(context.Channel, result.ErrorReason).CAF();
			}

			var loggingContext = await _Logging.CreateAsync(context.Message).CAF();
			if (loggingContext.ModLog == null || !loggingContext.ChannelCanBeLogged())
			{
				return;
			}

			await MessageUtils.SendMessageAsync(loggingContext.ModLog, embed: new EmbedWrapper
			{
				Description = context.Message.Content,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Mod Log", },
			}).CAF();
		}

		public Task OnReady()
		{
			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; " +
				$"Prefix: {_BotSettings.Prefix}; " +
				$"Launch Time: {ProcessInfoUtils.GetUptime().TotalMilliseconds:n}ms");
			return Task.CompletedTask;
		}

		private string FormatResult(CommandInfo command, ICommandContext context, IResult result)
		{
			var time = context.Message.CreatedAt.UtcDateTime.ToReadable();
			if (context is IElapsed elapsed)
			{
				time += $" ({elapsed.Elapsed.Milliseconds}ms)";
			}

			var info = new InformationMatrix();
			var collection = info.CreateCollection();
			collection.Add("Command", command.Aliases[0]);
			collection.Add("Guild", context.Guild.Format());
			collection.Add("Channel", context.Channel.Format());
			collection.Add("Time", time);
			collection.Add("Text", context.Message.Content);
			if (!result.IsSuccess && result.ErrorReason != null)
			{
				collection.Add("Error", result.ErrorReason);
			}
			return info.ToString(_ResultFormattingArgs);
		}
	}
}