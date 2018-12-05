using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// A result which should only be logged once.
	/// </summary>
	public class AdvobotResult : RuntimeResult
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public TimeSpan? Time { get; }

		/// <summary>
		/// Creates an instance of <see cref="AdvobotResult"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		protected AdvobotResult(TimeSpan? time, CommandError? error, string reason) : base(error, reason)
		{
			Time = time;
		}

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult FromSuccess(string reason)
			=> new AdvobotResult(null, null, reason);
		/// <summary>
		/// Creates a successful result which gets removed after a specific amount of milliseconds.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult FromTime(string reason, TimeSpan time)
			=> new AdvobotResult(time, null, reason);
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult FromFailure(string reason)
			=> new AdvobotResult(null, CommandError.Unsuccessful, reason);
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult FromFailure(CommandError error, string reason)
			=> new AdvobotResult(null, error, reason);
		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(AdvobotResult result)
			=> Task.FromResult<RuntimeResult>(result);
	}

	public abstract class PrintableResult : RuntimeResult
	{
		public string Content { get; set; } = Constants.ZERO_LENGTH_CHAR;
		public EmbedWrapper? Embed { get; set; }
		public TextFileInfo? File { get; set; }

		protected PrintableResult() : base(null, null) { }
		protected PrintableResult(CommandError? error, string reason) : base(error, reason) { }

		public Task<IUserMessage?> RespondAsync(AdvobotCommandContext context)
			=> !IsSuccess && context.GuildSettings.NonVerboseErrors ? Task.FromResult(default(IUserMessage)) : DoRespondAsync(context);
		protected virtual Task<IUserMessage?> DoRespondAsync(AdvobotCommandContext context)
			=> MessageUtils.SendMessageAsync(context.Channel, Content, Embed, File);
	}

	public sealed class TimedResult : PrintableResult
	{
		public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(5);

		private Func<AdvobotCommandContext, IUserMessage, TimeSpan, Task> Func { get; }

		/// <summary>
		/// Creates an instance of <see cref="TimedResult"/>.
		/// </summary>
		/// <param name="timers"></param>
		public TimedResult(ITimerService timers) : this((context, message, time) =>
		{
			var removableMessage = new RemovableMessage(context, new[] { message }, time);
			return timers.AddAsync(removableMessage);
		}) { }
		/// <summary>
		/// Creates an instance of <see cref="TimedResult"/> and uses an unawaited <see cref="Task.Run(Func{Task})"/> to delete the messages.
		/// </summary>
		public TimedResult() : this((context, message, time) =>
		{
			_ = Task.Run(async () =>
			{
				await Task.Delay(time).CAF();
				await MessageUtils.DeleteMessagesAsync(context.Channel, new[] { context.Message, message, }, context.GenerateRequestOptions()).CAF();
			});
			return Task.CompletedTask;
		}) { }
		/// <summary>
		/// Creates an instance of <see cref="TimedResult"/>.
		/// </summary>
		/// <param name="func"></param>
		public TimedResult(Func<AdvobotCommandContext, IUserMessage, TimeSpan, Task> func)
		{
			Func = func;
		}

		/// <inheritdoc />
		protected override async Task<IUserMessage?> DoRespondAsync(AdvobotCommandContext context)
		{
			var message = await base.DoRespondAsync(context).CAF();
			await Func(context, message, Time).CAF();
			return message;
		}
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}