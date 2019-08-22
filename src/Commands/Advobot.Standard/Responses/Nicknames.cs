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
	}
}
