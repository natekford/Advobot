using Advobot.Invites.Service;
using Advobot.Modules;

namespace Advobot.Invites
{
	public abstract class InviteModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public IInviteListService Invites { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}