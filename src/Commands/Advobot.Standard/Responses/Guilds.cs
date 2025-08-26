using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Guilds : AdvobotResult
{
	public static AdvobotResult LeftGuild(IGuild guild)
	{
		return Success(GuildsLeftGuild.Format(
			guild.Format().WithBlock()
		));
	}

	public static AdvobotResult ModifiedOwner(IUser user)
	{
		return Success(GuildsModifiedOwner.Format(
			user.Format().WithBlock()
		));
	}
}