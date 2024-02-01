using Advobot.Invites.Service;
using Advobot.Modules;

namespace Advobot.Invites;

public abstract class InviteModuleBase : AdvobotModuleBase
{
	public IInviteListService Invites { get; set; } = null!;
}