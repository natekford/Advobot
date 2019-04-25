using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// Result telling whether or not the object passed the verification it was tried for.
	/// </summary>
	public readonly struct VerifiedObjectResult : IResult
	{
		/// <summary>
		/// The parsed value.
		/// </summary>
		public object? Value { get; }
		/// <summary>
		/// Any errors which occurred.
		/// </summary>
		public CommandError? Error { get; }
		/// <summary>
		/// The reason for the error.
		/// </summary>
		public string? ErrorReason { get; }
		/// <summary>
		/// Whether or not was successful.
		/// </summary>
		public bool IsSuccess { get; }

		/// <summary>
		/// Creates an instace of verified object result.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="error"></param>
		/// <param name="errorReason"></param>
		private VerifiedObjectResult(object? value, CommandError? error, string? errorReason)
		{
			Value = value;
			Error = error;
			ErrorReason = errorReason;
			IsSuccess = Error == null;
		}

		/// <summary>
		/// Returns a result indicating success.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static VerifiedObjectResult FromSuccess(object value)
			=> new VerifiedObjectResult(value, null, null);
		/// <summary>
		/// Returns a result indicating an error.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		public static VerifiedObjectResult FromError(CommandError error, string errorReason)
			=> new VerifiedObjectResult(null, error, errorReason);
		/// <summary>
		/// Returns a result indicating an error where the user has a lower position than the supplied object.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static VerifiedObjectResult FromUnableToModify(SocketGuildUser invoker, ISnowflakeEntity target)
		{
			var start = invoker.Id == invoker.Guild.CurrentUser.Id ? "I am" : "You are";
			var reason = $"{start} unable to make the given changes to `{target.Format()}`.";
			return FromError(CommandError.UnmetPrecondition, reason);
		}
	}
}
