using Advobot.Modules;
using Advobot.Utilities;

namespace Advobot.CommandMarking.Responses
{
	public sealed class Nicknames : CommandResponses
	{
		private Nicknames() { }

		public static AdvobotResult ModifiedNickname(string old, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the nickname of {old} to {name}."));
		public static AdvobotResult MultiUserAction(int amountLeft)
			=> Success(Default.FormatInterpolated($"Attempting to change the nicknames of {amountLeft} users. ETA on completion: {(int)(amountLeft * 1.2)} seconds."));
		public static AdvobotResult MultiUserActionSuccess(int modified)
			=> Success(Default.FormatInterpolated($"Successfully changed the nicknames of {modified} users."));
	}
}
