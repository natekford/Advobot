using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Emotes : AdvobotResult
{
	public static AdvobotResult AddedRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
	{
		return Success(EmotesAddedRequiredRoles.Format(
			roles.Select(x => x.Format()).Join().WithBlock(),
			emote.Format().WithBlock()
		));
	}

	public static AdvobotResult RemoveRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
	{
		return Success(EmotesRemovedRequiredRoles.Format(
			roles.Select(x => x.Format()).Join().WithBlock(),
			emote.Format().WithBlock()
		));
	}
}