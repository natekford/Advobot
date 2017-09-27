using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// Result telling whether or not the object passed the verification it was tried for.
	/// </summary>
	public struct VerifiedObjectResult : IResult
	{
		public object Value { get; private set; }
		public CommandError? Error { get; private set; }
		public string ErrorReason { get; private set; }
		public bool IsSuccess { get; private set; }

		public VerifiedObjectResult(object value, CommandError? error, string errorReason)
		{
			Value = value;
			Error = error;
			ErrorReason = errorReason;
			IsSuccess = Error == null;
		}
	}
}
