using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
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
		private readonly ILoggingDatabase _Db;

		private readonly InformationMatrixFormattingArgs _ResultFormattingArgs = new InformationMatrixFormattingArgs
		{
			InformationSeparator = "\n\t",
			TitleFormatter = x => x.FormatTitle() + ":",
		};

		public CommandHandlerLogger(ILoggingDatabase db, IBotSettings botSettings)
		{
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

			var color = result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
			ConsoleUtils.WriteLine(FormatResult(command, context, result), color);

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
				Footer = new EmbedFooterBuilder { Text = "Mod Log", },
			}.ToMessageArgs()).CAF();
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