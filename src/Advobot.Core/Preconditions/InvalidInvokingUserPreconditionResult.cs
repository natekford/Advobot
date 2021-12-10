using Discord.Commands;

namespace Advobot.Preconditions;

/// <summary>
/// Result indicating the invoking user is invalid.
/// </summary>
public class InvalidInvokingUserPreconditionResult : PreconditionResult
{
	/// <summary>
	/// A static instance of this class.
	/// </summary>
	public static InvalidInvokingUserPreconditionResult Instance { get; }
		= new InvalidInvokingUserPreconditionResult();

	/// <summary>
	/// Creates an instance of <see cref="InvalidInvokingUserPreconditionResult"/>.
	/// </summary>
	protected InvalidInvokingUserPreconditionResult()
		: base(CommandError.UnmetPrecondition, "Invalid invoking user.")
	{
	}
}