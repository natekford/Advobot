using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Nicknames : AdvobotResult
{
	private Nicknames() : base(null, "")
	{
	}

	public static AdvobotResult ModifiedNickname(IGuildUser user, string name)
	{
		return Success(NicknamesModifiedNickname.Format(
			user.Format().WithBlock(),
			name.WithBlock()
		));
	}

	public static AdvobotResult RemovedNickname(IGuildUser user)
	{
		return Success(NicknamesRemovedNickname.Format(
			user.Format().WithBlock()
		));
	}
}