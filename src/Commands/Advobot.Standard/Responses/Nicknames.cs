using Advobot.Modules;
using Advobot.Utilities;
using Discord;

namespace Advobot.Standard.Responses
{
	public sealed class Nicknames : CommandResponses
	{
		private Nicknames() { }

		public static AdvobotResult RemovedNickname(IGuildUser user)
			=> Success(Default.FormatInterpolated($"Successfully removed the nickname of {user}."));
		public static AdvobotResult ModifiedNickname(IGuildUser user, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the nickname of {user} to {name}."));
		public static AdvobotResult MultiUserAction(int amountLeft)
			=> Success(Default.FormatInterpolated($"Attempting to change the nicknames of {amountLeft} users. ETA on completion: {(int)(amountLeft * 1.2)} seconds."));
		public static AdvobotResult MultiUserActionSuccess(int modified)
			=> Success(Default.FormatInterpolated($"Successfully changed the nicknames of {modified} users."));
	}
}
