using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Interfaces;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds information about a command. 
	/// </summary>
	public class LoggedCommand
	{
		private static readonly string _Joiner = "\n" + new string(' ', 28);

		public string Guild { get; private set; }
		public string Channel { get; private set; }
		public string User { get; private set; }
		public string Time { get; private set; }
		public string Text { get; private set; }
		public string ErrorReason { get; private set; }
		public ConsoleColor WriteColor { get; private set; } = ConsoleColor.Green;
		private Stopwatch Stopwatch;

		public LoggedCommand()
		{
			Stopwatch = new Stopwatch();
			Stopwatch.Start();
		}

		/// <summary>
		/// Updates the logged command with who did what and other information.
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(ICommandContext context)
		{
			Guild = context.Guild.FormatGuild();
			Channel = context.Channel.FormatChannel();
			User = context.User.FormatUser();
			Time = TimeFormatting.FormatReadableDateTime(context.Message.CreatedAt.UtcDateTime);
			Text = context.Message.Content;
		}
		/// <summary>
		/// Attempts to get an error reason if the <paramref name="result"/> has <see cref="IResult.IsSuccess"/> false.
		/// </summary>
		/// <param name="result"></param>
		public void SetError(IResult result)
		{
			if (TryGetErrorReason(result, out string errorReason))
			{
				ErrorReason = errorReason;
				WriteColor = ConsoleColor.Red;
			}
		}
		/// <summary>
		/// Sets <see cref="TimeCompleted"/> to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		public void Finalize(ICommandContext context, IResult result)
		{
			SetContext(context);
			SetError(result);
			Stopwatch.Stop();
		}
		/// <summary>
		/// Writes this to the console in whatever color <see cref="WriteColor"/> is.
		/// </summary>
		public void Write() => ConsoleUtils.WriteLine(ToString(), nameof(LoggedCommand), WriteColor);
		/// <summary>
		/// Returns true if there is a valid error reason. Returns false if the command executed without errors.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		private bool TryGetErrorReason(IResult result, out string errorReason)
		{
			errorReason = result.ErrorReason;
			if (result.IsSuccess || Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				return false;
			}

			switch (result.Error)
			{
				case null:
				//Ignore commands with the unknown command error because it's annoying
				case CommandError.UnknownCommand:
				{
					return false;
				}
				default:
				{
					return true;
				}
			}
		}
		/// <summary>
		/// Writes the results of the command to the console and log channel.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <param name="logging"></param>
		/// <returns></returns>
		public async Task LogCommand(IAdvobotCommandContext context, IResult result, ILogService logging)
		{
			Finalize(context, result);
			if (result.IsSuccess)
			{
				logging.SuccessfulCommands.Increment();
				await MessageUtils.DeleteMessageAsync(context.Message, new AutomaticModerationReason("logged command")).CAF();

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog != null && !guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new EmbedWrapper(null, context.Message.Content)
						.AddAuthor(context.User)
						.AddFooter("Mod Log");
					await MessageUtils.SendEmbedMessageAsync(guildSettings.ModLog, embed).CAF();
				}
			}
			//Failure in a valid fail way
			else if (ErrorReason != null)
			{
				logging.FailedCommands.Increment();
				await MessageUtils.SendErrorMessageAsync(context, new ErrorReason(ErrorReason)).CAF();
			}
			//Failure in a way that doesn't need to get logged (unknown command, etc)
			else
			{
				return;
			}

			Write();
			logging.RanCommands.Add(this);
			logging.AttemptedCommands.Increment();
		}

		public override string ToString()
		{
			var response = new StringBuilder()
				.Append($"Guild: {Guild}")
				.Append($"{_Joiner}Channel: {Channel}")
				.Append($"{_Joiner}User: {User}")
				.Append($"{_Joiner}Time: {Time}")
				.Append($"{_Joiner}Text: {Text}")
				.Append($"{_Joiner}Time taken: {Stopwatch.ElapsedMilliseconds}ms");
			if (ErrorReason != null)
			{
				response.Append($"{_Joiner}Error: {ErrorReason}");
			}
			return response.ToString();
		}
	}
}