using Advobot.Gacha.Checkers;
using Advobot.Gacha.Database;
using Advobot.Modules;

namespace Advobot.Gacha
{
	public abstract class GachaModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public GachaDatabase Database { get; set; }
		public ICheckersService Checkers { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}
