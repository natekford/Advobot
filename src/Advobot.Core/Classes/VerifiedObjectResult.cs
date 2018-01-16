using Discord.Commands;

namespace Advobot.Core.Classes.Results
{
	/// <summary>
	/// Result telling whether or not the object passed the verification it was tried for.
	/// </summary>
	public struct VerifiedObjectResult : IResult
	{
		public object Value { get; }
		public CommandError? Error { get; }
		public string ErrorReason { get; }
		public bool IsSuccess { get; }

		public VerifiedObjectResult(object value, CommandError? error, string errorReason)
		{
			Value = value;
			Error = error;
			ErrorReason = errorReason;
			IsSuccess = Error == null;
		}
	}
}
