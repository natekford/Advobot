using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating the invoking user is invalid.
/// </summary>
public class InvalidInvokingUser : PreconditionResult
{
	/// <summary>
	/// A static instance of this class.
	/// </summary>
	public static InvalidInvokingUser Instance { get; }
		= new InvalidInvokingUser();

	/// <summary>
	/// Creates an instance of <see cref="InvalidInvokingUser"/>.
	/// </summary>
	protected InvalidInvokingUser()
		: base(CommandError.UnmetPrecondition, "Invalid invoking user.")
	{
	}
}