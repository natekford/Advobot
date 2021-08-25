
using Discord;

namespace Advobot.Gacha.ActionLimits
{
	public interface ITokenHolderService
	{
		CancellationToken Get(IGuildUser user);
	}
}