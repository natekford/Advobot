using System;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
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
		/// How long to let this message stay up for.
		/// </summary>
		public TimeSpan? Time { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="AdvobotResult"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		protected AdvobotResult(CommandError? error, string? reason) : base(error, reason) { }

		/// <summary>
		/// Creates an error result from an exception.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static AdvobotResult Exception(Exception e)
			=> Failure(e.Message, CommandError.Exception);

		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static AdvobotResult Failure(string? reason, CommandError? error = CommandError.Unsuccessful)
			=> new(error, reason);

		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<AdvobotResult>(AdvobotResult result)
			=> Task.FromResult(result);

		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(AdvobotResult result)
			=> Task.FromResult<RuntimeResult>(result);

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static AdvobotResult Success(string reason)
		{
			if (reason.Length < 2000)
			{
				return new(null, reason);
			}

			return Success(new TextFileInfo
			{
				Name = "Message_Too_Long",
				Text = reason,
			});
		}

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="embed"></param>
		/// <returns></returns>
		public static AdvobotResult Success(EmbedWrapper embed)
			=> Success(Constants.ZERO_WIDTH_SPACE).WithEmbed(embed);

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static AdvobotResult Success(TextFileInfo file)
			=> Success(Constants.ZERO_WIDTH_SPACE).WithFile(file);

		/// <summary>
		/// Sends this result to the specified context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task<IUserMessage> SendAsync(ICommandContext context)
		{
			var destination = context.Channel;
			if (OverrideDestinationChannelId is ulong id)
			{
				destination = await context.Guild.GetTextChannelAsync(id).CAF();
				if (destination == null)
				{
					return await context.Channel.SendMessageAsync(new SendMessageArgs
					{
						Content = $"{id} is not a valid destination channel.",
					}).CAF();
				}
			}

			return await destination.SendMessageAsync(new SendMessageArgs
			{
				Content = Reason,
				Embed = Embed,
				File = File,
			}).CAF();
		}

		/// <summary>
		/// Returns the reason of this result.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> Reason;

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
		/// Sets the time in this result.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public AdvobotResult WithTime(TimeSpan? time)
		{
			Time = time;
			return this;
		}
	}
}