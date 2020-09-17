using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Preconditions
{
	/// <summary>
	/// Result indicating the user cannot modify the target.
	/// </summary>
	public class LackingPermissionsPreconditionResult : PreconditionResult
	{
		/// <summary>
		/// The target being attempted to modify.
		/// </summary>
		public ISnowflakeEntity Target { get; }
		/// <summary>
		/// The user lacking permissions.
		/// </summary>
		public IGuildUser User { get; }

		/// <summary>
		/// Creates an instance of <see cref="InvalidInvokingUserPreconditionResult"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		public LackingPermissionsPreconditionResult(IGuildUser user, ISnowflakeEntity target)
			: base(CommandError.UnmetPrecondition, $"`{user.Format()}` lacks the ability to modify `{target.Format()}`.")
		{
			User = user;
			Target = target;
		}
	}
}