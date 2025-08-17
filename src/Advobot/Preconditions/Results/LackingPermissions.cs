using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating the user cannot modify the target.
/// </summary>
/// <param name="user"></param>
/// <param name="target"></param>
public class LackingPermissions(IGuildUser user, ISnowflakeEntity target) : PreconditionResult(CommandError.UnmetPrecondition, $"`{user.Format()}` lacks the ability to modify `{target.Format()}`.")
{
	/// <summary>
	/// The target being attempted to modify.
	/// </summary>
	public ISnowflakeEntity Target { get; } = target;
	/// <summary>
	/// The user lacking permissions.
	/// </summary>
	public IGuildUser User { get; } = user;
}