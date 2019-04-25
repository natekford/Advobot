using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// A result which should only be logged once.
	/// </summary>
	public class AdvobotResult : RuntimeResult
	{
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
		public static AdvobotResult Success(string reason = Constants.ZERO_LENGTH_CHAR)
			=> new AdvobotResult(null, reason);
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static AdvobotResult Failure(string? reason, CommandError? error = CommandError.Unsuccessful)
			=> new AdvobotResult(error, reason);
		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(AdvobotResult result)
			=> Task.FromResult<RuntimeResult>(result);
	}
}