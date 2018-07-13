using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// Result telling whether or not the object passed the verification it was tried for.
	/// </summary>
	public struct VerifiedObjectResult : IResult
	{
		/// <summary>
		/// The parsed value.
		/// </summary>
		public object Value { get; }
		/// <summary>
		/// Any errors which occurred.
		/// </summary>
		public CommandError? Error { get; }
		/// <summary>
		/// The reason for the error.
		/// </summary>
		public string ErrorReason { get; }
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
		public VerifiedObjectResult(object value, CommandError? error, string errorReason)
		{
			Value = value;
			Error = error;
			ErrorReason = errorReason;
			IsSuccess = Error == null;
		}
	}
}
