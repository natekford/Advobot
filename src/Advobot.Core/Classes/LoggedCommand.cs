using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds information about a command. 
	/// </summary>
	public class LoggedCommand
	{
		private static string _Joiner = "\n" + new string(' ', 28);

		public string Guild { get; private set; }
		public string Channel { get; private set; }
		public string User { get; private set; }
		public string Time { get; private set; }
		public string Text { get; private set; }
		public string ErrorReason { get; private set; }
		public ConsoleColor WriteColor { get; private set; } = ConsoleColor.Green;
		private Stopwatch _Stopwatch = new Stopwatch();

		public LoggedCommand()
		{
			_Stopwatch.Start();
		}

		/// <summary>
		/// Updates the logged command with who did what and other information.
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(ICommandContext context)
		{
			Guild = context.Guild.Format();
			Channel = context.Channel.Format();
			User = context.User.Format();
			Time = context.Message.CreatedAt.UtcDateTime.Readable();
			Text = context.Message.Content;
		}
		/// <summary>
		/// Attempts to get an error reason if the <paramref name="result"/> has <see cref="IResult.IsSuccess"/> false.
		/// </summary>
		/// <param name="result"></param>
		public void SetError(IResult result)
		{
			if (!TryGetErrorReason(result, out var errorReason))
			{
				return;
			}
			ErrorReason = errorReason;
			WriteColor = ConsoleColor.Red;
		}
		/// <summary>
		/// Sets the time completed to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		public void Complete(ICommandContext context, IResult result)
		{
			SetContext(context);
			SetError(result);
			_Stopwatch.Stop();
		}
		/// <summary>
		/// Writes this to the console in whatever color <see cref="WriteColor"/> is.
		/// </summary>
		public void Write()
		{
			ConsoleUtils.WriteLine(ToString(), nameof(LoggedCommand), WriteColor);
		}
		/// <summary>
		/// Returns true if there is a valid error reason. Returns false if the command executed without errors.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		private bool TryGetErrorReason(IResult result, out string errorReason)
		{
			errorReason = result.ErrorReason;
			return !(result.IsSuccess || Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason)
				|| result.Error == null || result.Error == CommandError.UnknownCommand);
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
			Complete(context, result);
			if (result.IsSuccess)
			{
				logging.SuccessfulCommands.Increment();
				await MessageUtils.DeleteMessageAsync(context.Message, new ModerationReason("logged command")).CAF();

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog != null && !guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					var embed = new EmbedWrapper
					{
						Description = context.Message.Content
					};
					embed.TryAddAuthor(context.User, out _);
					embed.TryAddFooter("Mod Log", null, out _);
					await MessageUtils.SendEmbedMessageAsync(guildSettings.ModLog, embed).CAF();
				}
			}
			//Failure in a valid fail way
			else if (ErrorReason != null)
			{
				logging.FailedCommands.Increment();
				await MessageUtils.SendErrorMessageAsync(context, new Error(ErrorReason)).CAF();
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
			var response = $"Guild: {Guild}" +
				$"{_Joiner}Channel: {Channel}" +
				$"{_Joiner}User: {User}" +
				$"{_Joiner}Time: {Time}" +
				$"{_Joiner}Text: {Text}" +
				$"{_Joiner}Time taken: {_Stopwatch.ElapsedMilliseconds}ms";
			return ErrorReason == null ? response : response + $"{_Joiner}Error: {ErrorReason}";
		}
	}
}