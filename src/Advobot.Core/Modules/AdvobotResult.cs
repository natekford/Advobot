using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// A result which should only be logged once.
	/// </summary>
	public class AdvobotResult : RuntimeResult
	{
		/// <summary>
		/// The result to use when this should be fully ignored.
		/// </summary>
		public static AdvobotResult IgnoreFailure { get; } = Failure(null, CommandError.Unsuccessful);
		/// <summary>
		/// The result to use when indicating a success that has no reason.
		/// </summary>
		public static AdvobotResult IgnoreSuccess { get; } = Success("");

		/// <summary>
		/// How long to let this message stay up for.
		/// </summary>
		public TimeSpan? Time { get; private set; }
		/// <summary>
		/// The embed to post with the message.
		/// </summary>
		public EmbedWrapper? Embed { get; private set; }
		/// <summary>
		/// The file to post with the message.
		/// </summary>
		public TextFileInfo? File { get; private set; }
		/// <summary>
		/// Where to send this result to. If this is null, the default context channel will be used instead.
		/// </summary>
		public ulong? OverrideDestinationChannelId { get; private set; }
		/// <summary>
		/// When the message being sent is over 2,000 characters long. This breaks it up into sendable chunks.
		/// </summary>
		public IReadOnlyCollection<string>? ReasonSegments { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="AdvobotResult"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		protected AdvobotResult(CommandError? error, string? reason) : base(error, reason) { }

		/// <summary>
		/// Sets the time in this result.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public AdvobotResult WithTime(TimeSpan? time)
		{
			Time = time;
			return this;
		}
		/// <summary>
		/// Sets the embed in this result.
		/// </summary>
		/// <param name="embed"></param>
		/// <returns></returns>
		public AdvobotResult WithEmbed(EmbedWrapper? embed)
		{
			Embed = embed;
			return this;
		}
		/// <summary>
		/// Sets the file in this result.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public AdvobotResult WithFile(TextFileInfo? file)
		{
			File = file;
			return this;
		}
		/// <summary>
		/// Sets the override destination channel in this result.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public AdvobotResult WithOverrideDestinationChannelId(ulong? id)
		{
			OverrideDestinationChannelId = id;
			return this;
		}
		/// <summary>
		/// Sends this result to the specified context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task SendAsync(ICommandContext context)
		{
			var destinationChannel = context.Channel;
			if (OverrideDestinationChannelId is ulong id)
			{
				destinationChannel = await context.Guild.GetTextChannelAsync(id).CAF();
				if (destinationChannel == null)
				{
					await MessageUtils.SendMessageAsync(context.Channel, $"{id} is not a valid channel.").CAF();
					return;
				}
			}

			if (ReasonSegments != null)
			{
				foreach (var segment in ReasonSegments)
				{
					await MessageUtils.SendMessageAsync(destinationChannel, segment).CAF();
				}
			}
			else
			{
				await MessageUtils.SendMessageAsync(destinationChannel, Reason, Embed, File).CAF();
			}
		}
		/// <summary>
		/// Returns the reason of this result.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> Reason;

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult Success(string reason)
			=> new AdvobotResult(null, reason);
		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="embed"></param>
		/// <returns></returns>
		public static AdvobotResult Success(EmbedWrapper embed)
			=> Success(Constants.ZERO_LENGTH_CHAR).WithEmbed(embed);
		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static AdvobotResult Success(TextFileInfo file)
			=> Success(Constants.ZERO_LENGTH_CHAR).WithFile(file);
		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="reasonSegments"></param>
		/// <param name="joiner"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static AdvobotResult FromReasonSegments(IReadOnlyCollection<string> reasonSegments, string joiner = "\n", CommandError? error = null)
		{
			return new AdvobotResult(error, reasonSegments.Join(joiner))
			{
				ReasonSegments = reasonSegments
			};
		}
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static AdvobotResult Failure(string? reason, CommandError? error = CommandError.Unsuccessful)
			=> new AdvobotResult(error, reason);
		/// <summary>
		/// Creates an error result from an exception.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static AdvobotResult Exception(Exception e)
			=> Failure(e.Message, CommandError.Exception);
		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(AdvobotResult result)
			=> Task.FromResult<RuntimeResult>(result);
		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<AdvobotResult>(AdvobotResult result)
			=> Task.FromResult(result);
	}
}