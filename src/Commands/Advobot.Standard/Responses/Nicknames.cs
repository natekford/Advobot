using Advobot.Modules;
using Advobot.Utilities;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Nicknames : CommandResponses
	{
		private Nicknames() { }

		public static AdvobotResult RemovedNickname(IGuildUser user)
		{
			return Success(NicknamesRemovedNickname.Format(
				user.Format().WithBlock()
			));
		}
		public static AdvobotResult ModifiedNickname(IGuildUser user, string name)
		{
			return Success(NicknamesModifiedNickname.Format(
				user.Format().WithBlock(),
				name.WithBlock()
			));
		}
		public static AdvobotResult MultiUserActionProgress(int amountLeft)
		{
			return Success(NicknamesMultiUserActionProgress.Format(
				amountLeft.ToString().WithBlock(),
				((int)(amountLeft * 1.2)).ToString().WithBlock()
			));
		}
		public static AdvobotResult MultiUserActionSuccess(int modified)
		{
			return Success(NicknamesMultiUserActionSuccess.Format(
				modified.ToString().WithBlock()
			));
		}
	}
}
